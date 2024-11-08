<script setup>
	import { get, post } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted } from "vue";
	import { ref } from "vue";
	import Alert from "../components/Alert.vue";
	import BackBtnHeader from "../components/BackBtnHeader.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";

	const userId = ref("");
	const username = ref("");
	const jsonData = ref(null);
	const subheader = ref("");
	const loading = ref(false);
	const showAlert = ref(false);
	const alertMsg = ref({
		header: "",
		message: "",
	});

	onMounted(async () => {
		const user = await getCurrentUser();
		userId.value = user.userId;
		username.value = user.username;
	});

	const closeAlert = () => {
		showAlert.value = false;
	};

	const noAB = ref(false);
	const noBA = ref(false);

	const onSubmit = async () => {
		var requestButton = document.getElementById("request-button");
		requestButton.disabled = true;

		const sender = username.value;
		const recipient = document.getElementById("to-username").value.trim();

		await getFriendship(sender, recipient, noAB);
		await getFriendship(recipient, sender, noBA);


		if (recipient === "") {
			alert("Enter in their username!");
		} else if (recipient === username.value || recipient === userId.value) {
			alert("You can\’t add yourself as a friend, silly!");
		} else {
			if (noAB.value && noBA.value) {
				await sendFriendRequest(sender, recipient);
			} else {
				alert("You're either friends with this person or they already sent you a request, silly!");
			}
		}
		requestButton.disabled = false;
		resetFields();
	};

	function resetFields() {
		document.getElementById("to-username").value = "";
	}

	const getFriendship = async (sender, recipient, noFriendship) => {
		try
		{
			const restOperation = get({
				apiName: "yapp",
				path: `/api/friends/getFriendship?fromUserName=${sender}&toUserName=${recipient}`,
			});
			const { body } = await restOperation.response;
			jsonData.value = await body.json();
			noFriendship.value = false;
		} catch (error) {
			console.log("GET call failed", error);
			noFriendship.value = true;
		}
	}

	const sendFriendRequest = async (fromUser, toUser) => {
		loading.value = true;
		try {
			const newRequest = {
				fromUserName: fromUser,
				toUserName: toUser,
			};

			const sendPostRequest = post({
				apiName: "yapp",
				path: "/api/friends/friendRequest",
				headers: {
					"Content-type": "application/json",
				},
				options: {
					body: newRequest,
				},
			});
			await sendPostRequest.response;

			alertMsg.value.header = "Yipee!";
			alertMsg.value.message = `Friend request sent to ${toUser}!`;
			showAlert.value = true;

		} catch (err) {
			alertMsg.value.header = "Error!";
			alertMsg.value.message = `Please try again!`;
			showAlert.value = true;
			console.error(err);
		}
		loading.value = false;
	};
</script>

<template>
	<LoadingScreen v-if="loading" />

	<div v-else class="backBtnDiv">
		<BackBtnHeader
			header="Add a new Friend!"
			:subheader="subheader"
			:backBtn="true" />
		<br /><br />
		<div class="mt-3 w-full md:mx-6 md:px-16">
			<div class="rounded-xl bg-white p-5">
				<div class="mb-4 flex flex-col">
					<label for="to-username-label" class="mb-5 font-bold">
						Enter their username:
					</label>
					<input class="input" id="to-username" type="text" />
				</div>
				<div class="flex justify-end">
					<button
						class="rounded rounded-lg bg-dark px-4 py-2 font-bold text-white transition-colors hover:bg-white hover:text-dark"
						@click="onSubmit"
						id="request-button">
						Send Request
					</button>
				</div>
			</div>
		</div>

		<Alert
			:showModal="showAlert"
			:header="alertMsg.header"
			:message="alertMsg.message"
			:close="closeAlert" />
	</div>
</template>

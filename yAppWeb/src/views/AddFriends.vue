<script setup>
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted } from "vue";
	import { post } from "aws-amplify/api";
	import { ref } from "vue";
	import Alert from "../components/Alert.vue";
	import BackBtnHeader from "../components/BackBtnHeader.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";

	const userId = ref("");
	const username = ref("");
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

	const onSubmit = async () => {
		const sender = username.value;
		const receiver = document.getElementById("to-username").value.trim();
		var requestButton = document.getElementById("request-button");

		if (receiver === "") {
			alert("Enter in their username!");
			resetFields();
		} else if (receiver === username.value || receiver === userId.value) {
			alert("You can\’t add yourself as a friend, silly!");
			resetFields();
		} else {
			requestButton.disabled = true;
			await sendFriendRequest(sender, receiver);
			requestButton.disabled = false;
			resetFields();
		}
	};

	function resetFields() {
		document.getElementById("to-username").value = "";
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
					<label for="to-username" class="mb-5 font-bold">
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

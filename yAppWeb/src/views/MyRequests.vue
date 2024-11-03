<script setup>
	import { get, put } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted, ref } from "vue";
	import Alert from "../components/Alert.vue";
	import BackBtnHeader from "../components/BackBtnHeader.vue";
	import ConfirmationModal from "../components/ConfirmationModal.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";

	const username = ref("");
	const jsonData = ref([]);
	const loading = ref(false);
	const alertMsg = ref({
		header: "",
		message: "",
	});

	const showAlert = ref(false);
	const showModal = ref(false);
	const message = ref("");
	const currentFriendship = ref(null);
	const openModal = request => {
		message.value = `Are you sure you want to decline ${request.FromUserName}'s friend request?`;
		showModal.value = true;
		currentFriendship.value = request;
	};
	const closeModal = () => {
		showModal.value = false;
		currentFriendship.value = null;
	};
	const closeAlert = () => {
		showAlert.value = false;
	};
	const confirmDecline = async () => {
		await declineRequest(currentFriendship.value);
		showModal.value = false;
		// Update the view of pending requests
		await getRequests();

		currentFriendship.value = null;
	};

	// Get list of friends as JSON
	onMounted(async () => {
		const user = await getCurrentUser();
		username.value = user.username;
		getRequests();
	});

	// Get authenticated user's friend requests
	const getRequests = async () => {
		loading.value = true;
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/friends/getFriendsByStatus?userName=${username.value}&status=0`,
			});
			const { body } = await restOperation.response;
			jsonData.value = await body.json();
		} catch (error) {
			console.log("GET call failed", error);
		}
		loading.value = false;
	};

	// Accept toUser's friend request to authenticated user
	const accept = async request => {
		await acceptRequest(request);
		await getRequests();
	};

	const acceptRequest = async request => {
		loading.value = true;
		try {
			const newRequest = {
				fromUserName: request.FromUserName,
				toUserName: request.ToUserName,
				status: 1,
			};

			const sendPutRequest = put({
				apiName: "yapp",
				path: "/api/friends/updateFriendRequest",
				headers: {
					"Content-type": "application/json",
				},
				options: {
					body: newRequest,
				},
			});
			await sendPutRequest.response;

			(alertMsg.value.header = "Yipee!"),
				(alertMsg.value.message = `You are now friends with ${request.FromUserName}!`);

			showAlert.value = true;
		} catch (err) {
			(alertMsg.value.header = "Error!"),
				(alertMsg.value.message = `Please try again!`);
			console.error(err);
		}
		loading.value = false;
	};

	// Decline toUser's friend request to authenticated user
	const declineRequest = async request => {
		// Close modal
		showModal.value = false;
		loading.value = true;
		try {
			const newRequest = {
				fromUserName: request.FromUserName,
				toUserName: request.ToUserName,
				status: 2,
			};

			const sendPutRequest = put({
				apiName: "yapp",
				path: "/api/friends/updateFriendRequest",
				headers: {
					"Content-type": "application/json",
				},
				options: {
					body: newRequest,
				},
			});
			await sendPutRequest.response;
			(alertMsg.value.header = "Yipee!"),
				(alertMsg.value.message = `Declined ${request.FromUserName}'s friend request!`);
			showAlert.value = true;
		} catch (err) {
			(alertMsg.value.header = "Error!"),
				(alertMsg.value.message = `Please try again!`);
			showAlert.value = true;
			console.error(err);
		}
		loading.value = false;
	};
</script>

<template>
	<LoadingScreen v-if="loading" />

	<div
		v-else
		class="backBtnDiv">
		<BackBtnHeader
			header="My Requests"
			subheader="Here are your pending friend requests!"
			:backBtn="true"
			url="/profile/addFriends"
			btnText="Add a new Friend!" />

		<!-- Show this message if the request list is empty -->
		<div v-if="jsonData.length == 0">
			<h4 class="text-center text-white">
				Wow... you have no friend requests!
			</h4>
		</div>

		<div
			v-else
			class="flex-box py-4 pl-32">
			<div v-for="request in jsonData">
				<div
					class="request bg-deep-dark p-5 text-white"
					v-if="request.FromUserName !== username">
					<h4>{{ request.FromUserName }}</h4>
					<div class="request-actions">
						<button
							class="rounded-lg bg-light-pink p-4 font-bold text-white"
							@click="accept(request)"
							style="margin-right: 10px">
							Accept
						</button>
						<button
							class="rounded-lg bg-light-pink p-4 font-bold text-white"
							@click="openModal(request)">
							Decline
						</button>
					</div>
				</div>
			</div>
			<ConfirmationModal
				:showModal="showModal"
				:close="closeModal"
				:confirm="confirmDecline"
				header="Woah there!"
				:message="message" />
			<Alert
				:showModal="showAlert"
				:header="alertMsg.header"
				:message="alertMsg.message"
				:close="closeAlert" />
		</div>
	</div>
</template>

<style>
	.request {
		display: flex;
		justify-content: space-between;
		background-color: var(--amplify-colors-neutral-10);
		margin-bottom: 15px;
		padding: 10px;
		padding-left: 30px;
		padding-right: 30px;
		border-radius: 5px;
		place-items: center;
	}

	.flex-box {
		flex-direction: column;
	}

	.action-button {
		background-color: rgba(183, 143, 175, 0.577);
		color: var(--amplify-colors-purple-100);
		font-weight: bold;
		float: left;
	}
</style>

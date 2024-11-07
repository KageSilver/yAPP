<script setup>
	import { del, get } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted, ref } from "vue";
	import { useRouter } from "vue-router";
	import Alert from "../components/Alert.vue";
	import ConfirmationModal from "../components/ConfirmationModal.vue";
	import ProfileHeader from "../components/ProfileHeader.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";

	const router = useRouter();
	const username = ref("");
	const jsonData = ref([]);
	const loading = ref(false);

	const showAlert = ref(false);
	const showModal = ref(false);
	const message = ref("");
	const alertMsg = ref({
		header: "",
		message: "",
	});

	const currentFriend = ref(null);
	const currentFriendship = ref(null);

	onMounted(async () => {
		const user = await getCurrentUser();
		username.value = user.username;
		await getFriends();
	});

	// Get user's list of current friends
	const getFriends = async () => {
		loading.value = true;
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/friends/getFriendsByStatus?userName=${username.value}&status=1`,
			});
			const { body } = await restOperation.response;
			jsonData.value = await body.json();
		} catch (error) {
			console.log("GET call failed", error);
		}
		loading.value = false;
	};

	const setModal = friendship => {
		showModal.value = true;
		currentFriendship.value = friendship;

		if (friendship.FromUserName === username.value) {
			currentFriend.value = friendship.ToUserName;
		} else {
			currentFriend.value = friendship.FromUserName;
		}

		message.value = `Are you sure you want to unfollow ${currentFriend.value}?`;
	};

	const setAlert = (header, message) => {
		alertMsg.value.header = header;
		alertMsg.value.message = message;
		showAlert.value = true;
	}

	const closeModal = () => {
		showModal.value = false;
	};

	const closeAlert = () => {
		showAlert.value = false;
	};

	// Triggers when user confirms to unfollow
	const confirmUnfollow = async () => {
		// Wait for deletion to be fully proccessed
		await unfollowFriend(currentFriendship.value);

		closeModal();

		// Update the current list of friends
		await getFriends();
	};

	// Unfollow the chosen friend :(
	const unfollowFriend = async friendship => {
		//set loading screen
		loading.value = true;
		try {
			const deleteRequest = del({
				apiName: "yapp",
				path: `/api/friends/deleteFriendship?fromUsername=${friendship.FromUserName}&toUsername=${friendship.ToUserName}`,
			});
			await deleteRequest.response;
1
			//set alert
			setAlert("Yipee!", "Friendship deleted successfully");

			//send it back to the previous page
			router.push({
				name: "profile",
			});
		} catch (e) {
			console.log("DELETE call failed: ", e);
			setAlert("Oops!", "Failed to delete friendship");
		}
		//disable loading screen
		loading.value = false;
	}
</script>

<template>
	<ProfileHeader />
	<LoadingScreen v-if="loading" />
	<div v-else>
		<!-- Show this message if the friend list is empty -->
		<div v-if="jsonData.length === 0">
			<h4 class="text-center text-white">Wow... you have no friends!</h4>
		</div>

		<!-- Display friend list if available -->
		<div v-else>
			<div
				class="flex-box mt-5 px-16 pr-32"
				v-for="friendship in jsonData"
				:key="friendship.ToUserName || friendship.FromUserName">
				<div class="request bg-deep-dark p-5 text-white">
					<!--Display the friend's username (not the current user)-->
					<h4 v-if="friendship.ToUserName !== username">
						{{ friendship.ToUserName }}
					</h4>
					<h4 v-else>{{ friendship.FromUserName }}</h4>
					<div class="request-actions">
						<button
							class="rounded-lg bg-light-pink p-4 font-bold text-white"
							@click="setModal(friendship)">
							Unfollow
						</button>
					</div>
				</div>
				<ConfirmationModal
					:showModal="showModal"
					:close="closeModal"
					:confirm="confirmUnfollow"
					header="Woah there!"
					:message="message" />
				<Alert
					:showModal="showAlert"
					:header="alertMsg.header"
					:message="alertMsg.message"
					:close="closeAlert" />
			</div>
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

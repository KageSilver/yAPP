<script setup>
	import { get, put } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted, ref } from "vue";
	import ConfirmationModal from "../components/ConfirmationModal.vue";
	import ProfileHeader from "../components/ProfileHeader.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";

	const username = ref("");
	const jsonData = ref([]);
	const loading = ref(false);

	// Get list of friends as JSON
	onMounted(async () => {
		const user = await getCurrentUser();
		username.value = user.username;
		getFriends();
	});

	// Get authenticated user's list of current friends
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

	const showModal = ref(false);
	const currentFriend = ref(null);
	const message = ref("");
	const currentFriendship = ref(null);

	const openModal = friendship => {
		showModal.value = true;
		currentFriendship.value = friendship;

		if (friendship.FromUserName === username.value) {
			currentFriend.value = friendship.ToUserName;
		} else {
			currentFriend.value = friendship.FromUserName;
		}

		message.value = `Are you sure you want to unfollow ${currentFriend.value}?`;
	};

	const closeModal = () => {
		showModal.value = false;
	};

	const confirmUnfollow = async () => {
		// Wait for unfollowing friend to be fully proccessed
		await unfollowFriend(currentFriendship.value);
		closeModal();
		// Update the current list of friends
		await getFriends();
	};

	// Unfollow sent friend
	const unfollowFriend = async friendship => {
		loading.value = true;
		try {
			const newRequest = {
				fromUserName: friendship.FromUserName,
				toUserName: friendship.ToUserName,
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
		} catch (err) {
			console.error(err);
		}
		loading.value = false;
	};
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
					<h4 v-if="friendship.ToUserName !== username">
						{{ friendship.ToUserName }}
					</h4>
					<h4 v-else>{{ friendship.FromUserName }}</h4>
					<div class="request-actions">
						<button
							class="rounded-lg bg-light-pink p-4 font-bold text-white"
							@click="openModal(friendship)">
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

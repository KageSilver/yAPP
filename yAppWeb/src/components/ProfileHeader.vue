<script setup>
	import { get, put } from "aws-amplify/api";
	import { ref, onMounted } from "vue";
	import { getCurrentUser } from "aws-amplify/auth";
	import { useRoute } from "vue-router";
	import { useRouter } from "vue-router";
	import { watch } from "vue";

	const router = useRouter(); // Use router hook

	const username = ref(""); // Reacted variable to hold the username
	const userId = ref(""); // Reacted variable to hold the userId
	const jsonData = ref([]); // Reacted array to hold the list of friendships
	const counts = ref(0); // Reacted variable to hold the number of friend requests
	const route = useRoute(); // This composable provides access to the current route object

	// Function to determine if the current route's path matches the given path
	const isActive = path => {
		return route.path.includes(path);
	};
	const navigateTo = event => {
		const selectedRoute = event.target.value;
		router.push(selectedRoute); // Navigate to the selected route
	};

	const selectedTab = ref("/profile/myPosts");

	// Get list of friends as JSON
	onMounted(async () => {
		getRequests();
		const user = await getCurrentUser();
		username.value = user.username;
		userId.value = user.userId;
		//update selected tab
		selectedTab.value = route.path;
	});

	// Get authenticated user's friend requests
	async function getRequests() {
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/friends/getFriendsByStatus?userName=${username}&status=0`,
			});
			const { body } = await restOperation.response;
			const response = await (await body.blob()).arrayBuffer();
			const decoder = new TextDecoder("utf-8"); // Use TextDecoder to decode the ArrayBuffer to a string
			const decodedText = decoder.decode(response);
			jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
			counts.value = jsonData.value.length;
			console.log(jsonData);
		} catch (error) {
			console.log("GET call failed", error);
		}
	}
</script>

<template>
	<div class="relative mt-[8rem] w-full bg-light-pink pl-8 pt-[8rem]">
		<h5 class="mb-2 text-3xl font-bold text-white">
			Welcome back, {{ username }}!
		</h5>
		<!-- Button positioned at the top-right corner -->
		<button
			class="hover:bg-dark-pink-800 absolute right-2 top-0 mr-16 mt-2 rounded-lg px-4 py-2 font-bold text-white"
			@click="router.push('/profile/friendRequests')">
			<span class="material-icons">group_add</span>
			<!-- Notification Badge -->
			<span
				class="absolute -right-1 top-1 flex h-5 w-5 items-center justify-center rounded-full bg-dark-pink text-xs font-bold text-white"
				v-if="counts">
				{{ counts }}
			</span>
		</button>
	</div>

	<div class="mt-0 w-full bg-dark-purple pb-[5rem] pl-8 pt-2">
		<h5 class="mb-2 text-sm text-purple">UUID: {{ userId }}</h5>
	</div>
	<div class="sm:hidden">
		<label for="tabs" class="sr-only">Select tab</label>
		<select
			id="tabs"
			v-model="selectedTab"
			@change="navigateTo($event)"
			class="block w-full border-none bg-white p-2.5 text-sm text-gray-900 focus:ring-2 focus:ring-indigo-500">
			<option value="/profile/myPosts">My Posts</option>
			<option value="/profile/friends">My Friends</option>
			<option value="/profile/awards">My Awards</option>
		</select>
	</div>
	<ul
		class="hidden text-center text-sm font-medium text-gray-900 sm:flex"
		id="fullWidthTab"
		data-tabs-toggle="#fullWidthTabContent"
		role="tablist">
		<li class="w-full">
			<button
				@click="router.push('/profile/myPosts')"
				:class="[
					'inline-block w-full bg-white p-4 hover:bg-dark-purple hover:text-white focus:outline-none',
					isActive('/myPosts') ? 'text-light-pink' : '',
				]">
				My Posts
			</button>
		</li>
		<li class="w-full">
			<button
				@click="router.push('/profile/friends')"
				:class="[
					'inline-block w-full bg-white p-4 hover:bg-dark-purple hover:text-white focus:outline-none',
					isActive('/friends') ? 'text-light-pink' : '',
				]">
				My Friends
			</button>
		</li>
		<li class="w-full">
			<button
				@click="router.push('/profile/awards')"
				:class="[
					'inline-block w-full bg-white p-4 hover:bg-dark-purple hover:text-white focus:outline-none',
					isActive('/awards') ? 'text-light-pink' : '',
				]">
				My Awards
			</button>
		</li>
	</ul>
</template>

<style scoped></style>

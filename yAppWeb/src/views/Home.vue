<script setup>
	import { useRouter } from "vue-router"; // Import useRouter
	import { usePostHelper } from "../composables/usePostHelper"; // Import the helper
	import PostCard from "../components/PostCard.vue"; // Import the PostCard component

	const router = useRouter(); // Use router hook
	const maxResults = 10; // Default is 10
	const currentDateTime = new Date(); // Setting for on creation
	const since = currentDateTime.toJSON();
	// Retrieve the necessary data and function from the helper
	const { jsonData, loading, getPosts, updatePath } = usePostHelper(
		`/api/posts/getRecentPosts?since=${since}&maxResults=${maxResults}`,
	);
	console.log(jsonData);

	var dates = new Array();
	var currentDatePosition = 0;
	dates.push(since);

	function clickPost(pid) {
		router.push({ name: "details", params: { pid } });
	}

	function pageForwards() {
		var lastPostTime = getLastPostTime();
		if (lastPostTime != null && jsonData.value.length == 10) {
			document.getElementById("pageBackwards").style.visibility = "visible";
			var since = new Date(lastPostTime);
			since = since.toJSON();
			dates.push(since);
			currentDatePosition++;
			loading.value = true;
			updatePath(
				`/api/posts/getRecentPosts?since=${since}&maxResults=${maxResults}`,
			);
			getPosts();
		} else {
			document.getElementById("pageForwards").style.visibility = "hidden";
		}
	}

	function pageBackwards() {
		if (currentDatePosition != 0) {
			document.getElementById("pageForwards").style.visibility = "visible";
			loading.value = true;
			currentDatePosition--;
			updatePath(
				`/api/posts/getRecentPosts?since=${dates.at(currentDatePosition)}&maxResults=${maxResults}`,
			);
			dates.pop();
			getPosts();
		} else {
			document.getElementById("pageBackwards").style.visibility = "hidden";
		}
	}

	function getLastPostTime() {
		var holder = null;
		if (jsonData.value.length > 0) {
			var lastPost = jsonData.value[jsonData.value.length - 1];
			holder = lastPost.createdAt;
		}
		return holder;
	}
</script>

<template>
	<div class="relative w-full items-center justify-center pl-8 pt-[8rem]">
		<div
			v-if="loading"
			class="flex items-center justify-center">
			<div
				class="spinner h-10 w-10 animate-spin rounded-full border-4 border-t-transparent"></div>
		</div>
		<div
			v-else
			class="flex w-full flex-col items-center">
			<div
				class="card m-2 w-full max-w-4xl cursor-pointer rounded-lg border border-gray-300 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md"
				v-for="post in jsonData"
				:key="post.pid"
				@click="clickPost(post.pid)">
				<PostCard :post="post" />
			</div>
			<button
				id="pageBackwards"
				class="add-margin self-center rounded bg-blue-500 px-4 py-2 font-bold text-white hover:bg-blue-600"
				type="button"
				@click="pageBackwards()">
				Go back
			</button>
			<button
				id="pageForwards"
				class="self-center rounded bg-blue-500 px-4 py-2 font-bold text-white hover:bg-blue-600"
				type="button"
				@click="pageForwards()">
				Load more!
			</button>
		</div>
	</div>
</template>

<style scoped>
	@keyframes spin {
		from {
			transform: rotate(0deg);
		}

		to {
			transform: rotate(360deg);
		}
	}

	.add-margin {
		margin-bottom: 5px;
	}
</style>

<script setup>
	import { get } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted, ref } from "vue";
	import { useRouter } from "vue-router";
	import LoadingScreen from "../components/LoadingScreen.vue";
	import PostCard from "../components/PostCard.vue";
	import ProfileHeader from "../components/ProfileHeader.vue";

	const router = useRouter(); // Use router hook
	const uid = ref("");
	const jsonData = ref([]);
	const loading = ref(false);

	// Retrieve the necessary data and function from the helper
	onMounted(async () => {
		const user = await getCurrentUser();
		uid.value = user.userId;
		await getPosts(uid);
	});

	async function getPosts(uid) {
		loading.value = true;
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/posts/getPostsByUser?uid=${uid.value}`,
			});
			const { body } = await restOperation.response;
			jsonData.value = await body.json();
			sortPosts();
		} catch (error) {
			console.log("GET call failed", error);
		}
		loading.value = false;
	}

	function clickPost(pid) {
		router.push({ name: "details", params: { pid } });
	}

	function sortPosts() {
		if (Array.isArray(jsonData.value))
			jsonData.value.sort(
				(a, b) => new Date(b.createdAt) - new Date(a.createdAt),
			);
	}
</script>

<template>
	<ProfileHeader />
	<LoadingScreen v-if="loading" />
	<div
		v-else
		class="mx-auto flex w-full flex-col items-center">
		<div
			class="card m-2 w-full max-w-4xl cursor-pointer rounded-lg border border-gray-300 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md"
			v-for="post in jsonData"
			:key="post.pid"
			@click="clickPost(post.pid)">
			<PostCard :post="post" />
		</div>
	</div>
</template>

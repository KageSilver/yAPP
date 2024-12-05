<script setup>
	import { get } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { useRouter } from "vue-router";
	import { ref, onMounted } from "vue";
	import ProfileHeader from "../components/ProfileHeader.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";

	const router = useRouter(); // Use router hook
	const uid = ref("");
	const awards = ref([]);
	const newAwards = ref([]);
	const loading = ref(false);

	// Retrieve the necessary data and function from the helper
	onMounted(async () => {
		const user = await getCurrentUser();
		uid.value = user.userId;
		await getAwards(uid);
		await getNewAwards(uid);
	});

	async function getAwards(uid) {
		loading.value = true;
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/awards/getAwardsByUser?uid=${uid.value}`,
			});
			const { body } = await restOperation.response;
			awards.value = await body.json();
		} catch (error) {
			console.log("GET call failed", error);
		}
		loading.value = false;
	}

	async function getNewAwards() {
		try {
			const restOperation = get({
				apiName: "yapp",
				path: `/api/awards/getNewAwardsByUser?uid=${uid.value}`,
			});
			const { body } = await restOperation.response;
			newAwards.value = await body.json();
		} catch (error) {
			console.log("GET call failed", error);
		}
		loading.value = false;
	}
</script>

<template>
	<ProfileHeader />
	<LoadingScreen v-if="loading" />
	<div class="mx-auto flex w-full flex-col items-center">
		<div
			class="card m-2 w-full max-w-4xl cursor-pointer rounded-lg border border-gray-500 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md"
			v-for="award in newAwards"
			:key="award.aid"
		>
			<div class="card-header mb-2">
				<h3 class="truncate text-lg font-semibold">{{ award.name }}</h3>
				<p
					class="overflow-hidden overflow-ellipsis whitespace-nowrap text-sm text-gray-600"
				>
					<strong>Created At:</strong>
					{{ new Date(award.createdAt).toLocaleString() }}
				</p>
			</div>
			<div class="card-body">
				<p
					class="overflow-hidden text-ellipsis whitespace-nowrap text-gray-700"
				>
					{{ award.type }} award: tier {{ award.tier }}
				</p>
			</div>
		</div>
	</div>

	<div class="mx-auto flex w-full flex-col items-center">
		<div
			class="card m-2 w-full max-w-4xl cursor-pointer rounded-lg border border-gray-500 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md"
			v-for="award in awards"
			:key="award.aid"
		>
			<div class="card-header mb-2">
				<h3 class="truncate text-lg font-semibold">{{ award.name }}</h3>
				<p
					class="overflow-hidden overflow-ellipsis whitespace-nowrap text-sm text-gray-600"
				>
					<strong>Created At:</strong>
					{{ new Date(award.createdAt).toLocaleString() }}
				</p>
			</div>
			<div class="card-body">
				<p
					class="overflow-hidden text-ellipsis whitespace-nowrap text-gray-700"
				>
					{{ award.type }} award: tier {{ award.tier }}
				</p>
			</div>
		</div>
	</div>
</template>

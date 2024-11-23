<script setup lang="js">
	import SideNav from "../components/SideNav.vue";
	import TopNav from "../components/TopNav.vue";
	import { computed } from "vue";
	import { useRoute } from "vue-router";

	const route = useRoute();
	const currentPath = computed(() => route.path.includes("profile"));
	console.log(currentPath.value);
</script>

<template>
	<div class="flex h-screen overflow-hidden bg-hero-gradient">
		<!-- Sidebar -->
		<div class="">
			<SideNav />
		</div>

		<!-- Main content area -->
		<div class="flex flex-1 flex-col">
			<TopNav />
			<main
				:class="`flex min-h-screen flex-1 flex-col overflow-y-auto ${currentPath.value ? 'justify-center py-[22rem]' : ''}`">
				<router-view></router-view>
			</main>
		</div>
	</div>
</template>

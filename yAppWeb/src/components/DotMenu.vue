<script setup>
	import { ref, defineProps, defineEmits } from "vue";

	// Define props
	const props = defineProps({});

	// Emit events for edit and delete actions
	const emit = defineEmits(["edit", "openDeleteModal"]);

	// State to track if the dropdown menu is open
	const isMenuOpen = ref(false);

	// Method to toggle the dropdown menu
	const toggleMenu = () => {
		isMenuOpen.value = !isMenuOpen.value;
	};

	// Methods to emit actions
	const edit = () => {
		emit("edit");
		isMenuOpen.value = false; // Close the menu after action
	};

	const openDeleteModal = () => {
		emit("openDeleteModal");
		isMenuOpen.value = false; // Close the menu after action
	};
</script>

<template>
	<div class="absolute right-0 top-0">
		<button
			@click="toggleMenu"
			class="text-gray-600 hover:text-gray-900 focus:outline-none">
			<svg
				class="h-10 w-10"
				fill="none"
				stroke="currentColor"
				viewBox="0 0 24 24"
				xmlns="http://www.w3.org/2000/svg">
				<path
					stroke-linecap="round"
					stroke-linejoin="round"
					stroke-width="2"
					d="M12 6v.01M12 12v.01M12 18v.01"></path>
			</svg>
		</button>

		<!-- Dropdown Menu -->
		<div
			v-if="isMenuOpen"
			class="absolute right-0 z-10 mt-2 w-24 rounded border border-gray-200 bg-white text-center shadow-lg">
			<a
				href="#"
				@click="edit"
				class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
				>Edit</a
			>
			<a
				href="#"
				@click="openDeleteModal"
				class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
				>Delete</a
			>
		</div>
	</div>
</template>

<style scoped>
	/* Add any custom styles here if needed */
</style>

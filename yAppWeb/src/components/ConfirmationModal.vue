<script setup>
	import { defineProps } from "vue";

	// Define the props for the component
	const props = defineProps({
		showModal: {
			type: Boolean,
			required: true,
		},
		close: {
			type: Function,
			required: true,
		},
		confirm: {
			type: Function,
			required: true,
		},
		header: {
			type: String,
			default: "",
		},
		message: {
			type: String,
			default: "",
		},
	});
</script>

<template>
	<!-- Show the modal only if showDeleteModal is true -->
	<div
		v-if="showModal"
		class="fixed inset-0 z-50 flex items-center justify-center bg-gray-800 bg-opacity-75"
	>
		<div class="mx-auto max-w-lg rounded-lg bg-white p-6">
			<h3 class="mb-4 text-lg font-bold">{{ header }}</h3>
			<p>{{ message }}</p>
			<div class="mt-4 flex justify-end">
				<button
					@click="close"
					class="mr-2 rounded bg-light-pink px-4 py-2 text-white"
				>
					Cancel
				</button>
				<button
					@click="confirm"
					class="rounded bg-dark-pink px-4 py-2 text-white"
				>
					Confirm
				</button>
			</div>
		</div>
	</div>
</template>

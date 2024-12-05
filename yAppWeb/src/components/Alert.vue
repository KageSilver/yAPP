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
	<div class="absolute bottom-20 left-10 w-[25rem]" v-if="showModal">
		<div
			id="alert-border-5"
			class="flex flex-col rounded-xl border-t-4 border-gray-300 bg-white p-4"
			role="alert">
			<div class="flex w-full items-center justify-between">
				<!-- Icon and Header on the same line -->
				<div class="flex items-center">
					<svg
						class="h-4 w-4 flex-shrink-0 bg-white text-dark"
						aria-hidden="true"
						xmlns="http://www.w3.org/2000/svg"
						fill="currentColor"
						viewBox="0 0 20 20">
						<path
							d="M10 .5a9.5 9.5 0 1 0 9.5 9.5A9.51 9.51 0 0 0 10 .5ZM9.5 4a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3ZM12 15H8a1 1 0 0 1 0-2h1v-3H8a1 1 0 0 1 0-2h2a1 1 0 0 1 1 1v4h1a1 1 0 0 1 0 2Z" />
					</svg>
					<!-- Header -->
					<h4 class="ms-2 text-lg font-bold text-gray-900">
						{{ props.header }}
					</h4>
				</div>
				<!-- Close Button on the same line as Header -->
				<button
					type="button"
					class="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-white p-1.5 text-gray-500 hover:bg-gray-200 focus:ring-2 focus:ring-gray-400"
					data-dismiss-target="#alert-border-5"
					aria-label="Close"
					@click="close">
					<span class="sr-only">Dismiss</span>
					<svg
						class="h-3 w-3"
						aria-hidden="true"
						xmlns="http://www.w3.org/2000/svg"
						fill="none"
						viewBox="0 0 14 14">
						<path
							stroke="currentColor"
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="m1 1 6 6m0 0 6 6M7 7l6-6M7 7l-6 6" />
					</svg>
				</button>
			</div>
			<!-- Subtext -->
			<div
				class="ml-8 mt-2 text-sm font-medium text-gray-800 dark:text-gray-300">
				<p class="text-gray-600">{{ props.message }}</p>
			</div>
		</div>
	</div>
</template>

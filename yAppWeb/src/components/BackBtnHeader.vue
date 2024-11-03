<script setup>
	import BackBtn from "./BackBtn.vue";
	const props = defineProps({
		header: String,
		subheader: String,
		backBtn: Boolean,
		url: String,
		btnText: String,
	});
</script>
<template>
	<div class="flex w-full flex-col pl-16">
		<div class="mb-0 flex items-center justify-between">
			<div class="flex items-center">
				<BackBtn v-if="backBtn" />
				<h1 class="mb-0 ml-8 text-4xl font-bold text-white">{{ header }}</h1>
			</div>
			<a
				class="rounded bg-deep-dark px-4 py-2 text-2xl font-bold text-white hover:bg-purple"
				v-if="btnText"
				:href="url">
				{{ btnText }}
			</a>
		</div>
		<p
			:class="`m-0 mt-2 text-sm font-bold text-white ${backBtn ? 'ml-20' : 'ml-8'}`">
			{{ subheader }}
		</p>
	</div>
</template>

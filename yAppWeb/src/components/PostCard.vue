<script setup>
	import { getCurrentUser } from "aws-amplify/auth";
	import { defineProps, onMounted, ref } from "vue";
	const props = defineProps({
		post: {
			type: Object,
			required: true,
			default: () => ({
				postTitle: "",
				createdAt: "",
				postBody: "",
				uid: "",
				pid: "",
			}),
		},
	});
	const isMenuOpen = ref(false);
	// function to get the current user , if we need to edit the post?
	// diplay the 3 dot in the right corner of the post card
	const currentUser = ref(null);
	onMounted(async () => {
		const user = await getCurrentUser();
		currentUser.value = user.username;
	});

	const toggleMenu = () => {
		isMenuOpen.value = !isMenuOpen.value;
	};
</script>
<template>
	<div>
		<div class="card-header relative mb-2">
			<h3 class="truncate text-lg font-semibold">{{ props.post.postTitle }}</h3>
			<p
				class="overflow-hidden overflow-ellipsis whitespace-nowrap text-sm text-gray-600">
				<strong>Created At:</strong>
				{{ new Date(props.post.createdAt).toLocaleString() }}
			</p>

			<!-- Three dot menu (Dropdown) -->

			<div
				class="absolute right-0 top-0"
				v-if="currentUser == props.post.userName">
				<button
					class="text-gray-600 hover:text-gray-900 focus:outline-none"
					@click="toggleMenu">
					<svg
						class="h-6 w-6"
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
			</div>
		</div>

		<div class="card-body">
			<p class="overflow-hidden text-ellipsis whitespace-nowrap text-gray-700">
				{{ props.post.postBody }}
			</p>
		</div>

		<!-- Icons for upvote, downvote, and reply -->
		<!-- <div class="flex space-x-4 mt-8">
            <button @click.stop="upvote(post.pid)">
                <img src="../assets/post/upvote.svg" alt="Upvote" class="w-6 h-6">
                10
            </button>
            <button @click.stop="downvote(post.pid)">
                <img src="../assets/post/downvote.svg" alt="Downvote" class="w-6 h-6">
                20
            </button>
        </div> -->
	</div>
</template>

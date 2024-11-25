<script setup>
	import {
		getCurrentUser
	} from "aws-amplify/auth";
	import {
		defineProps,
		onMounted,
		ref,
		computed
	} from "vue";
	import {
		get,
		post,
		del
	} from "aws-amplify/api";
	const myPostVotes = ref([]);
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
				upvotes: 0,
				downvotes: 0,
			}),
		},
	});
	const isMenuOpen = ref(false);
	// function to get the current user , if we need to edit the post?
	// diplay the 3 dot in the right corner of the post card
	const currentUser = ref(null);
	const userId = ref(null);
	onMounted(async () => {
		const user = await getCurrentUser();
		console.log("user", user);
		currentUser.value = user.username;
		userId.value = user.userId;
		myPostVotes.value = await getVotes(props.post.pid);
	});
	const isUpvotePost = computed(() => {
		//check if the user has upvoted the post
		const voted = ref(false);

		voted.value = myPostVotes.value.filter(vote => vote.type == true).length > 0;

		return voted;
	});

	const isDownvotePost = computed(() => {
		//check if the user has downvoted the post
		const voted = ref(false);
		voted.value = myPostVotes.value.filter(vote => vote.type == false).length > 0;

		return voted;
	});


	const toggleMenu = () => {
		isMenuOpen.value = !isMenuOpen.value;
	};

	const getVotes = async (pid) => {
		const restOperation = get({
			apiName: "yapp",
			path: `/api/votes/getVotesByPid?pid=${pid}`,
		});
		const {
			body: body
		} = await restOperation.response;
		var votes = await body.json();
		return votes
	};

	const vote = async (pid, isPost, isUpVote, currentValue) => {
	
		const body = ref({});
		body.value = {
			uid: userId.value,
			pid: pid,
			type: isUpVote,
			isPost: isPost,
		};
		console.log("currentValue", currentValue.value);

		if (currentValue.value == false) {
			console.log("adding vote");
			const restOperation = post({
				apiName: "yapp",
				path: `/api/votes/addVote`,
				headers: {
					"Content-Type": "application/json",
				},
				options: {
					body: body.value,
				},
			});
			await restOperation.response;
		} else {
			const restOperation = del({
				apiName: "yapp",
				path: `/api/votes/removeVote?uid=${userId.value}&pid=${pid}&isPost=${isPost}&type=${isUpVote}`,
			});
			await restOperation.response;
		}

		//refresh the votes
		window.location.reload();
	};
</script>
<template>
	<div>
		<div class="card-header relative mb-2">
			<h3 class="truncate text-lg font-semibold">{{ props.post.postTitle }}</h3>
			<p class="overflow-hidden overflow-ellipsis whitespace-nowrap text-sm text-gray-600">
				<strong>Created At:</strong>
				{{ new Date(props.post.createdAt).toLocaleString() }}
			</p>

			<!-- Three dot menu (Dropdown) -->

			<div class="absolute right-0 top-0" v-if="currentUser == props.post.userName">
				<button class="text-gray-600 hover:text-gray-900 focus:outline-none" @click="toggleMenu">
					<svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"
						xmlns="http://www.w3.org/2000/svg">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
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
		<div class="flex space-x-4 mt-8">
			<!-- Upvote -->
			<!-- Upvote Button -->
			<button @click.stop="vote(props.post.pid, true, true, isUpvotePost)"
				class="relative flex rounded-xl items-center p-2 hover:bg-light-pink hover:text-transparent disabled:hover:bg-none disabled:hover:text-current disabled:opacity-50 disabled:cursor-not-allowed"
				:disabled="isDownvotePost.value">
				<span class="upvotes top-0" v-if="props.post.upvotes > 0">
					{{ props.post.upvotes }}
				</span>
				<img src="../assets/post/upvote.svg" alt="Upvote" class="w-5 h-5" v-if="!isUpvotePost.value" />
				<img src="../assets/post/upvote_activated.svg" alt="Upvote" class="w-5 h-5" v-else />
			</button>

			<!-- Downvote Button -->
			<button @click.stop="vote(props.post.pid, true, false, isDownvotePost)"
				class="relative flex rounded-xl items-center p-2 hover:bg-light-pink hover:text-transparent disabled:hover:bg-none disabled:hover:text-current disabled:opacity-50 disabled:cursor-not-allowed"
				:disabled="isUpvotePost.value">
				<span class="downvotes top-0" v-if="props.post.downvotes > 0">
					{{ props.post.downvotes }}
				</span>
				<img src="../assets/post/downvote.svg" alt="Downvote" class="w-5 h-5" v-if="!isDownvotePost.value" />
				<img src="../assets/post/downvote_activated.svg" alt="Downvote" class="w-5 h-5" v-else />
			</button>

		</div>
	</div>
</template>
<style scoped>
	.upvotes {
		@apply absolute right-0 text-xs;
	}

	.downvotes {
		@apply absolute right-0 text-xs;
	}
</style>
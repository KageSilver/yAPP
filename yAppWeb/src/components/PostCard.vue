<script setup>
	import {
	del,
	get,
	post
} from "aws-amplify/api";
import {
	getCurrentUser
} from "aws-amplify/auth";
import {
	computed,
	defineProps,
	onMounted,
	ref,
	watch
} from "vue";

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
				username: "",
			}),
		},
		isDiary : {
			type: Boolean,
			required: false,
			default: false
		}
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
		//filter out the votes for the current user
		myPostVotes.value = myPostVotes.value.filter(vote => vote.uid == userId.value);
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
		document.getElementById("upBtn_" + pid).disabled = true;
		document.getElementById("downBtn_" + pid).disabled = true;
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
			if (isUpVote) {
				props.post.upvotes  += 1;
			} else { 
				props.post.downvotes += 1;
			}

		
		} else {
			const restOperation = del({
				apiName: "yapp",
				path: `/api/votes/removeVote?uid=${userId.value}&pid=${pid}&isPost=${isPost}&type=${isUpVote}`,
			});
			await restOperation.response;
			if (isUpVote) {
				props.post.upvotes  -= 1;
			} else { 
				props.post.downvotes -= 1;
			}

		}
		//fetch current post
		document.getElementById("upBtn_" + pid).disabled = false;
		document.getElementById("downBtn_" + pid).disabled = false;

		//refresh the votes
		//update the current post 
	
		myPostVotes.value = await getVotes(pid);
		myPostVotes.value = myPostVotes.value.filter(vote => vote.uid == userId.value);
		
		await fetchPost(props.post.pid);
	};


	watch(myPostVotes, (newValue, oldValue) => {
		isUpvotePost.value = myPostVotes.value.filter(vote => vote.type == true).length > 0;
		isDownvotePost.value = myPostVotes.value.filter(vote => vote.type == false).length > 0;
		//update the 
    // Do something when the value changes (e.g., update the UI or call a function)
	});



</script>
<template>
	<div>
		<div class="card-header relative mb-2" :id="'post_' + props.post.pid">
			<h3 class="truncate text-lg font-semibold">{{ props.post.postTitle }}</h3>
			<p class="overflow-hidden overflow-ellipsis whitespace-nowrap text-sm text-gray-600">
				<strong>Created At:</strong>
				{{ new Date(props.post.createdAt).toLocaleString() }}
			</p>
			<p class="overflow-hidden overflow-ellipsis whitespace-nowrap text-sm text-gray-600"
				v-if="isDiary == true && userId == props.post.uid"
			>
				<strong>Posted By:</strong> You
			</p>
			<p class="overflow-hidden overflow-ellipsis whitespace-nowrap text-sm text-gray-600"
				v-else-if="isDiary == true && userId != props.post.uid"
			>
				<strong>Posted By:</strong> {{ props.post.username }}
			</p>

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
			<button @click.stop="vote(props.post.pid, true, true, isUpvotePost)" :id="'upBtn_' + props.post.pid"
				class="relative flex rounded-xl items-center p-2 hover:bg-light-pink hover:text-transparent disabled:hover:bg-none disabled:hover:text-current disabled:opacity-50 disabled:cursor-not-allowed"
				:disabled="isDownvotePost.value">
				<span class="upvotes top-0" v-if="props.post.upvotes > 0">
					{{ props.post.upvotes }}
				</span>
				<img src="../assets/post/upvote.svg" alt="Upvote" class="w-5 h-5" v-if="!isUpvotePost.value" />
				<img src="../assets/post/upvote_activated.svg" alt="Upvote" class="w-5 h-5" v-else />
			</button>

			<!-- Downvote Button -->
			<button @click.stop="vote(props.post.pid, true, false, isDownvotePost)"  :id="'downBtn_' + props.post.pid"
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
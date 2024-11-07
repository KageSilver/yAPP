<script setup>
	import { del, get, post, put } from "aws-amplify/api";
	import { getCurrentUser } from "aws-amplify/auth";
	import { onMounted, ref } from "vue";
	import { useRoute, useRouter } from "vue-router";
	import Alert from "../components/Alert.vue";
	import BackBtn from "../components/BackBtn.vue";
	import ConfirmationModal from "../components/ConfirmationModal.vue";
	import DotMenu from "../components/DotMenu.vue";
	import LoadingScreen from "../components/LoadingScreen.vue";
	import { getCurrentTime } from "../composables/helper";

	const route = useRoute();
	const router = useRouter();

	// State
	const currentPost = ref(null); // Stores the current post details

	// Loading and alert management
	const showAlert = ref(false); // Controls the visibility of the alert
	const loading = ref(false); // Controls the loading state
	const alertMsg = ref({
		header: "",
		message: "",
	}); // Stores alert messages

	// Post editing and deleting states
	const isEditing = ref(false); // Controls the visibility of the edit form
	const isDeleting = ref(false); // Controls the visibility of the delete confirmation modal
	const isDiscardingUpdate = ref(false); // Controls the visibility of the discard confirmation modal
	const anonIsChecked = ref(true);

	// Comment management states
	const isAddingComment = ref(false); // Controls the visibility of the add comment form
	const isDeletingComment = ref(false); // Controls the visibility of the delete comment modal
	const commentIds = ref([]); // Tracks the IDs of comments being edited
	const commentToDelete = ref(null); // Stores the comment being deleted
	const commentMsg = ref(""); // Stores the comment message
	const comments = ref([]); // List of comments
	const userId = ref(""); // Stores the user ID

	// Post functions

	// Handles the start of post editing
	const editPost = () => {
		isEditing.value = true;
		isDiscardingUpdate.value = false;

		console.log(currentPost.value);

		if (currentPost.value.diaryEntry) {
			document.getElementById("anonymous").hidden = false;
			anonIsChecked.value = currentPost.value.anonymous;
		}
	};

	// Opens the delete post modal
	const openDeleteModal = () => {
		isDeleting.value = true;
	};

	// Closes the delete post modal
	const closeDeleteModal = () => {
		isDeleting.value = false;
	};

	// Opens the discard post changes modal
	const openDiscardModal = () => {
		isDiscardingUpdate.value = true;
	};

	// Closes the discard post changes modal
	const closeDiscardModal = () => {
		isDiscardingUpdate.value = false;
	};

	// Confirms discarding post changes
	const confirmDiscard = () => {
		isEditing.value = false;
		isDiscardingUpdate.value = false;
	};

	// Closes the alert component
	const closeAlert = () => {
		showAlert.value = false;
	};

	// Comment functions

	// Edits a comment by adding it to the list of editable comments
	const editComment = comment => {
		commentIds.value.push(comment.cid);
	};

	const confirmDeleteComment = async () => {
		isDeletingComment.value = false;
		await deleteComment(commentToDelete.value);
		await fetchComments(currentPost.value.pid);
		//reset the commentToDelete
		commentToDelete.value = null;
	};

	// Deletes a comment (can integrate API call here)
	const deleteComment = async comment => {
		loading.value = true;
		try {
			const deleteRequest = del({
				apiName: "yapp",
				path: `/api/comments/deleteComment?cid=${comment.cid}`,
			});
			await deleteRequest.response;
			setAlert("Yipee!", "Comment deleted successfully");
		} catch (e) {
			console.log("DELETE call failed: ", e);
			setAlert("Oops!", "Failed to delete comment");
		}
		loading.value = false;
	};

	// Opens the delete comment confirmation modal
	const openDeleteCommentModal = comment => {
		isDeletingComment.value = true;
		commentToDelete.value = comment;
		commentMsg.value = comment.commentBody;
	};

	// Closes the delete comment confirmation modal
	const closeDeleteCommentModal = () => {
		isDeletingComment.value = false;
	};

	// Cancels comment editing by removing its ID from the edit list
	const cancelEditComment = comment => {
		commentIds.value = commentIds.value.filter(id => id !== comment.cid);
	};

	// Adds a reply to a comment
	const reply = () => {
		isAddingComment.value = true;
	};

	// Cancels the reply action
	const cancelReply = () => {
		isAddingComment.value = false;
	};

	// Checks if a comment is currently being edited
	const isEditingComment = comment => {
		return ref(commentIds.value.includes(comment.cid)).value;
	};

	onMounted(async () => {
		const pid = route.params.pid;
		const user = await getCurrentUser();
		userId.value = user.userId;
		//set loading screen
		loading.value = true;
		//fetch the post
		await fetchPost(pid);
		//fetch the comments
		await fetchComments(pid);
	});

	const fetchPost = async pid => {
		//set loading screen
		loading.value = true;
		try {
			const restOperation = await get({
				apiName: "yapp",
				path: `/api/posts/getPostById?pid=${pid}`,
			});
			const { body } = await restOperation.response;
			const response = await body.json();
			currentPost.value = response;
		} catch (error) {
			console.log("Failed to load post", error);
		}
		//disable loading screen
		loading.value = false;
	};

	const deletePost = async () => {
		//close the delete modal
		isDeleting.value = false;
		//set loading screen
		loading.value = true;
		try {
			const deleteRequest = del({
				apiName: "yapp",
				path: `/api/posts/deletePost?pid=${currentPost.value.pid}`,
			});
			await deleteRequest.response;

			//set alert
			setAlert("Yipee!", "Post deleted successfully");
			//send it back to the previous page
			loading.value = false;

			router.push({
				name: "profile",
			});
		} catch (e) {
			console.log("DELETE call failed: ", e);
			setAlert("Oops!", "Failed to delete post");
		}
		//disable loading screen
		loading.value = false;
	};

	const putPost = async (title, content) => {
		//set loading screen
		loading.value = true;
		//set the updated values
		const updatedPost = ref(null);
		updatedPost.value = currentPost.value;
		updatedPost.value.postTitle = title;
		updatedPost.value.postBody = content;
		updatedPost.value.anonymous = anonIsChecked.value;
		try {
			const putRequest = put({
				apiName: "yapp",
				path: "/api/posts/updatePost",
				headers: {
					"Content-Type": "application/json",
				},
				options: {
					body: updatedPost.value,
				},
			});
			const { body } = await putRequest.response;
			const response = await body.json();
			//update the current post with the new values
			currentPost.value = updatedPost.value;
			//set alert
			setAlert("Yipee!", "Update post successfully");
		} catch (error) {
			console.log("Failed to load post", error);
			setAlert("Oops!", "Failed to update post");
		}
		//disable loading screen
		loading.value = false;
	};

	const fetchComments = async pid => {
		//set loading screen
		loading.value = true;
		try {
			const restOperationComments = get({
				apiName: "yapp",
				path: `/api/comments/getCommentsByPid?pid=${pid}`,
			});
			const { body: bodyComments } = await restOperationComments.response;
			comments.value = await bodyComments.json();
			//disable loading screen
		} catch (error) {
			console.log("Failed to load post", error);
		}
		loading.value = false;
	};

	const postComment = async message => {
		loading.value = true;
		const newComment = {
			uid: userId.value,
			pid: currentPost.value.pid,
			commentBody: message,
		};
		//clear the comment field
		try {
			const request = post({
				apiName: "yapp",
				path: "/api/comments/createComment",
				headers: {
					"Content-Type": "application/json",
				},
				options: {
					body: newComment,
				},
			});
			const { body } = await request.response;
			const response = await body.json();
			setAlert("Yipee!", "Comment created successfully");
		} catch (error) {
			console.log("Failed to create comment", error);
			setAlert("Oops!", "Failed to create comment");
		}
		loading.value = false;
	};

	const putComment = async (comment, message) => {
		loading.value = true;
		const updatedComment = ref({
			cid: "",
			pid: "",
			createdAt: "",
			updatedAt: "",
			uid: "",
			commentBody: "",
			upvotes: 0,
			downvotes: 0,
		});
		console.log(comment);
		updatedComment.value.cid = comment.cid;
		updatedComment.value.uid = comment.uid;
		updatedComment.value.pid = comment.pid;
		updatedComment.value.createdAt = comment.createdAt;
		updatedComment.value.updatedAt = getCurrentTime();
		updatedComment.value.upvotes = comment.upvotes;
		updatedComment.value.downvotes = comment.downvotes;
		updatedComment.value.commentBody = message;
		console.log(updatedComment);
		try {
			const request = put({
				apiName: "yapp",
				path: "/api/comments/updateComment",
				headers: {
					"Content-Type": "application/json",
				},
				options: {
					body: updatedComment.value,
				},
			});
			const { body } = await request.response;
			const response = await body.json();
			setAlert("Yipee!", "Comment updated successfully");
		} catch (error) {
			console.log("Failed to update comment", error);
			setAlert("Oops!", "Failed to update comment");
		}
		loading.value = false;
	};

	const createComment = async () => {
		//verify if the comment is not empty
		var comment = document.getElementById("comment").value;
		if (comment === "") {
			alert("Comment cannot be empty");
			return;
		}
		// post the comment
		await postComment(comment);
		//close the comment form
		isAddingComment.value = false;
		//fetch the comments again
		await fetchComments(currentPost.value.pid);
	};

	const updateComment = async comment => {
		//verify if the comment is not empty
		var commentText = document.getElementById(
			`commentText-${comment.cid}`,
		).value;
		if (commentText === "") {
			alert("Comment cannot be empty");
			return;
		}
		console.log(comment);
		//update the comment
		await putComment(comment, commentText);
		//remove the comment from the list of comments being edited
		commentIds.value = commentIds.value.filter(id => id !== comment.cid);
		//fetch the comments again
		await fetchComments(currentPost.value.pid);
	};

	const updatePost = async () => {
		var title = document.getElementById("title").value;
		var content = document.getElementById("content").value;
		// verify if the title and content are not empty
		if (title === "" || content === "") {
			alert("Title and Content cannot be empty");
			return;
		}
		await putPost(
			document.getElementById("title").value,
			document.getElementById("content").value,
		);
		//close the edit form
		isEditing.value = false;
	};

	const setAlert = (header, message) => {
		alertMsg.value.header = header;
		alertMsg.value.message = message;
		showAlert.value = true;
	};

	function toggleAnonymous() {
		anonIsChecked.value = !anonIsChecked.value;
	}
</script>

<template>
	<LoadingScreen v-if="loading" />

	<div
		v-else
		class="mb-5 flex min-h-screen flex-row items-start justify-center gap-4 px-4 pt-[10rem]"
		id="postDetails">
		<div v-if="currentPost && !isEditing" class="flex w-full justify-center">
			<BackBtn class="mt-2 self-start" />
			<div
				class="m-2 max-w-4xl flex-1 rounded-lg border border-gray-300 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md">
				<div class="relative mb-4">
					<!-- Post Title and Created At -->
					<h3 class="break-words text-lg font-semibold">
						{{ currentPost.postTitle }}
					</h3>
					<p class="text-sm text-gray-600">
						<strong>Created At:</strong>
						{{ new Date(currentPost.createdAt).toLocaleString() }}
					</p>
					<DotMenu
						v-if="currentPost.uid == userId"
						@edit="editPost"
						@openDeleteModal="openDeleteModal" />
				</div>

				<!-- Post Body -->
				<div class="mb-4 whitespace-pre-wrap break-words text-gray-700">
					{{ currentPost.postBody }}
				</div>
				<!-- Icons for upvote, downvote, and reply -->
				<div class="mx-4 flex justify-end space-x-4">
					<!-- <button @click.stop="upvote(post.pid)" class="flex items-center space-x-2">
                        <img src="../assets/post/upvote.svg" alt="Upvote" class="w-6 h-6">
                        <span>10</span>
                    </button> -->
					<!-- <button @click.stop="downvote(post.pid)" class="flex items-center space-x-2">
                        <img src="../assets/post/downvote.svg" alt="Downvote" class="w-6 h-6">
                        <span>20</span>
                    </button> -->
					<button
						@click="reply()"
						class="flex items-center space-x-2 rounded-xl p-2 hover:bg-light-pink hover:text-white">
						<img src="../assets/post/reply.svg" alt="Reply" class="h-6 w-6" />
						<span>Reply</span>
					</button>
				</div>
				<hr />
				<div class="relative mb-4 mt-5" v-if="isAddingComment">
					<div class="px-2">
						<textarea
							type="text"
							class="input"
							placeholder="Add a comment"
							id="comment" />
					</div>
					<div class="flex justify-end p-2">
						<button
							class="mr-2 rounded-lg bg-light-pink p-2 text-white"
							id="cancelAddComment"
							@click="cancelReply()">
							Cancel
						</button>
						<button
							class="rounded-lg bg-dark-pink p-2 text-white"
							id="createAddComment"
							@click="createComment">
							Send
						</button>
					</div>
				</div>

				<!--Comment section-->
				<div v-for="comment in comments" :key="comment.cid" class="max flex-1">
					<div
						class="m-2 max-w-4xl flex-1 rounded-lg border border-gray-300 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md">
						<div class="relative mb-4" :id="`comment-${comment.cid}`">
							<p
								class="break-words text-sm text-gray-600"
								:id="`commentBody-${comment.cid}`"
								v-if="!isEditingComment(comment) || comment.uid != userId">
								{{ comment.commentBody }}
							</p>
							<p
								class="text-xs text-gray-600"
								:id="`createAt-${comment.cid}`"
								v-if="
									comment.updatedAt == comment.createdAt &&
									!isEditingComment(comment)
								">
								<strong>Created At:</strong>
								{{ new Date(comment.createdAt).toLocaleString() }}
							</p>

							<p
								class="text-xs text-gray-600"
								:id="`createAt-${comment.cid}`"
								v-if="
									comment.updatedAt != comment.createdAt &&
									!isEditingComment(comment)
								">
								<strong>Updated At:</strong>
								{{ new Date(comment.updatedAt).toLocaleString() }}
							</p>

							<textarea
								:id="`commentText-${comment.cid}`"
								v-if="isEditingComment(comment) && comment.uid == userId"
								class="input"
								>{{ comment.commentBody }}</textarea
							>

							<DotMenu
								v-if="!isEditingComment(comment) && comment.uid == userId"
								:id="`commentDot-${comment.cid}`"
								@edit="editComment(comment)"
								@openDeleteModal="openDeleteCommentModal(comment)" />
						</div>
						<div
							class="flex justify-end"
							v-if="isEditingComment(comment) && comment.uid == userId">
							<button
								class="mr-2 rounded-lg bg-light-pink p-2 text-white"
								:id="`cancelComment-${comment.cid}`"
								@click="cancelEditComment(comment)">
								Cancel
							</button>
							<button
								class="rounded-lg bg-dark-pink p-2 text-white"
								@click="updateComment(comment)"
								:id="`updateComment-${comment.cid}`">
								Update Comment
							</button>
						</div>
					</div>
				</div>
			</div>
		</div>

		<div
			v-if="currentPost && isEditing"
			class="m-2 max-w-4xl flex-1 rounded-lg border border-gray-300 bg-gray-100 p-5 shadow transition-shadow hover:shadow-md">
			<div
				v-if="currentPost.diaryEntry"
				class="mb-4 rounded-lg border-2 border-gray-300 p-8">
				<div class="float-root mb-6">
					<label class="float-left block text-lg font-semibold text-gray-700"
						>Anonymous?</label
					>

					<label class="float-right cursor-pointer select-none items-center">
						<div class="relative ml-2 mr-2">
							<input
								type="checkbox"
								class="sr-only"
								@change="toggleAnonymous" />
							<div
								:class="{ '!bg-[#A55678]': anonIsChecked }"
								class="box block h-8 w-14 rounded-full bg-[#9E9E9E]"></div>
							<div
								:class="{ 'translate-x-full': anonIsChecked }"
								class="dot absolute left-1 top-1 h-6 w-6 rounded-full bg-white transition"></div>
						</div>
					</label>
				</div>
			</div>

			<div class="form-group mb-4 w-full">
				<label for="title" class="mb-2 block text-gray-700">Title:</label>
				<input
					type="text"
					id="title"
					required
					:value="currentPost.postTitle"
					class="input" />
			</div>

			<div class="form-group mb-4 w-full">
				<label for="content" class="mb-2 block text-gray-700">Content:</label>
				<textarea
					id="content"
					required
					:value="currentPost.postBody"
					class="input"></textarea>
			</div>

			<div class="flex w-full flex-col space-y-2">
				<button
					title="Update Post"
					id="updatePostButton"
					class="w-full rounded-xl bg-pink-purple px-5 py-3 text-white"
					type="submit"
					@click="updatePost">
					Update Post
				</button>
				<button
					title="Discard Post"
					class="w-full rounded-xl border border-gray-300 bg-white px-5 py-3 text-dark"
					@click="openDiscardModal">
					Discard
				</button>
			</div>
		</div>

		<ConfirmationModal
			:showModal="isDeleting"
			:close="closeDeleteModal"
			:confirm="deletePost"
			header="Woah there!"
			message="Are you sure you want to delete this post?" />
		<ConfirmationModal
			:showModal="isDiscardingUpdate"
			:close="closeDiscardModal"
			:confirm="confirmDiscard"
			header="Woah there!"
			message="Are you sure you want to discard your update?" />
		<ConfirmationModal
			:showModal="isDeletingComment"
			:close="closeDeleteCommentModal"
			:confirm="confirmDeleteComment"
			header="Woah there!"
			:message="`Are you sure you want to delete this '${commentMsg}'?`" />
		<Alert
			:showModal="showAlert"
			:header="alertMsg.header"
			:message="alertMsg.message"
			:close="closeAlert" />
	</div>
</template>

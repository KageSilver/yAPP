<script setup>
    import {
    del,
    get,
    post,
    put
} from 'aws-amplify/api';
import {
    getCurrentUser
} from 'aws-amplify/auth';
import {
    onMounted,
    ref
} from 'vue';
import {
    useRoute,
    useRouter
} from 'vue-router';
import Alert from '../components/Alert.vue';
import BackBtn from '../components/BackBtn.vue';
import ConfirmationModal from '../components/ConfirmationModal.vue';
import DotMenu from '../components/DotMenu.vue';
import LoadingScreen from '../components/LoadingScreen.vue';
    // Importing necessary modules
    const route = useRoute();
    const router = useRouter();

    // State
    const currentPost = ref(null); // Stores the current post details

    // Loading and alert management
    const loading = ref(false); // Controls the loading state
    const showAlert = ref(false); // Controls the visibility of the alert
    const alertMsg = ref({
        header: '',
        message: ''
    }); // Stores alert messages

    // Post editing and deleting states
    const isEditing = ref(false); // Controls the visibility of the edit form
    const isDeleting = ref(false); // Controls the visibility of the delete confirmation modal
    const isDiscardingUpdate = ref(false); // Controls the visibility of the discard confirmation modal

    // Comment management states
    const isAddingComment = ref(false); // Controls the visibility of the add comment form
    const isDeletingComment = ref(false); // Controls the visibility of the delete comment modal
    const commentIds = ref([]); // Tracks the IDs of comments being edited
    const commentMsg = ref(''); // Stores the content of the comment being deleted
    const comments = ref([]); // List of comments
    const userId = ref(''); // Stores the user ID

    // Post functions

    // Handles the start of post editing
    const editPost = () => {
        isEditing.value = true;
        isDiscardingUpdate.value = false;
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
    const editComment = (comment) => {
        commentIds.value.push(comment.cid);
    };

    // Deletes a comment (can integrate API call here)
    const deleteComment = (comment) => {
        // API call to delete the comment and refresh data
    };

    // Opens the delete comment confirmation modal
    const openDeleteCommentModal = (comment) => {
        isDeletingComment.value = true;
        commentMsg.value = comment.commentBody;
    };

    // Closes the delete comment confirmation modal
    const closeDeleteCommentModal = () => {
        isDeletingComment.value = false;
    };

    // Cancels comment editing by removing its ID from the edit list
    const cancelEditComment = (comment) => {
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
    const isEditingComment = (comment) => {
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

    const fetchPost = async (pid) => {
        loading.value = true;
        try {
            //set loading screen

            const restOperation = await get({
                apiName: 'yapp',
                path: `/api/posts/getPostById?pid=${pid}`,
            });
            const {
                body
            } = await restOperation.response;
            const response = await body.json();
            currentPost.value = response;

            //disable loading screen
            loading.value = false;

        } catch (error) {
            console.log('Failed to load post', error);
        }
        //disable loading screen
        loading.value = false;
    };

const deletePost = async () => {
        //close the delete modal
        isDeleting.value = false;
        //set loading screen
        loading.value = true;
        console.log(currentPost.value.pid);
        try {
            const deleteRequest = del({
                apiName: 'yapp',
                path: `/api/posts/deletePost?pid=${currentPost.value.pid}`,
            });
            await deleteRequest.response;
           
            //set alert
            setAlert("Yipee!", "Post deleted successfully");
            //send it back to the previous page
            loading.value = false;
            
            router.go(-2);
        } catch (e) {
           console.log('DELETE call failed: ', e);
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
        try {
            const putRequest = await put({
                apiName: 'yapp',
                path: '/api/posts/updatePost',
                headers: {
                    'Content-Type': 'application/json'
                },
                options: {
                    body: updatedPost.value
                }
            });
            const {
                body
            } = await putRequest.response;
            const response = await body.json();
            //update the current post with the new values
            currentPost.value = updatedPost.value;
            //set alert
            setAlert("Yipee!", "Update post successfully");

        } catch (error) {
            console.log('Failed to load post', error);
            setAlert("Oops!", "Failed to update post");
        }
        //disable loading screen
        loading.value = false;
    };

    const fetchComments = async (pid) => {
        //set loading screen
        loading.value = true;
        try {
            const restOperationComments = await get({
                apiName: 'yapp',
                path: `/api/comments/getCommentsByPid?pid=${pid}`,
            });
            const {
                body: bodyComments
            } = await restOperationComments.response;
            comments.value = await bodyComments.json();
            //disable loading screen


        } catch (error) {
            console.log('Failed to load post', error);
        }
        loading.value = false;
    };

    const postComment = async (message) => {
        loading.value = true;
        const newComment = {
            "uid": userId.value,
            "pid": currentPost.value.pid,
            "commentBody": message
        };
        //clear the comment field
        try {

            const request = await post({
                apiName: 'yapp',
                path: '/api/comments/createComment',
                headers: {
                    'Content-Type': 'application/json'
                },
                options: {
                    body: newComment
                }

            });
            const {
                body
            } = await request.response;
            const response = await body.json();
            setAlert("Yipee!", "Comment created successfully");
        } catch (error) {
            console.log('Failed to create comment', error);
            setAlert("Oops!", "Failed to create comment");

        }
        loading.value = false;
    };

    const putComment = async (comment, message) => {
        loading.value = true;
        const updatedComment = ref(null);
        updatedComment.value = comment.value;
        updatedComment.commentBody = message;
        try {

            const request = await put({
                apiName: 'yapp',
                path: '/api/comments/updateComment',
                headers: {
                    'Content-Type': 'application/json'
                },
                options: {
                    body: updatedComment
                }
            });
            const {
                body
            } = await request.response;
            const response = await body.json();
            setAlert("Yipee!", "Comment updated successfully");
        } catch (error) {
            console.log('Failed to update comment', error);
            setAlert("Oops!", "Failed to update comment");
        }
        loading.value = false;
    };


    const createComment = async () => {
        //verify if the comment is not empty
        var comment = document.getElementById("comment").value;
        if (comment === '') {
            alert('Comment cannot be empty');
            return;
        }
        // post the comment
        await postComment(comment);
        //close the comment form
        isAddingComment.value = false;
        //fetch the comments again
        await fetchComments(currentPost.value.pid);
    };

    const updateComment = async (comment) => {
        //verify if the comment is not empty
        var commentText = document.getElementById(`commentText-${comment.cid}`).value;
        if (commentText === '') {
            alert('Comment cannot be empty');
            return;
        }
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
        if (title === '' || content === '') {
            alert('Title and Content cannot be empty');
            return;
        }
        await putPost(document.getElementById("title").value, document.getElementById("content").value);
        //close the edit form
        isEditing.value = false;
    }

    const setAlert = (header, message) => {
        alertMsg.value.header = header;
        alertMsg.value.message = message;
        showAlert.value = true;
    };
</script>

<template>
    <LoadingScreen v-if="loading" />

    <div v-else class="flex flex-row items-start justify-center min-h-screen gap-4 mb-5 px-4 pt-[10rem]"
        id="postDetails">


        <div v-if="currentPost&&!isEditing" class="w-full justify-center flex">
            <BackBtn class="self-start mt-2" />
            <div
                class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
                <div class="mb-4 relative">
                    <!-- Post Title and Created At -->
                    <h3 class="text-lg font-semibold break-words">{{ currentPost.postTitle }}</h3>
                    <p class="text-sm text-gray-600">
                        <strong>Created At:</strong> {{ new Date(currentPost.createdAt).toLocaleString() }}
                    </p>
                    <DotMenu v-if="currentPost.uid == userId" @edit="editPost" @openDeleteModal="openDeleteModal" />

                </div>

                <!-- Post Body -->
                <div class="text-gray-700 mb-4 whitespace-pre-wrap break-words">{{ currentPost.postBody }}</div>
                <!-- Icons for upvote, downvote, and reply -->
                <div class="flex justify-end space-x-4 mx-4">
                    <!-- <button @click.stop="upvote(post.pid)" class="flex items-center space-x-2">
                        <img src="../assets/post/upvote.svg" alt="Upvote" class="w-6 h-6">
                        <span>10</span>
                    </button> -->
                    <!-- <button @click.stop="downvote(post.pid)" class="flex items-center space-x-2">
                        <img src="../assets/post/downvote.svg" alt="Downvote" class="w-6 h-6">
                        <span>20</span>
                    </button> -->
                    <button @click="reply()"
                        class="flex items-center space-x-2 p-2 rounded-xl hover:bg-light-pink hover:text-white">
                        <img src="../assets/post/reply.svg" alt="Reply" class="w-6 h-6">
                        <span>Reply</span>
                    </button>
                </div>
                <hr>
                <div class="mb-4 relative  mt-5" v-if="isAddingComment">
                    <div class="px-2">
                        <textarea type="text" class="input" placeholder="Add a comment" id="comment" />
                        </div>
                    <div class="flex justify-end p-2">
                            <button class="bg-light-pink p-2 rounded-lg text-white  mr-2"
                                id="cancelAddComment" @click="cancelReply()">Cancel</button>
                            <button class="bg-dark-pink p-2 rounded-lg text-white "
                                id="createAddComment" @click="createComment"> Send</button>
                    </div>

                </div>
                
                <!--Comment section-->
                <div v-for="comment in comments" :key="comment.cid" class="flex-1 max">
                    <div
                        class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
                        <div class="mb-4 relative" :id="`comment-${comment.cid}`">
                    
                            <p class="text-sm text-gray-600 break-words" :id="`commentBody-${comment.cid}`" v-if="!isEditingComment(comment)|| comment.uid != userId" >
                                {{ comment.commentBody }}</p>
                            <p class="text-xs text-gray-600" :id="`createAt-${comment.cid}`" v-if="comment.updatedAt == comment.createdAt && !isEditingComment(comment)">
                                <strong>Created At:</strong> {{ new Date(comment.createdAt).toLocaleString() }}
                            </p>

                            <p class="text-xs text-gray-600" :id="`createAt-${comment.cid}`" v-if="comment.updatedAt != comment.createdAt && !isEditingComment(comment) ">
                                <strong>Updated At:</strong> {{ new Date(comment.updatedAt).toLocaleString() }}
                            </p>

                            <textarea :id="`commentText-${comment.cid}`" v-if="isEditingComment(comment)&& comment.uid == userId"
                                class="input">{{ comment.commentBody }}</textarea>

                        <DotMenu v-if="!isEditingComment(comment) && comment.uid == userId"
                            :id="`commentDot-${comment.cid}`" @edit="editComment(comment)"
                            @openDeleteModal="openDeleteCommentModal(comment)" />

                    </div>
                    <div class="flex justify-end" v-if="isEditingComment(comment)&& comment.uid == userId">
                        <button class="bg-light-pink p-2 rounded-lg text-white  mr-2"
                            :id="`cancelComment-${comment.cid}`" @click="cancelEditComment(comment)">Cancel</button>
                        <button class="bg-dark-pink p-2 rounded-lg text-white " @click="updateComment(comment)"
                            :id="`updateComment-${comment.cid}`">Update Comment</button>
                    </div>
                </div>
            </div>

        </div>


    </div>

    <div v-if="currentPost&&isEditing"
        class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
        <div class="form-group w-full mb-4">
            <label for="title" class="block mb-2 text-gray-700">Title:</label>
            <input type="text" id="title" required :value="currentPost.postTitle" class="input">
        </div>
        <div class="form-group w-full mb-4">
            <label for="content" class="block mb-2 text-gray-700">Content:</label>
            <textarea id="content" required :value="currentPost.postBody" class="input"></textarea>
        </div>
        <div class="flex flex-col space-y-2 w-full">
            <button title="Update Post" id="updatePostButton"
                class="bg-pink-purple text-white px-5 py-3 rounded-xl w-full" type="submit" @click="updatePost">
                Update Post
            </button>
            <button title="Discard Post" class="bg-white text-dark px-5 py-3 rounded-xl w-full border border-gray-300"
                @click="openDiscardModal">
                Discard
            </button>
        </div>

    </div>



    <ConfirmationModal :showModal=isDeleting :close="closeDeleteModal" :confirm="deletePost" header="Woah there!"
        message="Are you sure you want to delete this post?" />
    <ConfirmationModal :showModal=isDiscardingUpdate :close="closeDiscardModal" :confirm="confirmDiscard"
        header="Woah there!" message="Are you sure you want to discard your update?" />
    <ConfirmationModal :showModal=isDeletingComment :close="closeDeleteCommentModal" :confirm="deleteComment"
        header="Woah there!" :message="`Are you sure you want to delete this '${commentMsg}'?`" />
    <Alert :showModal="showAlert" :header="alertMsg.header" :message="alertMsg.message" :close="closeAlert" />

    </div>
</template>
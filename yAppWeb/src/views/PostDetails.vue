<script setup>
    import {
        get
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
    import BackBtn from '../components/BackBtn.vue';
    import ConfirmationModal from '../components/ConfirmationModal.vue';
    import DotMenu from '../components/DotMenu.vue';

    const route = useRoute();
    const post = ref(null);
    //post editing and deleting
    const isEditing = ref(false); // State to control the visibility of the edit form
    const isDeleting = ref(false); // State to control the visibility of the confirmation modal
    const isDiscardingUpdate = ref(false); // State to control the visibility of the confirmation modal


    const isAddingComment = ref(false); // State to control the visibility of the adding comment form
    const isDeletingComment = ref(false); // State to control the visibility of the confirmation modal
    const commentIds = ref([]);
    const commentMsg = ref('');


    const isEditingComment = (comment) => {
        return ref(commentIds.value.includes(comment.cid)).value;
    };

    

    // Function to handle editing the post
    const editPost = () => {
        isEditing.value = true; // Set the editing state to true
        isDiscardingUpdate.value = false; // Set the discarding update state to false
    };

    // Function to open the  modal
    const openDeleteModal = () => {
        isDeleting.value = true; // Set the deleting state to true
    };

    const closeDeleteModal = () => {
        isDeleting.value = false;
    };

    const confirmDelete = () => {
        // Make API call to delete the post
        //get the updated values


    };


    const openDiscardModal = () => {
        isDiscardingUpdate.value = true; // Set the discarding update state to true
    };

    const closeDiscardModal = () => {
        isDiscardingUpdate.value = false;
    };

    const confirmDiscard = () => {
        isEditing.value = false; // Set the editing state to false
        isDiscardingUpdate.value = false; // Set the discarding update state to false
    };


    const deleteComment = (comment) => {
        // Make API call to delete the comment
        //get the updated values
    };

    const editComment = (comment) => {
        // Make API call to edit the comment
        //get the updated values
        commentIds.value.push(comment.cid);
        console.log(comment.commentBody);
        //document.getElementById("commentText").value = comment.commentBody;
    };
    const openDeleteCommentModal = (comment) => {
        isDeletingComment.value = true;
        //set the comment message
        commentMsg.value = comment.commentBody;
        // Make API call to delete the comment
        //get the updated values
    };

    const closeDeleteCommentModal = () => {
        // Make API call to delete the comment
        //get the updated values
        isDeletingComment.value = false;

    };

    const cancelEditComment = (comment) => {
        // Remove the comment ID from the list of editing comments
        commentIds.value = commentIds.value.filter(id => id !== comment.cid);
    };

    const reply = () => {
        isAddingComment.value = true;
    };

    const canceReply = () => {
        isAddingComment.value = false;
    };


    const comments = ref([]);
    const userId = ref('');


    onMounted(async () => {

        const pid = route.params.pid;
        const user = await getCurrentUser();
        userId.value = user.userId;

        try {
            // const restOperation = await get({
            //     apiName: 'yapp',
            //     path: `/api/posts/getPostById?pid=${pid}`,
            // });
            // const {
            //     body
            // } = await restOperation.response;
            // const response = await ((await body.blob()).arrayBuffer());
            // const decoder = new TextDecoder('utf-8');
            // const decodedText = decoder.decode(response);
            // post.value = JSON.parse(decodedText);

            // if (post.value.uid === user.userId) {
            //     isEditable.value = true;
            // } else {
            //     isEditable.value = false;
            // }
     

            //set post value for now
            post.value = {
                "pid": 1,
                "postTitle": "This is a post",
                "postBody": "This is a post body",
                "createdAt": "2021-10-10T10:10:10",
                "updatedAt": "2021-10-10T10:10:10",
                "uid": user.userId,
            };

            // also getting a list of the comments for the post
            comments.value.push({
                    "cid": 1,
                    "pid": 1,
                    "commentBody": "This is a comment",
                    "createdAt": "2021-10-10T10:10:10",
                    "updatedAt": "2021-10-10T10:10:10",
                    "commentBody": "This is a comment",
                    "upvotes": 10,
                    "downvotes": 5,
                    "uid": user.userId, // this allow to edit looks like
                }, {
                    "cid": 2,
                    "pid": 1,
                    "commentBody": "This is another comment",
                    "createdAt": "2021-10-10T10:10:10",
                    "updatedAt": "",
                    "commentBody": "This is another comment",
                    "upvotes": 10,
                    "downvotes": 5,
                    "uid": user.userId, // this allow to edit looks like
                },

                {
                    "cid": 3,
                    "pid": 1,
                    "commentBody": "This is another comment",
                    "createdAt": "2021-10-10T10:10:10",
                    "updatedAt": "2021-10-10T10:10:10",
                    "commentBody": "This is another comment",
                    "upvotes": 10,
                    "downvotes": 5,
                    "uid": "123", // this allow to edit looks like
                }


            );
        } catch (error) {
            console.log('Failed to load post', error);
        }
    });


    const updatePost = () => {
        // Make API call to update the post
        //get the updated values
        var updatedPost = {
            "pid": post.value.pid,
            "postTitle": document.getElementById("title").value,
            "postBody": document.getElementById("content").value,
            "updatedAt": '',
            "createdAt": post.value.createdAt,
            "userName": post.value.userName
        };
    };
</script>

<template>
    <div class="flex flex-row items-start justify-center min-h-screen gap-4 mb-5 px-4 pt-[10rem]">


        <div v-if="post&&!isEditing" class="w-full justify-center flex">
            <BackBtn class="self-start mt-2" />
            <div
                class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
                <div class="mb-4 relative">
                    <!-- Post Title and Created At -->
                    <h3 class="text-lg font-semibold break-words">{{ post.postTitle }}</h3>
                    <p class="text-sm text-gray-600">
                        <strong>Created At:</strong> {{ new Date(post.createdAt).toLocaleString() }}
                    </p>
                    <DotMenu v-if="post.uid == userId" @edit="editPost" @openDeleteModal="openDeleteModal" />

                </div>
                
                <!-- Post Body -->
                <div class="text-gray-700 mb-4 whitespace-pre-wrap break-words">{{ post.postBody }}</div>
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
                    <button @click="reply()" class="flex items-center space-x-2 p-2 rounded-xl hover:bg-light-pink hover:text-white">
                        <img src="../assets/post/reply.svg" alt="Reply" class="w-6 h-6">
                        <span>Reply</span>
                    </button>
                </div>
                <hr>
                <div class="mb-4 relative  mt-5" v-if="isAddingComment">
                    <div class="px-2">
                        <textarea type="text" class="input" placeholder="Add a comment" />
                    </div>
                    <div class="flex justify-end p-2">
                            <button class="bg-light-pink p-2 rounded-lg text-white  mr-2"
                                :id="`cancelAddComment`" @click="canceReply()">Cancel</button>
                            <button class="bg-dark-pink p-2 rounded-lg text-white "
                                :id="`createAddComment`">Update Comment</button>
                    </div>

                </div>
                
                <!--Comment section-->
                <div v-for="comment in comments" :key="comment.cid" class="flex-1 max">
                    <div
                        class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
                        <div class="mb-4 relative" :id="`comment-${comment.cid}`">
                    
                            <p class="text-sm text-gray-600 break-words" :id="`commentBody-${comment.cid}`" v-if="!isEditingComment(comment)|| comment.uid != userId" >
                                {{ comment.commentBody }}</p>
                            <p class="text-xs text-gray-600" :id="`createAt-${comment.cid}`" v-if="comment.updatedAt == '' && !isEditingComment(comment)">
                                <strong>Created At:</strong> {{ new Date(comment.createdAt).toLocaleString() }}
                            </p>

                            <p class="text-xs text-gray-600" :id="`createAt-${comment.cid}`" v-if="comment.updatedAt != '' && !isEditingComment(comment) ">
                                <strong>Updated At:</strong> {{ new Date(comment.updatedAt).toLocaleString() }}
                            </p>

                            <textarea :id="`commentText-${comment.cid}`" v-if="isEditingComment(comment)&& comment.uid == userId"
                                class="input">{{ comment.commentBody }}</textarea>
                        
                            <DotMenu v-if="!isEditingComment(comment) && comment.uid == userId" :id="`commentDot-${comment.cid}`"
                                @edit="editComment(comment)" @openDeleteModal="openDeleteCommentModal(comment)" />

                        </div>
                        <div class="flex justify-end" v-if="isEditingComment(comment)&& comment.uid == userId">
                            <button class="bg-light-pink p-2 rounded-lg text-white  mr-2"
                                :id="`cancelComment-${comment.cid}`" @click="cancelEditComment(comment)">Cancel</button>
                            <button class="bg-dark-pink p-2 rounded-lg text-white "
                                :id="`updateComment-${comment.cid}`">Update Comment</button>
                        </div> 
                    </div>
                </div>

            </div>


        </div>

        <div v-if="post&&isEditing"
            class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
            <div class="form-group w-full mb-4">
                <label for="title" class="block mb-2 text-gray-700">Title:</label>
                <input type="text" id="title" required :value="post.postTitle" class="input">
            </div>
            <div class="form-group w-full mb-4">
                <label for="content" class="block mb-2 text-gray-700">Content:</label>
                <textarea id="content" required :value="post.postBody" class="input"></textarea>
            </div>
            <div class="flex flex-col space-y-2 w-full">
                <button title="Create Post" id="create-button"
                    class="bg-pink-purple text-white px-5 py-3 rounded-xl w-full" type="submit" @click="updatePost">
                    Update Post
                </button>
                <button title="Discard Post"
                    class="bg-white text-dark px-5 py-3 rounded-xl w-full border border-gray-300"
                    @click="openDiscardModal">
                    Discard
                </button>
            </div>

        </div>



        <ConfirmationModal :showModal=isDeleting :close="closeDeleteModal" :confirm="confirmDelete" header="Woah there!"
            message="Are you sure you want to delete this post?" />
        <ConfirmationModal :showModal=isDiscardingUpdate :close="closeDiscardModal" :confirm="confirmDiscard"
            header="Woah there!" message="Are you sure you want to discard your update?" />
        <ConfirmationModal :showModal=isDeletingComment :close="closeDeleteCommentModal" :confirm="deleteComment"
            header="Woah there!" :message="`Are you sure you want to delete this '${commentMsg}'?`" />


    </div>
</template>
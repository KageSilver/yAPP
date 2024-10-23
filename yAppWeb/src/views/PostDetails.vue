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

    const route = useRoute();
    const router = useRouter();
    const post = ref(null);

    const isEditing = ref(false); // State to control the visibility of the edit form
    const isDeleting = ref(false); // State to control the visibility of the confirmation modal
    const isDiscardingUpdate = ref(false); // State to control the visibility of the confirmation modal
    const isMenuOpen = ref(false); // State to control the visibility of the 3-dot menu
    const isEditable = ref(false); // State to control the visibility of the 3-dot menu

    // Toggle the 3-dot menu visibility
    const toggleMenu = () => {
        isMenuOpen.value = !isMenuOpen.value; // Toggle the menu visibility
    };

    // Function to handle editing the post
    const editPost = () => {
        isMenuOpen.value = false; // Close the menu
        isEditing.value = true; // Set the editing state to true
        isDiscardingUpdate.value = false; // Set the discarding update state to false
    };

    // Function to open the  modal
    const openDeleteModal = () => {
        isMenuOpen.value = false; // Close the menu
        isDeleting.value = true; // Set the deleting state to true
    };

    const closeDeleteModal = () => {
        isMenuOpen.value = false; // Close the menu
        isDeleting.value = false;
    };

    const confirmDelete = () => {
        // Make API call to delete the post
        //get the updated values

    };


    const openDiscardModal = () => {
        isMenuOpen.value = false; // Close the menu
        isDiscardingUpdate.value = true; // Set the discarding update state to true
    };

    const closeDiscardModal = () => {
        isMenuOpen.value = false; // Close the menu
        isDiscardingUpdate.value = false;
    };

    const confirmDiscard = () => {
        isEditing.value = false; // Set the editing state to false
        isDiscardingUpdate.value = false; // Set the discarding update state to false
    };

    const user = ref(null);

    onMounted(async () => {

        const pid = route.params.pid;
        user.value = await getCurrentUser();

        try {
            const restOperation = await get({
                apiName: 'yapp',
                path: `/api/posts/getPostById?pid=${pid}`,
            });
            const {
                body
            } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8');
            const decodedText = decoder.decode(response);
            post.value = JSON.parse(decodedText);

            if (post.value.uid === user.userId) {
                isEditable.value = true;
            } else {
                isEditable.value = false;
            }
            isEditable.value = true;
            console.log(isEditable.value);
            // to display the 3 dot menu for the post for now.. will change when we have the backend
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
        <BackBtn class="self-start mt-2" />

        <div v-if="post&&!isEditing"
            class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
            <div class="mb-4 relative">
                <!-- Post Title and Created At -->
                <h3 class="text-lg font-semibold break-words">{{ post.postTitle }}</h3>
                <p class="text-sm text-gray-600">
                    <strong>Created At:</strong> {{ new Date(post.createdAt).toLocaleString() }}
                </p>

                <!-- Three-dot Menu (Dropdown) -->
                <div class="absolute top-0 right-0" v-if="isEditable">
                    <button @click="toggleMenu" class="text-gray-600 hover:text-gray-900 focus:outline-none">
                        <svg class="w-10 h-10 " fill="none" stroke="currentColor" viewBox="0 0 24 24"
                            xmlns="http://www.w3.org/2000/svg">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                                d="M12 6v.01M12 12v.01M12 18v.01"></path>
                        </svg>
                    </button>

                    <!-- Dropdown Menu -->
                    <div v-if="isMenuOpen"
                        class="absolute right-0 mt-2 w-24 bg-white border border-gray-200 rounded shadow-lg z-10 text-center">
                        <a href="#" @click="editPost"
                            class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">Edit</a>
                        <a href="#" @click="openDeleteModal"
                            class="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">Delete</a>
                    </div>
                </div>
            </div>

            <!-- Post Body -->
            <div class="text-gray-700 mb-4 whitespace-pre-wrap break-words">{{ post.postBody }}</div>


            <!-- Icons for upvote, downvote, and reply with counts -->
            <!-- <div class="flex justify-start space-x-4 mt-4">
                <button @click.stop="upvote(post.pid)" class="flex items-center space-x-2">
                    <img src="../assets/post/upvote.svg" alt="Upvote" class="w-6 h-6">
                    <span>10</span>
                </button>
                <button @click.stop="downvote(post.pid)" class="flex items-center space-x-2">
                    <img src="../assets/post/downvote.svg" alt="Downvote" class="w-6 h-6">
                    <span>20</span>
                </button>
            </div> -->
        </div>
        <div v-else-if="isEditing"
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

    </div>
</template>
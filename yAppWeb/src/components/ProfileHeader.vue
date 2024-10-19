<script setup>
import { get, put } from 'aws-amplify/api';
import { ref, onMounted } from 'vue';
import { getCurrentUser } from 'aws-amplify/auth';
import { useRoute } from 'vue-router';
import { useRouter } from 'vue-router';

const router = useRouter(); // Use router hook

const username = ref(''); // Reacted variable to hold the username
const userId = ref(''); // Reacted variable to hold the userId
const jsonData = ref([]); // Reacted array to hold the list of friendships
const counts = ref(0); // Reacted variable to hold the number of friend requests
const route = useRoute();  // This composable provides access to the current route object

// Function to determine if the current route's path matches the given path
const isActive = (path) => {
    return route.path.includes(path);
};

// Get list of friends as JSON 
onMounted(async () => {
    getRequests();
    const user = await getCurrentUser();
    username.value = user.username;
    userId.value = user.userId;
});

// Get authenticated user's friend requests
async function getRequests() {
    try {
        const restOperation = get({
            apiName: 'yapp',
            path: `/api/friends/getFriendsByStatus?userName=${username}&status=0`
        });
        const { body } = await restOperation.response;
        const response = await ((await body.blob()).arrayBuffer());
        const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
        const decodedText = decoder.decode(response);
        jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
        counts.value = jsonData.value.length;
        console.log(jsonData);
    }
    catch (error) {
        console.log('GET call failed', error);
    }
}


</script>


<template>

    <div class="relative w-full pt-[8rem] pl-8 bg-light-pink mt-[8rem]">
        <h5 class="mb-2 text-3xl font-bold text-white">Welcome back, {{ username }}</h5>
        <!-- Button positioned at the top-right corner -->
        <button
            class="absolute top-0 right-2 mt-2 mr-16  hover:bg-dark-pink-800 text-white font-bold rounded-lg px-4 py-2"
            @click="router.push('/profile/friendRequests')">
            <span class="material-icons">group_add</span>
            <!-- Notification Badge -->
            <span
                class="absolute top-1 -right-1 bg-dark-pink text-white text-xs font-bold rounded-full h-5 w-5 flex items-center justify-center"
                v-if="counts">
                {{ counts }}
            </span>
        </button>
    </div>

    <div class="w-full pl-8 pb-[5rem] bg-dark-purple  mt-0 pt-2">
        <h5 class="mb-2 text-sm text-purple ">UUID: {{ userId }}</h5>
    </div>
    <div class="sm:hidden">
        <label for="tabs" class="sr-only">Select tab</label>
        <select id="tabs"
            class="bg-white text-gray-900 text-sm  block w-full p-2.5 border-none focus:ring-2 focus:ring-indigo-500">
            <option>My Posts</option>
            <option>My Friends</option>
            <option>My Achievement</option>
        </select>
    </div>
    <ul class="hidden text-sm font-medium text-center text-gray-900 sm:flex" id="fullWidthTab"
        data-tabs-toggle="#fullWidthTabContent" role="tablist">
        <li class="w-full">
            <button @click="router.push('/profile/myPosts')"
                :class="['inline-block w-full p-4 focus:outline-none bg-white hover:bg-dark-purple hover:text-white', isActive('/myPosts') ? 'text-light-pink' : '']">
                My Posts
            </button>
        </li>
        <li class="w-full">
            <button @click="router.push('/profile/friends')"
                :class="['inline-block w-full p-4 focus:outline-none bg-white hover:bg-dark-purple hover:text-white', isActive('/friends') ? 'text-light-pink' : '']">
                My Friends
            </button>
        </li>
        <li class="w-full">
            <button
                :class="['inline-block w-full p-4 focus:outline-none bg-white hover:bg-dark-purple hover:text-white', isActive('/achievements') ? 'text-light-pink' : '']">
                My Achievements
            </button>
        </li>
    </ul>



</template>

<style scoped>

</style>
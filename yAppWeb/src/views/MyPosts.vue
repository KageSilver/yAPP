<script setup>
import { get } from 'aws-amplify/api';
import { getCurrentUser } from 'aws-amplify/auth';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import ProfileHeader from '../components/ProfileHeader.vue';

const router = useRouter(); // Use router hook
const uid = ref('');
const jsonData = ref([]);
const loading = false;

const diaryEntry = false; // Replace with logic for setting whether it's diary entries or not
// Retrieve the necessary data and function from the helper
onMounted(async () => {
    const user = await getCurrentUser();
    uid.value = user.userId;
    await getPosts(uid, diaryEntry);

});

async function getPosts(uid, diaryEntry) {
    try {
        console.log(uid.value);
        const restOperation = get({
            apiName: 'yapp',
            path: `/api/posts/getPostsByUser?uid=${uid.value}`
        });
        const { body } = await restOperation.response;
        const response = await ((await body.blob()).arrayBuffer());
        const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
        const decodedText = decoder.decode(response);
        jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
    }
    catch (error) {
        console.log('GET call failed', error);
    }
}

function clickPost(pid) {
    router.push({ name: 'details', params: { pid } });
}


</script>

<template>
    <ProfileHeader />
    <div class="flex flex-col items-center w-full mx-auto">
        <div class="card bg-gray-100 border border-gray-300 rounded-lg p-5 shadow transition-shadow hover:shadow-md cursor-pointer w-full max-w-4xl m-2"
            v-for="post in jsonData" :key="post.pid" @click="clickPost(post.pid)">
            <div class="card-header mb-2">
                <h3 class="text-lg font-semibold truncate">{{ post.postTitle }}</h3>
                <p class="text-sm text-gray-600 overflow-hidden overflow-ellipsis whitespace-nowrap">
                    <strong>Created At:</strong> {{ new Date(post.createdAt).toLocaleString() }}
                </p>
            </div>
            <div class="card-body">
                <p class="text-gray-700 overflow-hidden text-ellipsis whitespace-nowrap">
                    {{ post.postBody }}
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
    </div>
</template>

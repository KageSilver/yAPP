<script setup>
import { get } from 'aws-amplify/api';
import { getCurrentUser } from 'aws-amplify/auth';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import ProfileHeader from '../components/ProfileHeader.vue';
import PostCard from '../components/PostCard.vue';

const router = useRouter(); // Use router hook
const username = ref('');
const jsonData = ref([]);
const loading = false;

const diaryEntry = false; // Replace with logic for setting whether it's diary entries or not
// Retrieve the necessary data and function from the helper
onMounted(async () => {
    const user = await getCurrentUser();
    username.value = user.username;
    await getPosts(username, diaryEntry);

});

async function getPosts(username, diaryEntry) {
    try {
        console.log(username.value);
        const restOperation = get({
            apiName: 'yapp',
            path: `/api/posts/getPostsByUser?userName=${username.value}`
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
            <PostCard :post="post" />

        </div>
    </div>
</template>

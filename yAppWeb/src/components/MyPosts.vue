<script setup>
	import { useAuthenticator } from "@aws-amplify/ui-vue";
    import { usePostHelper } from './PostListHelper';
    import { useRouter } from 'vue-router';
    import './PostListStyles.css';

    const router = useRouter(); // Use router hook
    const auth = useAuthenticator(); // Grab authenticator
    const userName = auth.user?.username;
    const diaryEntry = false; // Replace with logic for setting whether it's diary entries or not
    // Retrieve the necessary data and function from the helper
    const { jsonData, loading, truncateText } = usePostHelper(`/api/posts/getPostsByUser?userName=${userName}&diaryEntry=${diaryEntry}`);

    function clickPost(pid) 
    {
        router.push({ name: 'postDetails', params: { pid } });
    }
</script>

<template>
    <div class="card-container">
        <div v-if="loading" class="spinner"></div>
        <div v-else class="card" v-for="post in jsonData" :key="post.pid" @click="clickPost(post.pid)">
            <div class="card-header">
                <h3>{{ post.postTitle }}</h3>
                <p><strong>Created At:</strong> {{ new Date(post.createdAt).toLocaleString() }}</p>
            </div>
            <div class="card-body">
                <p>{{ truncateText(post.postBody) }}</p>
            </div>
        </div>
    </div>
</template>

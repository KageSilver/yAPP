<script setup>
	import { useRouter } from 'vue-router'; // Import useRouter
    import { usePostHelper } from './PostListHelper';
    import './PostListStyles.css';

    const router = useRouter(); // Use router hook
    var maxResults = 10; // Default is 10
    var currentDateTime = new Date(); // Setting for on creation
    var since = currentDateTime.toLocaleString();
    // Retrieve the necessary data and function from the helper
    const { jsonData, loading, truncateText, getPosts, updatePath } = usePostHelper(`/api/posts/getRecentPosts?since=${since}&maxResults=${maxResults}`);

    function clickPost(pid) 
    {
        router.push({ name: 'postDetails', params: { pid } });
    }

    function loadMore()
    {
        maxResults += 10;
        var currentDateTime = new Date(); // Setting for the current time
        var since = currentDateTime.toLocaleString();
        loading.value = true;
        updatePath(`/api/posts/getRecentPosts?since=${since}&maxResults=${maxResults}`);
        getPosts();
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
        <button class="primary-button" type="button" @click="loadMore()">Load more!</button>
    </div>
</template>

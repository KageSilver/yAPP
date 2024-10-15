<script setup="js">
    import { get } from 'aws-amplify/api';
	import { useAuthenticator } from "@aws-amplify/ui-vue";
    import { onMounted, ref } from 'vue';
	import { useRouter } from 'vue-router'; // Import useRouter
	
	const router = useRouter(); // Use router hook
    const auth = useAuthenticator(); // Grab authenticator for username

    const jsonData = ref([]); // Reacted array to hold the list of posts
    const maxLength = 100;
    var loading = ref(true); // Reactive boolean for loading spinner

    // Get list of most recent posts as JSON
    onMounted(async () => 
    {
        getPosts();
    });

    async function getPosts() 
    {
        const userName = auth.user?.username;
        const diaryEntry = false;
        try 
        {
            const restOperation = await get({
                apiName: 'yapp',
                path: `/api/posts/getPostsByUser?userName=${userName}&diaryEntry=${diaryEntry}`
            });
            const { body } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
            const decodedText = decoder.decode(response);
            jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
            loading.value = false;
        } 
        catch(error)
        {
            console.log('GET call failed', error);
        }
        if ( jsonData.value.length > 0 )
        {
            const tabContent = document.querySelector(".TabsContent .Text");
            tabContent.innerHTML = "";
        }
    }

    function clickPost(pid) 
    {
        router.push({ name: 'postDetails', params: { pid } });
    }

    function truncateText(text) 
    {
        var modifiedText = text
        if ( text.length > maxLength ) 
        {
            modifiedText = text.substring(0, maxLength) + "...";
        }
        return modifiedText;
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

<style>
    .card-container {
        display: flex;
        flex-direction: column;
        gap: 1rem;
        margin-bottom: 20px;
    }

    .card {
        background-color: #f4f4f4;
        border: 1px solid #ccc;
        border-radius: 10px;
        padding: 20px;
        cursor: pointer;
        transition: all 0.3s ease;
        width: 100%;
        max-width: 800px;
        margin: 0 auto;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

    .card:hover {
        box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
        cursor: pointer;
    }

    .card-header h3 {
        margin: 0;
        font-size: 1.5rem;
    }

    .card-header p {
        margin: 0.5rem 0;
        color: #666;
    }

    .card-body p {
        font-size: 1rem;
    }
    
    .spinner {
        border: 4px solid rgba(0, 0, 0, 0.1);
        width: 40px;
        height: 40px;
        border-radius: 50%;
        border-left-color: #09f;
        animation: spin 1s ease infinite;
    }
    @keyframes spin {
        0% {
            transform: rotate(0deg);
        }
        100% {
            transform: rotate(360deg);
        }
    }
</style>

  
<script setup>
    import { ref, onMounted } from 'vue';
    import { useRoute, useRouter } from 'vue-router';
    import { get } from 'aws-amplify/api';
    
    const route = useRoute();
    const router = useRouter();
    const post = ref(null);
    
    onMounted(async () => {
        console.log(route.params.pid);
        const pid = route.params.pid;
        try {
            const restOperation = await get({
                apiName: 'yapp',
                path: `/api/posts/getPostById?pid=${pid}`,
            });
            const { body } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8');
            const decodedText = decoder.decode(response);
            post.value = JSON.parse(decodedText);
            console.log(post.value)
        } catch (error) {
            console.log('Failed to load post', error);
        }
    });

	const goBack = async () => 
	{
		router.push('/dashboard');
	}
</script>

<template>
    <div class="button-bar" style="display:flex; justify-content:right; margin-bottom:35px;">
		<button class="primary-button" @click="goBack" style="margin-right:35px;">
            Dashboard
        </button>
    </div>
    <div class="post-container" v-if="post">
        <div class="post-box">
            <h1>{{ post.postTitle }}</h1>
            <p><strong>Created At:</strong> {{ new Date(post.createdAt).toLocaleString() }}</p>
            <p>{{ post.postBody }}</p>
        </div>
    </div>
</template>

<style>
    .post-container {
        display: flex;
        justify-content: center; 
        margin: 20px;
        color: hsl(300, 100%, 15%);
    }

    .post-box {
        background-color: #ffffff;
        border: 1px solid #ccc;
        border-radius: 10px;
        padding: 20px;
        width: 100%;
        max-width: 800px;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        word-wrap: break-word;
        white-space: pre-wrap;
        overflow: hidden;
    }
</style>

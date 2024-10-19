
  
<script setup>
    import { ref, onMounted } from 'vue';
    import { useRoute, useRouter } from 'vue-router';
    import { get } from 'aws-amplify/api';
    import BackBtn from '../components/BackBtn.vue';
    
    
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

</script>

<template>
    <div class="flex flex-row items-start justify-center min-h-screen gap-4 mb-5 px-4 pt-[10rem]">
        <BackBtn class="self-start mt-2" />

        <div v-if="post"
            class="flex-1 max-w-4xl bg-gray-100 border border-gray-300 rounded-lg shadow transition-shadow hover:shadow-md p-5 m-2">
            <div class="mb-4">
                <h3 class="text-lg font-semibold">{{ post.postTitle }}</h3>
                <p class="text-sm text-gray-600">
                    <strong>Created At:</strong> {{ new Date(post.createdAt).toLocaleString() }}
                </p>
            </div>

            <div class="text-gray-700 mb-4 whitespace-pre-wrap">{{ post.postBody }}</div>

            <!-- Icons for upvote, downvote, and reply with counts -->
            <div class="flex justify-start space-x-4 mt-4">
                <button @click.stop="upvote(post.pid)" class="flex items-center space-x-2">
                    <img src="../assets/post/upvote.svg" alt="Upvote" class="w-6 h-6">
                    <span>10</span>
                </button>
                <button @click.stop="downvote(post.pid)" class="flex items-center space-x-2">
                    <img src="../assets/post/downvote.svg" alt="Downvote" class="w-6 h-6">
                    <span>20</span>
                </button>
                <button @click.stop="reply(post.pid)" class="flex items-center space-x-2">
                    <img src="../assets/post/reply.svg" alt="Reply" class="w-6 h-6">
                    <span>Reply</span>
                </button>
            </div>
        </div>
    </div>
</template>








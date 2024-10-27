<script setup>
	import { useRouter } from 'vue-router'; // Import useRouter
    import { usePostHelper } from '../composables/usePostHelper'; // Import the helper
    import PostCard from '../components/PostCard.vue'; // Import the PostCard component
   
    const router = useRouter(); // Use router hook
    const maxResults = 10; // Default is 10
    const currentDateTime = new Date(); // Setting for on creation
    const since = currentDateTime.toJSON();
    // Retrieve the necessary data and function from the helper
    const { jsonData, loading,  getPosts, updatePath } = usePostHelper(`/api/posts/getRecentPosts?since=${since}&maxResults=${maxResults}`);
    console.log(jsonData);

    function clickPost(pid) 
    {
        router.push({ name: 'details', params: { pid } });
    }

    function loadMore()
    {
        var since = new Date(getLastPostTime());
        since = since.toJSON();
        loading.value = true;
        updatePath(`/api/posts/getRecentPosts?since=${since}&maxResults=${maxResults}`);
        getPosts();
    }

    function getLastPostTime()
    {
        var holder = null;
        if ( jsonData.value.length > 0 )
        {
            var lastPost = jsonData.value[jsonData.value.length-1];
            holder = lastPost.createdAt;
        }
        return holder;
    }
</script>

<template>

    <div class="relative w-full pt-[8rem] pl-8  items-center justify-center">
        <div v-if="loading" class="flex justify-center items-center">
            <div class="spinner animate-spin border-4 border-t-transparent rounded-full w-10 h-10"></div>
        </div>
        <div v-else class="flex flex-col items-center w-full">
            <div class="card bg-gray-100 border border-gray-300 rounded-lg p-5 shadow transition-shadow hover:shadow-md cursor-pointer w-full max-w-4xl m-2"
                v-for="post in jsonData" :key="post.pid" @click="clickPost(post.pid)">
                <PostCard :post="post" />
        
            </div>
                <button class="bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded self-center " type="button"
        @click="loadMore()">
        Load more!
    </button>
    </div>

    </div>
</template>


<style scoped>
@keyframes spin {
    from {
        transform: rotate(0deg);
    }

    to {
        transform: rotate(360deg);
    }
}
</style>

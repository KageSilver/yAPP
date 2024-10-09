<script setup="js">
    import { get } from 'aws-amplify/api';
    import { ref, onMounted } from 'vue';

    const jsonData = ref([]); // Reacted array to hold the list of posts
    const expandedIndex = ref(null); // Holds the index of the card to expand
    const maxResults = 10;

    // Get list of most recent posts as JSON
    onMounted(async () => 
    {
        const currentDateTime = new Date();
        const since = currentDateTime.toLocaleString();
        try 
        {
            const restOperation = await get({
                apiName: 'yapp',
                path: `/api/posts/getRecentPosts?since=${since}&maxResults=${maxResults}`
            });
            const { body } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
            const decodedText = decoder.decode(response);
            jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
            console.log(jsonData.value);
            const tabContent = document.querySelector(".TabsContent .Text");
            tabContent.innerHTML = "";
        } 
        catch(error)
        {
            console.log('GET call failed', error);
        }
    });

    function clickPost(index) {
        expandedIndex.value = expandedIndex.value === index ? null : index; // Toggle the expanded post
    }
</script>

<template>
    <div class="card-container">
        <div class="card" v-for="(post, index) in jsonData" :key="post.pid" @click="clickPost(index)">
            <div class="card-header">
                <h3>{{ post.postTitle }}</h3>
                <p><strong>Created At:</strong> {{ new Date(post.createdAt).toLocaleString() }}</p>
            </div>
            <div class="card-body">
                <p>{{ post.postBody }}</p>
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
  color: #4a148c;
}

.card-header p {
  margin: 0.5rem 0;
  color: #666;
}

.card-body p {
  font-size: 1rem;
  color: #5e35b1;
}
</style>
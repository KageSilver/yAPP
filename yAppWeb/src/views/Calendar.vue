<script setup>
import { getCurrentUser } from 'aws-amplify/auth';
import { onMounted, ref } from 'vue';
import { get } from 'aws-amplify/api';
import { useRouter } from 'vue-router';

const router = useRouter(); // Use router hook
const username = ref('');
const jsonData = ref([]);
const loading = false;

// Retrieve the necessary data and function from the helper
onMounted(async () => {
    const user = await getCurrentUser();
    username.value = user.username;
    await getMyDiaryEntries(username);

});

async function getMyDiaryEntries(username)
{
    try
    {
        const restOperation = get({
            apiName: 'yapp',
            path: `/api/posts/getPostsByUser?userName=${username.value}&diaryEntry=${true}`
        });
        const { body } = await restOperation.response;
        const response = await ((await body.blob()).arrayBuffer());
        const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
        const decodedText = decoder.decode(response);
        jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
    } 
    catch (e)
    {
        console.log('GET call failed', error);
    }
}

</script>

<template>  
    
</template>

<style>

</style>
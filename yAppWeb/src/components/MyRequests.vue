<script setup>
    import { get } from 'aws-amplify/api';
    import { ref, onMounted } from 'vue';
    import { useAuthenticator } from '@aws-amplify/ui-vue';

    const auth = useAuthenticator();
    const jsonData = ref([]); // Reacted array to hold the list of friendships

    // Get list of friends as JSON 
    onMounted(async () => 
    {
        try 
        {
            const username = auth.user?.username;
            const restOperation = await get({
                apiName: 'yapp',
                path: `/api/friends/getFriendsByStatus?userName=${username}&status=0`
            });
            const { body } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
            const decodedText = decoder.decode(response);
            jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
        } 
        catch(error)
        {
            console.log('GET call failed', error);
        }
    });
</script>

<template>
    <li v-for="request in jsonData">{{  request.ToUserName }}</li>
</template>

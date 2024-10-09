<script setup>
    import { get } from 'aws-amplify/api';
    import { onMounted, ref } from 'vue';
    import { useAuthenticator } from '@aws-amplify/ui-vue';

    const auth = useAuthenticator();
    const username = auth.user?.username;
    const jsonData = ref([]); // Reacted array to hold the list of friendships

    // Get list of friends as JSON 
    onMounted(async () => 
    {
        getFriends();
    });

    // Get authenticated user's friend requests
    async function getFriends() 
    {
        try 
        {
            const restOperation = get({
                apiName: 'yapp',
                path: `/api/friends/getFriendsByStatus?userName=${username}&status=1`
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
    }
</script>

<template>
    <div class="flex-box" v-for="friendship in jsonData">
        <div class="request" v-if="friendship.ToUserName !== username">
            <h4>{{ friendship.ToUserName }}</h4>
            <div class="request-actions">
                <button class="action-button" style="margin-right:10px;">
                    Following
                </button>
            </div>
        </div>
    </div>
</template>

<style>
    .request {
        display: flex;
        justify-content: space-between;
        background-color: var(--amplify-colors-neutral-10);
        margin-bottom: 15px;
        padding: 10px;
        padding-left: 30px;
        padding-right: 30px;
        border-radius: 5px;
        place-items: center;
    }

    .flex-box {
        flex-direction: column;
    }

    .action-button {
        background-color: rgba(183, 143, 175, 0.577);
        color: var(--amplify-colors-purple-100);
        font-weight: bold;
        float: left;
    }
</style>

<script setup>
    import { get, post } from 'aws-amplify/api';
    import { ref, onMounted } from 'vue';
    import { useAuthenticator } from '@aws-amplify/ui-vue';

    const auth = useAuthenticator();
    const username = auth.user?.username;
    const jsonData = ref([]); // Reacted array to hold the list of friendships

    // Get list of friends as JSON 
    onMounted(async () => 
    {
        getRequests();
    });

    async function getRequests() 
    {
        try 
        {
            const restOperation = await get({
                apiName: 'yapp',
                path: `/api/friends/getFriendsByStatus?userName=${username}&status=0`
            });
            const { body } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
            const decodedText = decoder.decode(response);
            console.log(decodedText);
            jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
        } 
        catch(error)
        {
            console.log('GET call failed', error);
        }
    }

    async function acceptRequest(toUser) 
    {
        try 
        {
            const newRequest = 
            {
                "fromUserName": toUser,
                "toUserName": username,
                "status": 1
            };

            const sendPostRequest = post({
                apiName: "yapp",
                path: "/api/friends/updateFriendRequest",
                headers: 
                {
                    'Content-type': 'application/json'
                },
                options: 
                {
                    body: newRequest
                }
            });
            console.log(await sendPostRequest.response);
            alert(`Accepted ${toUser} request!`);
            getRequests();
        } 
        catch (err)
        {
            alert('Failed to accept friend request. Please try again!')
            console.error(err);
        }
    }

    async function declineRequest(toUser) {
        try 
        {
            const newRequest = 
            {
                "fromUserName": toUser,
                "toUserName": username,
                "status": 2
            };

            const sendPostRequest = post({
                apiName: "yapp",
                path: "/api/friends/updateFriendRequest",
                headers: 
                {
                    'Content-type': 'application/json'
                },
                options: 
                {
                    body: newRequest
                }
            });
            console.log(await sendPostRequest.response);
            alert(`Declined ${toUser} request!`);
            getRequests();
        } 
        catch (err)
        {
            alert('Failed to decline friend request. Please try again!')
            console.error(err);
        }
    }
</script>

<template>
    <div class="flex-box">
        <div v-for="request in jsonData">
            <div class="request" v-if="request.FromUserName !== username">
                <h4>{{ request.FromUserName }}</h4>
                <div class="request-actions">
                    <button class="action-button" @click="acceptRequest(request.FromUserName)" style="margin-right:10px;">
                        Accept
                    </button>
                    <button class="action-button" @click="declineRequest(request.FromUserName)">
                        Decline
                    </button>
                </div> 
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

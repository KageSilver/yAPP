<script setup>
    import { get, put } from 'aws-amplify/api';
    import { ref, onMounted } from 'vue';
import { getCurrentUser } from 'aws-amplify/auth';
    import BackBtn from '../components/BackBtn.vue';

const username = ref('');
const jsonData = ref([]);



// Get list of friends as JSON 
onMounted(async () => {
    const user = await getCurrentUser();
    username.value = user.username;
    getRequests();
});

// Get authenticated user's friend requests
async function getRequests() {
    try {
        const restOperation = get({
            apiName: 'yapp',
            path: `/api/friends/getFriendsByStatus?userName=${username.value}&status=0`
        });
        const { body } = await restOperation.response;
        const response = await ((await body.blob()).arrayBuffer());
        const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
        const decodedText = decoder.decode(response);
        jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
        console.log(jsonData.value);
    }
    catch (error) {
        console.log('GET call failed', error);
    }
}

    // Accept toUser's friend request to authenticated user
    async function acceptRequest(request) 
    {
        try 
        {
            const newRequest = 
            {
                "fromUserName": request.FromUserName,
                "toUserName":request.ToUserName,
                "status": 1
            };

            const sendPutRequest = put({
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
            console.log(await sendPutRequest.response);
            alert(`Accepted ${request.FromUserName} request!`);
            getRequests(); // Update the view of pending requests
        } 
        catch (err)
        {
            alert('Failed to accept friend request. Please try again!')
            console.error(err);
        }
    }

    // Decline toUser's friend request to authenticated user
    async function declineRequest(request) {
        if (confirm("Are you sure you want to decline the request?"))
        {
            try 
            {
                const newRequest = 
                {
                    "fromUserName": request.FromUserName,
                    "toUserName": request.ToUserName,
                    "status": 2
                };

                const sendPutRequest = put({
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
                console.log(await sendPutRequest.response);

                alert(`Declined ${request.FromUserName} request!`);
                getRequests(); // Update the view of pending requests
            }
            catch (err)
            {
                alert('Failed to decline friend request. Please try again!')
                console.error(err);
            }
        } 
    }
</script>

<template>


    <div class="pt-[10rem]">
        <div class="flex justify-between items-center w-full px-16 pr-32">
            <div class="flex items-center">
                <BackBtn class="mt-2" />
                <h1 class="text-white text-4xl font-bold ml-8">Friend Requests</h1>
            </div>
            <a class="text-white bg-deep-dark hover:bg-purple font-bold py-2 px-4 rounded text-2xl"  href="/profile/addFriends">
                Add a Friend !
            </a>
        </div>

        <!-- Show this message if the friend list is empty -->
        <div v-if="jsonData.length == 0">
            <h4 class="text-white text-center">Wow... you have no friends!</h4>
        </div>

        <div v-else class="flex-box px-16 pr-32">

            <div v-for="request in jsonData">
                <div class="request p-5 bg-deep-dark text-white" v-if="request.FromUserName !== username">
                    <h4>{{ request.FromUserName }}</h4>
                    <div class="request-actions">
                        <button class="bg-light-pink text-white p-4 font-bold rounded-lg"
                            @click="acceptRequest(request)" style="margin-right:10px;">
                            Accept
                        </button>
                        <button class="bg-light-pink text-white p-4 font-bold rounded-lg"
                            @click="declineRequest(request)">
                            Decline
                        </button>
                    </div>
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

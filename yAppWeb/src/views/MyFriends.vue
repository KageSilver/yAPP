<script setup>
    import { get, put } from 'aws-amplify/api';
    import { onMounted, ref } from 'vue';
    import { getCurrentUser } from 'aws-amplify/auth';
    import ProfileHeader from '../components/ProfileHeader.vue';

    const username = ref('');
    const jsonData = ref([]);



    // Get list of friends as JSON 
    onMounted(async () => 
    {
        const user = await getCurrentUser();
        username.value = user.username;
        getFriends();
    });

    // Get authenticated user's friend requests
    async function getFriends() 
    {
        try 
        {
            const restOperation = get({
                apiName: 'yapp',
                path: `/api/friends/getFriendsByStatus?userName=${username.value}&status=1`
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

function onSubmit(friendship) {
    if (friendship.ToUserName !== username) {
        if (confirm(`Are you sure you want to unfollow ${friendship.ToUserName}?`)) {
            unfollowFriend(friendship, "receiver");
        }
    }
    else {
        if (confirm(`Are you sure you want to unfollow ${friendship.FromUserName}?`)) {
            unfollowFriend(friendship, "sender");
        }
    }
}

// Unfollow sent friend
async function unfollowFriend(friendship, role) {
    try {

        
        const newRequest =
        {
            "fromUserName": friendship.FromUserName,
            "toUserName": friendship.ToUserName,
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
        getFriends(); // Update the list of friends
    }
    catch (err) {
        alert('Failed to decline friend request. Please try again!');
        console.error(err);
    }
}

</script>

<template>
    <ProfileHeader />
    <!-- Show this message if the friend list is empty -->
    <div v-if="jsonData.length === 0">
        <h4 class="text-white text-center">Wow... you have no friends!</h4>
    </div>

    <!-- Display friend list if available -->
    <div v-else>
        <div class="flex-box px-16 pr-32 mt-5" v-for="friendship in jsonData"
            :key="friendship.ToUserName || friendship.FromUserName">
            <div class="request p-5 bg-deep-dark text-white">
                <h4 v-if="friendship.ToUserName !== username">{{ friendship.ToUserName }}</h4>
                <h4 v-else>{{ friendship.FromUserName }}</h4>
                <div class="request-actions">
                    <button class="bg-light-pink text-white p-4 font-bold rounded-lg" @click="onSubmit(friendship)">
                        Unfollow
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

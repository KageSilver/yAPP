<script setup>
    import { post } from 'aws-amplify/api';
    import { useAuthenticator } from '@aws-amplify/ui-vue';
    import { useRouter } from 'vue-router';

    const router = useRouter();
    const auth = useAuthenticator();

    const goHome = async () => 
    {
        router.push('/');
    }

    const goBack = async() => 
    {
        router.push('/dashboard');
    }

    function onSubmit() 
    {
        const sender = auth.user?.username;
        const receiver = document.getElementById("to-username").value;
        var requestButton = document.getElementById("request-button");

        if(receiver !== '') 
        {
            requestButton.disabled = true;
            sendFriendRequest(sender, receiver);
            requestButton.disabled = false;
        } 
        else 
        {
            alert('Enter in a username!');
        }
    }

    async function sendFriendRequest(fromUser, toUser) 
    {
        try 
        {
            const newRequest = 
            {
                "fromUserName": fromUser,
                "toUserId": toUser
            };

            const sendPostRequest = post({
                apiName: "yapp",
                path: "/api/friends/friendRequest",
                headers: 
                {
                    'Content-type': 'application/json'
                },
                options: 
                {
                    body: newRequest
                }
            });

            //TODO: Have Yappers send friend requests through username, 
			// see bug fix issue 139
            await sendPostRequest.response;
            alert('Successfully sent friend request!');
            document.getElementById("to-username").value = '';
        } 
        catch (err)
        {
            alert('Failed to send friend request. Please try again!');
            console.error(err);
        }
    }
</script>

<template>
    <authenticator></authenticator>

    <div class="button-bar" style="display:flex; justify-content:right; margin-bottom:35px;">
        <button class="primary-button" @click="goBack" style="margin-right:35px;">
            Dashboard
        </button>
        <button class="primary-button" @click="goHome">
            Home
        </button>
    </div>

    <h1>Add a Friend!</h1>
    
    <div class="fieldset">
        <label style="margin-bottom: 10px;">Enter in their username: </label>
        <input class="input" id="to-username" type="text">
    </div>

    <br>
    <button class="primary-button" @click="onSubmit" id="request-button">
        Send Request
    </button>
</template>

<style scoped>
		.fieldset {
        align-items: left;
        padding: 30px;
        background-color: var(--amplify-colors-neutral-40);
        color: var(--amplify-colors-neutral-100);
        border-radius: 5px;
    }

    .nav {
        display: flex;
        justify-content: space-between;
    }
</style>

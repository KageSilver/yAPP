<script setup>
import { useRouter } from 'vue-router';
import { post } from 'aws-amplify/api';
import { useAuthenticator } from '@aws-amplify/ui-vue';

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
    try 
    {
        const sender = auth.user?.username;
        const receiver = document.getElementById("to-username").value;
        if(receiver !== '') 
        {
            alert(`From: ${sender} and To: ${receiver}`);
            test(sender, receiver);
        } 
        else 
        {
        alert('Enter in a username!');
        }
    } 
    catch (err) 
    {
      console.log(err);
    }
}

async function test(fromUser, toUser) 
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

        const { body } = await sendPostRequest.response;
        const response = body.json;
        alert(response);
        document.getElementById("to-username").value = '';
    } 
    catch (err)
    {
        alert('Failed to send friend request. Please try again!')
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
    <button class="primary-button" @click="onSubmit">
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

<script setup>
    import {
        get,
        post
    } from 'aws-amplify/api';
    import {
        useAuthenticator
    } from '@aws-amplify/ui-vue';
    import {
        useRouter
    } from 'vue-router';
    import BackBtn from '../components/BackBtn.vue';
    import {
        getCurrentUser
    } from 'aws-amplify/auth';
    import {
        onMounted
    } from 'vue';
    import {
        ref
    } from 'vue';
import BackBtnHeader from '../components/BackBtnHeader.vue';

    const userId = ref('');
    const username = ref('');
    const subheader =ref('');
    onMounted(async () => {
        const user = await getCurrentUser();
        username.value = user.username;
        userId.value = user.userId;
        subheader.value = "You uuid: " + userId.value;

    });


    const router = useRouter();
   



    async function onSubmit() {
        const sender = username.value;
        const receiver = document.getElementById("to-username").value;
        var requestButton = document.getElementById("request-button");

        if (receiver !== '') {
            requestButton.disabled = true;
            sendFriendRequest(sender, receiver);
            requestButton.disabled = false;
        } else {
            alert('Enter in their UUID!');
        }
    }

    async function sendFriendRequest(fromUser, toUser) {
        try {
            const newRequest = {
                "fromUserName": fromUser,
                "toUserId": toUser
            };

            const sendPostRequest = post({
                apiName: "yapp",
                path: "/api/friends/friendRequest",
                headers: {
                    'Content-type': 'application/json'
                },
                options: {
                    body: newRequest
                }
            });
            await sendPostRequest.response;
            alert('Successfully sent friend request!');
            document.getElementById("to-username").value = '';
        } catch (err) {
            alert('Failed to send friend request. Please try again!');
            console.error(err);
        }
    }
</script>

<template>


    <div class="pt-[10rem] md:px-16 md:pr-32">
        <BackBtnHeader header="Add a new Friend!" :subheader="subheader" :backBtn="true" />
        <div class="w-full md:px-16 md:mx-6 mt-3">
            <div class="bg-white p-5 rounded-xl">
                <div class="flex flex-col mb-4">
                    <label for="to-username" class="mb-5 font-bold">Enter their UUID:</label>
                    <input class="input border border-gray-300 rounded p-2" id="to-username" type="text">
                </div>
                <div class="flex justify-end">
                    <button
                        class="bg-dark text-white hover:bg-white hover:text-dark rounded-lg font-bold py-2 px-4 rounded transition-colors"
                        @click="onSubmit" id="request-button">
                        Send Request
                    </button>

                </div>
            </div>
        </div>

    </div>




</template>
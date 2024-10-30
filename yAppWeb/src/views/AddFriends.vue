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
    import LoadingScreen from '../components/LoadingScreen.vue';
    import Alert from '../components/Alert.vue';

    const userId = ref('');
    const username = ref('');
    const subheader = ref('');
    const loading = ref(false);
    const showAlert = ref(false);
    const alertMsg = ref({
        header: '',
        message: ''
    });

    onMounted(async () => {
        const user = await getCurrentUser();
        username.value = user.username;
        userId.value = user.userId;
        subheader.value = "You uuid: " + userId.value;

    });


    const router = useRouter();
    const closeAlert = () => {
        showAlert.value = false;
    };






    const onSubmit = async () => {
        const sender = username.value;
        const receiver = document.getElementById("to-username").value;
        var requestButton = document.getElementById("request-button");

        if (receiver === '') {
            alert('Enter in their UUID!');
        } else if (receiver === userId.value || receiver === username.value) {
            alert('You can\’t add yourself as a friend, silly!');
        } else {
            requestButton.disabled = true;
            await sendFriendRequest(sender, receiver);
            requestButton.disabled = false;
            document.getElementById("to-username").value = '';
        }
    };

const sendFriendRequest = async (fromUser, toUser) => {
        loading.value = true;
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

            alertMsg.value.header = "Yipee!";
            alertMsg.value.message = `Friend request sent to ${toUser}!`;
            showAlert.value = true;
        } catch (err) {
            alertMsg.value.header = "Error!";
            alertMsg.value.message = `Please try again!`;
            showAlert.value = true;
            console.error(err);
        }
        loading.value = false;
    }
</script>

<template>

    <LoadingScreen v-if="loading" />

    <div v-else class="backBtnDiv">
        <BackBtnHeader header="Add a new Friend!" :subheader="subheader" :backBtn="true" />
        <div class="w-full md:px-16 md:mx-6 mt-3">
            <div class="bg-white p-5 rounded-xl">
                <div class="flex flex-col mb-4">
                    <label for="to-username" class="mb-5 font-bold">Enter their UUID:</label>
                    <input class="input" id="to-username" type="text">
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

        <Alert :showModal="showAlert" :header="alertMsg.header" :message="alertMsg.message" :close="closeAlert" />
    </div>
</template>
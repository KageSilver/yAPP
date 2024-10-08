<script setup>
import { useRouter } from 'vue-router';

const router = useRouter()

const goHome = async () => {
  router.push('/')
}

const goBack = async() => {
  router.push('/dashboard')
}

function onSubmit() {
    const receiver = document.getElementById("to-username").value
    alert('sending friend request to: ' + receiver)
}
</script>

<template>
    <div class="button-bar" style="display:flex; justify-content:right; margin-bottom:35px;">
        <button class="primary-button" @click="goBack" style="margin-right:35px;">Dashboard</button>
        <button class="primary-button" @click="goHome">Home</button>
    </div>

    <h1>Add a Friend!</h1>
    
    <div class="fieldset">
        <label style="margin-bottom: 10px;">Enter in their username: </label>
        <input class="input" id="to-username" type="text">
    </div>

    <br>
    <button class="primary-button" @click="onSubmit">Send Request</button>
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
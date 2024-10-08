<script setup>
import { RouterLink } from 'vue-router';

function onSubmit() {
    const receiver = document.getElementById("to-username").value
    console.log('sending friend request to: ', receiver)
}
</script>

<template>
    <h1>Add a Friend!</h1>
    <flex-box class="nav">
        <RouterLink to="/dashboard">Dashboard</RouterLink>
        <RouterLink to="/">Back to Home</RouterLink>
    </flex-box>
    <br>
    <div class="fieldset">
        <label style="margin-right:50px;">Their username: </label>
        <input class="input" id="to-username" type="text">
    </div>

    <br>
    <button class="primary-button" @click="onSubmit">Submit Changes</button>
</template>

<style scoped>
.fieldset {
    display: flex;
    justify-content: space-between;
    padding: 50px;
    background-color: var(--amplify-colors-neutral-40);
    color: var(--amplify-colors-neutral-100);
    border-radius: 5px;
}
.nav {
    display: flex;
    justify-content: space-between;
}
</style>
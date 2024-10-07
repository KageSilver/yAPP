<script setup>
import { RouterLink } from 'vue-router';

</script>

<template>
    <h1>Add a Friend!</h1>
    <flex-box class="nav">
        <RouterLink to="/dashboard">Dashboard</RouterLink>
        <RouterLink to="/">Back to Home</RouterLink>
    </flex-box>
    
</template>

<style scoped>
.nav {
    display: flex;
    justify-content: space-between;
}
</style>
<script setup lang="js">
import { useRouter } from 'vue-router';
const router = useRouter();

function goBack() {
    if (props.back) { // Check if back navigation is enabled
        router.go(-1); // Go back in history
    }
}
</script>
<template>
    <div class="mt-5 mb-0 mx-0">
        <button class="button icon-button" @click="router.back()">
            <i class="material-icons text-[3.2rem] text-white">arrow_back</i>
        </button>
    </div>
</template>
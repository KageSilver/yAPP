<script setup>
import { defineProps } from 'vue';

// Define the props for the component
const props = defineProps({
  showModal: {
    type: Boolean,
    required: true
  },
  close: {
    type: Function,
    required: true
  },
  confirm: {
    type: Function,
    required: true
  },
  header: {
    type: String,
    default: ''
    },
    message: {
        type: String,
        default: ''
    }
  
});
</script>

<template>
  <!-- Show the modal only if showDeleteModal is true -->
  <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center bg-gray-800 bg-opacity-75">
    <div class="bg-white rounded-lg p-6 max-w-lg mx-auto">
      <h3 class="text-lg font-bold mb-4">{{ header }}</h3>
      <p>{{ message }}</p>
      <div class="mt-4 flex justify-end">
        <button @click="close" class="mr-2 px-4 py-2 text-white rounded bg-light-pink">Cancel</button>
        <button @click="confirm" class="px-4 py-2 text-white rounded bg-dark-pink">Confirm </button>
      </div>
    </div>
  </div>
</template>

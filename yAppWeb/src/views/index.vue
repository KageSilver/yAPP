<template>
    <div class="flex h-screen overflow-hidden bg-hero-gradient">
        <div class="flex h-screen overflow-hidden">
            <!-- Sidebar on the right, no padding -->
            <div class="w-64 bg-dark-green h-full">
                <sidebar />
            </div>

            <!-- Main content area on the left, no padding -->
            <div class="flex-1 flex flex-col">
                <topbar />
                <!-- Page content -->
                <main class="flex-1 overflow-y-auto">
                    <router-view></router-view> <!-- Vue Router Outlet -->
                </main>
            </div>
        </div>
</div>
</template>

<script setup lang="ts">
import sidebar from "../components/sidebar.vue";
import topbar from "../components/topbar.vue";
</script>
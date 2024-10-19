
<script setup lang="js">
import SideNav from "../components/SideNav.vue";
import TopNav from "../components/TopNav.vue";
</script>

<template>
    <div class="flex h-screen overflow-hidden bg-hero-gradient">
        <div class="flex h-screen overflow-hidden">
            <!-- Sidebar on the right, no padding -->
            <div class="w-64 bg-dark-green h-full">
                <SideNav />
            </div>

            <!-- Main content area on the left, no padding -->
            <div class="flex-1 flex flex-col">
                <TopNav />
                <!-- Page content -->
                <main class="flex-1 overflow-y-auto">
                    <router-view></router-view> <!-- Vue Router Outlet -->
                </main>
            </div>
        </div>
</div>
</template>


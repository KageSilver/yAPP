<script setup lang="js">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const currentPath = computed(() => route.path);
const isActive = (path) => {
    return computed(() => route.path === path).value;
}

function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('open'); // Toggles sidebar visibility on mobile
}

</script>

<template>


        <!-- Menu toggle button for mobile view -->
        <button id="menu-toggle" class="text-3xl text-white bg-light-pink fixed top-4 right-0 z-30 mt-[7rem] w-18"
            @click="toggleSidebar">
            <span class="material-icons">menu</span>
        </button>

        <!-- Responsive Sidebar/Bottom Bar Container -->
        <div id="sidebar" class="fixed inset-y-0 right-0 w-16 md:w-16 bg-white  z-20 text-center text-xs mt-[7rem]">
            <!-- Navigation Menu -->
            <div class="flex flex-col justify-between h-full">
                <!-- Top Items -->
                <ul class="text-gray-700 flex flex-col items-center justify-start py-8">
                    <!-- Home Item -->
                    <li class="group hover:bg-purple-300 transition-all duration-300  w-full">
                        <a href="#" class="flex flex-col items-center justify-center p-2">
                            <img src="../assets/navigation/home.svg" alt="Home Icon"
                                :class="{ 'w-full h-full': true, 'bg-purple-300 p-2 rounded-full': isActive('/home') }">
                            <span class="font-medium">Home</span>
                        </a>
                    </li>
                    <!-- Profile Item -->
                    <li class="group hover:bg-purple-300 transition-all duration-300  w-full">
                        <a href="#" class="flex flex-col items-center justify-center p-2">
                            <img src="../assets/navigation/profile.svg" alt="Profile Icon"
                                :class="{ 'w-full h-full': true, 'bg-purple-300 p-2 rounded-full': isActive('/profile') }">
                            <span class="font-medium">Profile</span>
                        </a>
                    </li>
                    <!-- Calendar Item -->
                    <li class="group hover:bg-purple-300 transition-all duration-300  w-full">
                        <a href="#" class="flex flex-col items-center justify-center p-2">
                            <img src="../assets/navigation/calendar.svg" alt="Calendar Icon"
                                :class="{ 'w-full h-full': true, 'bg-purple-300 p-2 rounded-full': isActive('/calendar') }">
                            <span class="font-medium">Calendar</span>
                        </a>
                    </li>

                    <!-- Add post Item -->
                    <li class="group hover:bg-purple-300 transition-all duration-300  w-full">
                        <a href="#" class="flex flex-col items-center justify-center p-2">
                            <img src="../assets/navigation/add-post.svg" alt="Add Icon"
                                :class="{ 'w-full h-full': true, 'bg-purple-300 p-2 rounded-full': isActive('/add') }">
                            <span class="font-medium">Create Post</span>
                        </a>
                    </li>
                </ul>
                <!-- Bottom Items -->
                <ul class="text-gray-700 flex flex-col items-center justify-start mt-auto pb-8">
                    <!-- Settings Item -->
                    <li class="group hover:bg-purple-300 transition-all duration-300 w-full"
                        v-if="currentPath === '/settings'">
                        <a href="#" class="flex flex-col items-center justify-center p-2">
                            <img src="../assets/navigation/settings.svg" alt="Add Icon"
                                :class="{ 'w-full h-full': true, 'bg-purple-300 p-2 rounded-full': isActive('/settings') }">
                            <span class="font-medium">Settings</span>
                        </a>
                    </li>
                    <!-- Logout Item -->
                    <li class="group hover:bg-purple-300 transition-all duration-300  w-full">
                        <a href="#" class="flex flex-col items-center justify-center p-2">
                            <img src="../assets/navigation/logout.svg" alt="Add Icon" class="h-full w-full">
                            <span class="font-medium">Logout</span>
                        </a>
                    </li>
                </ul>
            </div>
        </div>
    
</template>

<style scoped>
/* Initial state for mobile view - Sidebar hidden off-screen to the right */
#sidebar {
    transform: translateX(100%);
    transition: transform 0.3s ease;
}

/* State when the sidebar is open - Sidebar slides in */
#sidebar.open {
    transform: translateX(0);
}
li{
    @apply mb-2;
}

@media (min-width: 768px) {

    /* Adjust this for your responsive breakpoint */
    #sidebar {
        transform: none;
        /* Sidebar always visible on larger screens */
    }

    #menu-toggle {
        /* Hide the toggle button on larger screens */
        display: none;
    }
}
</style>
<script setup lang="ts">
  import { ref } from 'vue';

  // State to track whether the menu is open or closed
  var isOpen = ref(false);

  // Function to toggle the menu visibility
  function toggleMenu() {
    isOpen.value = !isOpen.value;
  }
  function closeMenu() {
    isOpen.value = false;
  }
</script>

<template>
  <nav class="navbar">
    <div class="hamburger" @click="toggleMenu">
      <span class="bar"></span>
      <span class="bar"></span>
      <span class="bar"></span>
    </div>
    
    <ul v-if="isOpen" class="menu">
      <li @click="toggleMenu"><router-link to="/">Public Posts</router-link></li>
      <li @click="toggleMenu"><router-link to="/">Your Posts</router-link></li>
      <li @click="toggleMenu"><router-link to="/Diary">Diary</router-link></li>
      <li @click="toggleMenu"><router-link to="/Profile">Profile</router-link></li>
      <!-- Add more menu items as needed -->
    </ul>
  </nav>
  <div id="App" @click="closeMenu">
    <router-view></router-view>
  </div>
</template>

<style scoped>
.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}
.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}
.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}

.navbar {
  position: relative;
  width: 100%;
  background-color: #25364b;
  padding: 10px;
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
}

.hamburger {
  cursor: pointer;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  width: 30px;
  height: 20px;
}

.bar {
  height: 4px;
  width: 100%;
  background-color: white;
  margin: 3px 0;
  transition: 0.4s;
}

.menu {
  position: absolute;
  top: auto;
  left: 0;
  background-color: #25364b;
  padding: 10px;
  width: 200px;
  border-radius: 5px;
  list-style: none;
  display: flex;
  flex-direction: column;
}

.menu li {
  margin-bottom: 10px;
}

.menu li a {
  color: white;
  text-decoration: none;
}

.menu li a:hover {
  color: #ccc;
}
</style>

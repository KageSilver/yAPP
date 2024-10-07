<script setup>
import { TabsContent, TabsIndicator, TabsList, TabsRoot, TabsTrigger } from 'radix-vue';

import MyPosts from "./MyPosts.vue";
import MyFriends from "./MyFriends.vue";
import MyAchievements from "./MyAchievements.vue";
import { useRouter } from 'vue-router';

const router = useRouter()

const goBack = async () => {
  router.push('/')
}

const goToCreatePost = async () => {
  router.push('/create-post')
}

</script>

<template>

  <h1>My Dashboard</h1>
  <TabsRoot
    class="TabsRoot"
    default-value="tab1"
  >

    <TabsList
      class="TabsList"
      aria-label="Manage your account"
    >
      <TabsIndicator class="TagsIndicator ">
        <div
          style="width: 100%; height: 100%"
          class="bg-grass8 w-full h-full"
        />
      </TabsIndicator>
      <TabsTrigger
        class="TabsTrigger"
        value="tab1"
      >
        My Posts
      </TabsTrigger>
      <TabsTrigger
        class="TabsTrigger"
        value="tab2"
      >
        My Friends
      </TabsTrigger>
      <TabsTrigger
        class="TabsTrigger"
        value="tab3"
      >
        My Achievements
      </TabsTrigger>
      <TabsTrigger
        class="TabsTrigger"
        value="tab4"
      >
        Account Settings
      </TabsTrigger>
    </TabsList>

    <TabsContent
      class="TabsContent"
      value="tab1"
    >
      <p class="Text">
        Wow...it's kinda empty in here...
      </p>
      <MyPosts></MyPosts>
    </TabsContent>
    <TabsContent
      class="TabsContent"
      value="tab2"
    >
      <p class="Text">
        Wow...it's kinda empty in here...
      </p>
      <MyFriends></MyFriends>
    </TabsContent>
    <TabsContent
      class="TabsContent"
      value="tab3"
    >
    <p class="Text">
        Wow...it's kinda empty in here...
      </p>
      <MyAchievements></MyAchievements>
    </TabsContent>
    <TabsContent
      class="TabsContent"
      value="tab4"
    >
      <p class="Text">
        Change your Password
      </p>
    </TabsContent>

  </TabsRoot>
  <br>
  <authenticator></authenticator>
  <button class="signoutButton" @click="goBack">Back To Login</button>
  <button class="createPostButton signoutButton" type="button" @click="goToCreatePost">Make a new post!</button>
</template>

<style>
  .TabsRoot {
    display: flex;
    flex-direction: column;
    box-shadow: 0 2px 10px var(--black-a4);
  }

  .TabsList {
    flex-shrink: 0;
    display: flex;
    position: relative;
    background-color: var(--amplify-colors-neutral-10);
    border-top-left-radius: 5px;
    border-top-right-radius: 5px;
    border-width: 1px;
  }

  .TagsIndicator {
    position: absolute;
    padding-left: 2rem;
    left: 0;
    height: 2px;
    bottom: 0;
    width: var(--radix-tabs-indicator-size);
    transform: translateX(var(--radix-tabs-indicator-position));
    border-radius: 9999px; /* rounded-full equivalent */
    background-color: var(--amplify-colors-purple-80);
  }

  .TabsTrigger {
    font-family: inherit;
    background-color: var(--amplify-colors-neutral-10);
    padding: 0 20px;
    height: 45px;
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 15px;
    line-height: 1;
    color: var(--amplify-colors-purple-100);
    font-weight: bold;
    user-select: none;
  }
  .TabsTrigger:first-child {
    border-top-left-radius: 6px;
  }
  .TabsTrigger:last-child {
    border-top-right-radius: 6px;
  }
  .TabsTrigger:hover {
    background-color: var(--amplify-colors-neutral-40);
    color: var(--amplify-colors-purple-80);
  }
  .TabsTrigger[data-state='active'] {
    color: var(--amplify-colors-purple-80);
    border-bottom-color: var(--amplify-colors-purple-80);
    border-width: 2px;
  }
  .TabsTrigger:focus {
    position: relative;
    
  }

  .TabsContent {
    flex-grow: 1;
    padding: 20px;
    color:var(--amplify-colors-purple-100);
    background-color: var(--amplify-colors-neutral-20);
    border-bottom-left-radius: 6px;
    border-bottom-right-radius: 6px;
    outline: none;
  }

  .createPostButton {
    float: right !important;
  }
</style>
<script setup>
  import { Authenticator, useAuthenticator } from "@aws-amplify/ui-vue";
  import "@aws-amplify/ui-vue/styles.css";
  import Home from "./Home.vue";
 
  import { watch } from "vue";
  import { useRouter } from "vue-router";

  const router = useRouter()
  const { route } = useAuthenticator()
  
  watch(route, (newRoute) => {
    if (newRoute === "authenticated") {
      router.push({name: 'home'})
    }
  })
  // reference: https://ui.docs.amplify.aws/vue/connected-components/authenticator/customization#styling
  
</script>

<template>
  <authenticator>
    <template v-slot="{ user, signOut }">
      <h1>Hello!</h1>
      <section>
        <Home></Home>
      </section> 
      <br>
      <button class="signoutButton" @click="signOut">Sign Out</button>
    </template>
  </authenticator>
</template>


<style>
  [data-amplify-authenticator] {    
    --amplify-components-authenticator-router-box-shadow: 0 0 16px var(--amplify-colors-overlay-10);
    --amplify-components-authenticator-router-border-width: 0;

    --amplify-components-authenticator-form-padding: var(--amplify-space-medium) var(--amplify-space-xl) var(--amplify-space-xl);

    --amplify-components-primary-button-border-radius: 5px;
    --amplify-components-button-primary-background-color: var(--amplify-colors-neutral-100);
    --amplify-components-button-primary-hover-background-color: var(--amplify-colors-purple-100);

    --amplify-components-accordion-item-border-radius: 5px;
    --amplify-components-fieldcontrol-focus-box-shadow: 0 0 0 2px var(--amplify-colors-purple-60);
    
    --amplify-components-tabs-item-color: var(--amplify-colors-neutral-80);
    --amplify-components-tabs-item-active-color: var(--amplify-colors-purple-100);
    --amplify-components-tabs-item-hover-color: var(--amplify-colors-purple-100);
    --amplify-components-tabs-item-hover-background-color: var(--amplify-colors-overlay-10);
    
    --amplify-components-button-link-color: var(--amplify-colors-purple-60); 
    --amplify-components-button-link-hover-background-color: var(--amplify-colors-overlay-10);
    --amplify-components-button-link-hover-color: var(--amplify-colors-purple-80); 

    [data-amplify-router] {
      border-radius: 5px;
      opacity: 85%;
    }
  }

  .signoutButton {
    background-color: var(--amplify-colors-neutral-10);
    color: var(--amplify-colors-purple-100);
    font-weight: bold;
  }
  .signoutButton:hover {
    background-color: var(--amplify-colors-neutral-40);
    color: var(--amplify-colors-purple-80);
  }
</style>
<script setup>
  import { Authenticator } from "@aws-amplify/ui-vue";
  import "@aws-amplify/ui-vue/styles.css";
  
</script>

<template>
  <authenticator>
    <!-- FOOTER -->
    <template v-slot:footer>
      <div style="padding: var(--amplify-space-large); text-align: center">
        <p 
        class="amplify-text" 
        style="color: var(--amplify-colors-neutral-40)">
          yAPP © All Rights Reserved
        </p>
      </div>
    </template>


    <!-- LANDING PAGE -->
    <template v-slot="{ user, signOut }">
      <h1>Hello {{ user.username }}!</h1>
      <button class="signoutButton" @click="signOut">Sign Out</button>
    </template>

  </authenticator>
</template>


<style>
  [data-amplify-authenticator] {    
    --amplify-components-authenticator-form-padding: var(--amplify-space-medium) var(--amplify-space-xl) var(--amplify-space-xl);

    --amplify-components-primary-button-border-radius: 5px;
    --amplify-components-button-primary-background-color: var(--amplify-colors-neutral-100);
    --amplify-components-button-primary-hover-background-color: var(--amplify-colors-purple-90);
    --amplify-components-button-primary-active-background-color: var(--amplify-colors-purple-100);

    --amplify-components-accordion-item-border-radius: 5px;

    --amplify-components-fieldcontrol-focus-box-shadow: 0 0 0 2px var(--amplify-colors-purple-60);
    
    --amplify-components-tabs-item-color: var(--amplify-colors-neutral-80);
    --amplify-components-tabs-item-active-color: var(--amplify-colors-purple-100);
    --amplify-components-tabs-item-active-border-color: var(--amplify-colors-purple-100);
    --amplify-components-tabs-item-hover-color: var(--amplify-colors-purple-100);
    --amplify-components-tabs-item-hover-background-color: var(--amplify-colors-overlay-10);
    
    --amplify-components-button-link-color: var(--amplify-colors-purple-60); 
    --amplify-components-button-link-hover-color: var(--amplify-colors-purple-80); 
    --amplify-components-button-link-hover-background-color: var(--amplify-colors-overlay-10);

  }

  .amplify-button[data-amplify-button] {
    border-radius: 5px !important;
    font-weight: bold !important;
  }
  

  [data-amplify-router] {
    box-shadow: 0 0 16px var(--amplify-colors-overlay-10);
    border-width: 0px;
    border-radius: 5px;
    opacity: 85%;
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
<script setup>
  import { Authenticator, useAuthenticator } from '@aws-amplify/ui-vue';
  import { resetPassword } from 'aws-amplify/auth';
  
  // reference to access authenticated user
  // https://ui.docs.amplify.aws/react/connected-components/authenticator/advanced

  // reference to changing password via auth
  // https://docs.amplify.aws/gen1/javascript/prev/build-a-backend/auth/manage-passwords/
  // https://docs.amplify.aws/gen1/vue/build-a-backend/auth/manage-passwords/

  const auth = useAuthenticator()
  const user = auth.user

  const services = {
      async handleSignOut() {
      return resetPassword({user}) // not correct
  }
}
</script>

<template>
  <authenticator :services="services"></authenticator>

  <div class="fieldset">
        <label>Old Password: </label>
        <input class="input" id="oldPassword" type="password">
      </div>

      <br>

      <div class="fieldset">
        <label>New Password: </label>
        <input class="input" id="newPassword" type="password">
      </div>

      <br>

      <button>Submit Changes</button>
</template>

<style>
  .fieldset {
      display: flex;
      justify-content: space-between;
  }

  .input {
      border-radius: 5px;
      background-color: var(--amplify-colors-neutral-80);
  }
</style>

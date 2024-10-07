<script setup>
  import { Authenticator } from '@aws-amplify/ui-vue';

  // reference to access authenticated user
  // https://ui.docs.amplify.aws/react/connected-components/authenticator/advanced

  // reference to changing password via auth
  // https://docs.amplify.aws/gen1/javascript/prev/build-a-backend/auth/manage-passwords/

</script>

<template>
  <!-- NOTE thininking of just redirecting them to the reset password page but without the confirmation code -->
  <authenticator></authenticator>
  <flex-box class="fieldset">
    <label>Old Password: </label>
    <input class="input" id="newPassword" type="password">
  </flex-box>
  <br>
  <flex-box class="fieldset">
    <label>New Password: </label>
    <input class="input" id="newPassword" type="password">
  </flex-box>
  <br>
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

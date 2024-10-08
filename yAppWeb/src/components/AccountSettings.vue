<script setup>
  import { updatePassword } from 'aws-amplify/auth';

  // reference to changing password via auth
  // https://docs.amplify.aws/gen1/javascript/prev/build-a-backend/auth/manage-passwords/

  function onSubmit() {
    const oldPassword = document.getElementById("oldPassword").value; 
    const newPassword = document.getElementById("newPassword").value;
    handleUpdatePassword(oldPassword, newPassword)
  }

  async function handleUpdatePassword(oldPassword, newPassword) {
    try {
      await updatePassword({ oldPassword, newPassword });
      alert('password updated!')
      console.log('password updated!')
    } catch (err) {
      console.log(err);
    }
  }
</script>

<template>
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
  <button class="primary-button" @click="onSubmit">Submit Changes</button>
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

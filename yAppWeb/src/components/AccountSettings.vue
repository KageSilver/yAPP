<script setup>
	import { updatePassword } from 'aws-amplify/auth';

	// Reference to changing password via auth
	// https://docs.amplify.aws/gen1/javascript/prev/build-a-backend/auth/manage-passwords/

	function onSubmit() 
	{
		const oldPassword= document.getElementById("oldPassword").value;
		const newPassword = document.getElementById("newPassword").value;

		if(oldPassword !== '' && newPassword !== '') 
		{
			handleUpdatePassword(oldPassword, newPassword);
		} 
		else 
		{
			alert('Please fill in both fields before submitting!');
		}
	}

	async function handleUpdatePassword(oldPassword, newPassword) 
	{
		try 
		{
			await updatePassword({ oldPassword, newPassword });
			alert('Password updated!');
		} 
		catch 
		{
			alert('Password reset failed!' + 
					'Please ensure you entered your old password correctly' + 
					'and that your new password is a minimum of 8 characters');

			document.getElementById("oldPassword").value = '';
			document.getElementById("newPassword").value = '';
		}
	}
</script>

<template>
	<div class="fieldset">
		<label style="margin-bottom: 10px;">Old Password: </label>
		<input class="input" id="oldPassword" type="password">
	</div>
	<div class="fieldset">
		<label style="margin-bottom: 10px;">New Password: </label>
		<input class="input" id="newPassword" type="password">
	</div>
	<br>
	<button class="primary-button" @click="onSubmit">Submit Changes</button>
</template>

<style>
	.fieldset {
		display: flex;
		margin-bottom: 15px;
		width: 100%;
		flex-direction: column;
		justify-content: flex-start;
	}

	.input {
		border-radius: 5px;
		background-color: var(--amplify-colors-neutral-80);
		flex: 1 0 auto;
		border-radius: 5px;
		padding: 0 10px;
		font-size: 15px;
		line-height: 1;
		height: 35px;
		background-color: var(--amplify-colors-overlay-80);
	}
</style>

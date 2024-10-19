<script setup>
import { updatePassword } from 'aws-amplify/auth';
	import BackBtn from '../components/BackBtn.vue';

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
			router.push('/profile/myposts');
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
	<div class="px-16 pr-32 pt-[10rem]">
		<div class="flex justify-between items-center w-full px-16 pr-32">
			<div class="flex items-center">
				<BackBtn class="mt-2" />
				<h1 class="text-white text-4xl font-bold ml-8">Account Settings </h1>
			</div>
		</div>
		<div class="flex justify-start items-center w-full px-16 pr-32">
			<div class="flex items-center px-16">
				<h1 class="text-white text-sm font-bold ml-5">want to change your password ?</h1>
			</div>
		</div>
		<div class="bg-white text-dark p-16 rounded-xl mx-10">
			<div class="fieldset">
				<label class="mb-5 font-bold">Old Password: </label>
				<input class="input border border-dark text-xl  mb-2 w-full" id="oldPassword" type="password"
					placeholder="Password">
			</div>
			<div class="fieldset">
				<label class="mb-5 font-bold">New Password: </label>
				<input class="input border border-dark text-xl mb-2 w-full" id="newPassword" type="password"
					placeholder="New Password">
			</div>
			<br>
			<button class="bg-dark text-white p-2 rounded-xl w-full" @click="onSubmit">Submit Changes</button>
		</div>
	</div>
</template>

<style>
	.fieldset {
		display: flex;
		margin-bottom: 15px;
		width: 100%;
		flex-direction: column;
		justify-content: flex-start;
	}
	
</style>

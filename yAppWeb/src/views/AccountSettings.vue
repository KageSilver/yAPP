<script setup>
	import { updatePassword } from 'aws-amplify/auth';
	import BackBtnHeader from '../components/BackBtnHeader.vue';

	// Reference to changing password via auth
	// https://docs.amplify.aws/gen1/javascript/prev/build-a-backend/auth/manage-passwords/

	function onSubmit() {
		const oldPassword = document.getElementById("oldPassword").value.trim();
		const newPassword = document.getElementById("newPassword").value.trim();

		if (oldPassword !== '' && newPassword !== '') {
			handleUpdatePassword(oldPassword, newPassword);

		} else {
			alert('Please fill in both fields before submitting! \nEmpty spaces do not count, silly!');
			resetFields();
		}
	}

	function resetFields() {
		document.getElementById("oldPassword").value = '';
		document.getElementById("newPassword").value = '';
	}

	async function handleUpdatePassword(oldPassword, newPassword) {
		try {
			await updatePassword({
				oldPassword,
				newPassword
			});
			
			alert('Password updated!');
			router.push('/profile/myposts');

		} catch {
			alert('Password reset failed!' +
			'\nPlease ensure you entered your old password correctly' +
			'\nand that your new password is a minimum of 8 characters');
			resetFields();
		}
		
	}
</script>

<template>
	<div class="backBtnDiv">
		<BackBtnHeader header="Account Settings" subheader="Want to change your password?" :backBtn="true" />
		<br><br>
		<div class="w-full md:px-16 md:mx-6 mt-3">
			<div class="post-heading bg-white p-8 rounded-lg">
			<div class="form-group w-full mb-4">
				<label for="title" class="mb-5 font-bold">Old Password:</label>
				<input required id="oldPassword" type="password"
					class="input">
			</div>
			<div class="form-group w-full mb-4">
				<label for="title" class="mb-5 font-bold">New Password:</label>
				<input required id="newPassword" type="password"
					class="input">
			</div>
			<br>
			<button class="bg-pink-purple text-white px-5 py-3 rounded-xl w-full" @click="onSubmit">
				Submit Changes
			</button>
			</div>
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
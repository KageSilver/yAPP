<script setup>
	import {
		updatePassword
	} from 'aws-amplify/auth';
	import BackBtn from '../components/BackBtn.vue';
	import BackBtnHeader from '../components/BackBtnHeader.vue';

	// Reference to changing password via auth
	// https://docs.amplify.aws/gen1/javascript/prev/build-a-backend/auth/manage-passwords/

	function onSubmit() {
		const oldPassword = document.getElementById("oldPassword").value;
		const newPassword = document.getElementById("newPassword").value;

		if (oldPassword !== '' && newPassword !== '') {
			handleUpdatePassword(oldPassword, newPassword);
		} else {
			alert('Please fill in both fields before submitting!');
		}
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
				'Please ensure you entered your old password correctly' +
				'and that your new password is a minimum of 8 characters');

			document.getElementById("oldPassword").value = '';
			document.getElementById("newPassword").value = '';
		}
	}
</script>

<template>
	<div class="pt-[10rem] md:px-16 md:pr-32">
		<BackBtnHeader header="Account Settings" subheader="Want to change your password?" :backBtn="true" />

		<div class="w-full md:px-16 md:mx-6 mt-3">
			<div class="post-heading bg-white p-8 rounded-lg">
			<div class="form-group w-full mb-4">
				<label for="title" class="mb-5 font-bold">Old Password:</label>
				<input required id="oldPassword" type="password"
					class="w-full p-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500">
			</div>
			<div class="form-group w-full mb-4">
				<label for="title" class="mb-5 font-bold">New Password:</label>
				<input required id="newPassword" type="password"
					class="w-full p-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500">
			</div>

			<br>
			<button class="bg-pink-purple text-white px-5 py-3 rounded-xl w-full" @click="onSubmit">Submit
				Changes</button>
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
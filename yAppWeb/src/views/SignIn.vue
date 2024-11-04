<script setup lang>
	import { Authenticator } from "@aws-amplify/ui-vue";
	import { getCurrentUser } from "aws-amplify/auth";
	import { Hub } from "aws-amplify/utils";
	import { ref } from "vue";
	import { useRouter } from "vue-router";
	import "@aws-amplify/ui-vue/styles.css";

	const router = useRouter();
	const currentUser = ref(null);
	const services = ["SignIn", "SignUp", "ForgotPassword"]; // Services to be federated

	Hub.listen("auth", async data => {
		const { payload } = data;

		if (payload.event === "signedIn") {
			currentUser.value = await getCurrentUser();
			router.push("/home");
		}

		if (payload.event === "signedOut") {
			currentUser.value = null;
		}
	});
</script>

<template>
	<div class="h-screen bg-signin-gradient">
		<authenticator
			:services="services"
			:handleAuthStateChange="handleAuthStateChange"
		>
			<!-- HEADER -->
			<template v-slot:header>
				<div
					style="
						padding: var(--amplify-space-large);
						text-align: center bg-signin-gradient;
					"
				>
					<img
						class="amplify-image"
						alt="yAPP logo"
						src="../assets/yAPP-icon-light.svg"
						style="
							width: 1000px;
							height: 200px;
							justify-self: center;
							opacity: 90%;
						"
					/>
				</div>
				<div style="padding-bottom: var(--amplify-space-large)">
					<h1 class="text-center text-[3rem] text-white">yAPP</h1>
				</div>
			</template>

			<!-- FOOTER -->
			<template v-slot:footer>
				<div style="padding: var(--amplify-space-large); text-align: center">
					<p
						class="amplify-text"
						style="color: var(--amplify-colors-neutral-40)"
					>
						yAPP © All Rights Reserved
					</p>
				</div>
			</template>
		</authenticator>
	</div>
</template>

<style>
	[data-amplify-authenticator] {
		--amplify-components-authenticator-form-padding: var(--amplify-space-medium)
			var(--amplify-space-xl) var(--amplify-space-xl);

		--amplify-components-accordion-item-border-radius: 5px;

		--amplify-components-button-link-color: var(--amplify-colors-purple-60);
		--amplify-components-button-link-hover-color: var(
			--amplify-colors-purple-80
		);
		--amplify-components-button-link-hover-background-color: var(
			--amplify-colors-overlay-10
		);

		--amplify-components-button-primary-background-color: var(
			--amplify-colors-neutral-100
		);
		--amplify-components-button-primary-hover-background-color: var(
			--amplify-colors-purple-90
		);
		--amplify-components-button-primary-active-background-color: var(
			--amplify-colors-purple-100
		);

		--amplify-components-fieldcontrol-focus-box-shadow: 0 0 0 2px
			var(--amplify-colors-purple-60);

		--amplify-components-primary-button-border-radius: 5px;

		--amplify-components-tabs-item-color: var(--amplify-colors-neutral-80);
		--amplify-components-tabs-item-active-color: var(
			--amplify-colors-purple-100
		);
		--amplify-components-tabs-item-active-border-color: var(
			--amplify-colors-purple-100
		);
		--amplify-components-tabs-item-hover-color: var(
			--amplify-colors-purple-100
		);
		--amplify-components-tabs-item-hover-background-color: var(
			--amplify-colors-overlay-10
		);
	}

	.amplify-button[data-amplify-button] {
		border-radius: 5px !important;
		font-weight: bold !important;
	}

	[data-amplify-router] {
		box-shadow: 0 0 16px var(--amplify-colors-overlay-10);
		border-width: 0px;
		border-radius: 5px;
		opacity: 95%;
	}

	.UUID {
		font-size: x-large;
	}
</style>

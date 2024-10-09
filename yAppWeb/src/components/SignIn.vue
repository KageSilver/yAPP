<script setup lang>
	import { Authenticator, translations } from "@aws-amplify/ui-vue";
	import { I18n } from "aws-amplify/utils";
	import { RouterLink } from "vue-router";
	import { signUp } from "aws-amplify/auth";

	import "@aws-amplify/ui-vue/styles.css";

	// Tweak labels of sign up component by using Internationalization
	// https://ui.docs.amplify.aws/vue/connected-components/authenticator/customization
	I18n.putVocabularies(translations);
	I18n.setLanguage('en');
	I18n.putVocabularies({
		en: {
			'Username': 'Email',
			'Enter your Username': 'Enter your Email',
			'Email': 'Confirm Email',
			'Enter your Email': 'Please confirm your Email'
		}
	});
</script>

<template>
	<authenticator :services="services">
    <!-- HEADER -->
    <template v-slot:header>
      <div style="padding: var(--amplify-space-large); text-align: center">
        <img
          class="amplify-image"
          alt="yAPP logo"
          src="../assets/yAPP-icon-light.svg"
          style="width:1000px; height:200px; justify-self: center; opacity:90%"
        />
      </div>
      <h1 style="text-align: center">yAPP</h1>
    </template>

    <!-- FOOTER -->
    <template v-slot:footer>
      <div style="padding: var(--amplify-space-large); text-align: center">
        <p class="amplify-text" style="color: var(--amplify-colors-neutral-40)">
          yAPP © All Rights Reserved
        </p>
      </div>
    </template>

    <!-- LANDING PAGE -->
    <template v-slot="{ user, signOut }" >
      <h1>Hello {{ user.username }}!</h1>
      <RouterLink to="/dashboard" style="color:var(--amplify-colors-neutral-40);">
        Go to Dashboard
      </RouterLink>
      <br>
      <br>
      <button class="primary-button" @click="signOut">Sign Out</button>
    </template>

  </authenticator>
</template>

<style>
	[data-amplify-authenticator] {    
		--amplify-components-authenticator-form-padding: 
		var(--amplify-space-medium) var(--amplify-space-xl) var(--amplify-space-xl);

		--amplify-components-accordion-item-border-radius: 5px;

		--amplify-components-button-link-color: 
		var(--amplify-colors-purple-60); 
		--amplify-components-button-link-hover-color: 
		var(--amplify-colors-purple-80); 
		--amplify-components-button-link-hover-background-color: 
		var(--amplify-colors-overlay-10);

		--amplify-components-button-primary-background-color: 
		var(--amplify-colors-neutral-100);
		--amplify-components-button-primary-hover-background-color: 
		var(--amplify-colors-purple-90);
		--amplify-components-button-primary-active-background-color: 
		var(--amplify-colors-purple-100);

		--amplify-components-fieldcontrol-focus-box-shadow: 
		0 0 0 2px var(--amplify-colors-purple-60);

		--amplify-components-primary-button-border-radius: 5px;

		--amplify-components-tabs-item-color: 
		var(--amplify-colors-neutral-80);
		--amplify-components-tabs-item-active-color: 
		var(--amplify-colors-purple-100);
		--amplify-components-tabs-item-active-border-color: 
		var(--amplify-colors-purple-100);
		--amplify-components-tabs-item-hover-color: 
		var(--amplify-colors-purple-100);
		--amplify-components-tabs-item-hover-background-color: 
		var(--amplify-colors-overlay-10);
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
</style>

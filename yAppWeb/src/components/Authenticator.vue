<script setup>
  import { Authenticator } from "@aws-amplify/ui-vue";
  import { Amplify } from 'aws-amplify';
  import awsmobile from '../aws-exports';
  import "@aws-amplify/ui-vue/styles.css";

  // reference: https://ui.docs.amplify.aws/vue/connected-components/authenticator/customization#styling
  
  Amplify.configure(awsmobile);
  
  const formFields = {
    signIn: {
      email: {
        label: 'Email',
        placeholder: 'Enter your email',
        isRequired:true,
        type: 'email'
      }
    },
    signUp: {
      username: {
        label: 'Email',
        placeholder: 'Enter your email',
        order: 1,
        isRequired: true,
        type: 'email'
      },
      password: {
        label: 'Password',
        placeholder: 'Enter your password',
        order: 2,
        isRequired: true
      },
    },
    forceNewPassword: {
      password: {
        placeholder: 'Enter your Password:',
      },
    },
    forgotPassword: {
      username: {
        label: 'Email',
        placeholder: 'Enter your email:',
        type: 'email'
      },
    },
    confirmResetPassword: {
      confirmation_code: {
        placeholder: 'Enter your Confirmation Code:',
        label: 'Confirmation Code',
        isRequired: true,
      },
      confirm_password: {
        placeholder: 'Enter your Password Please',
      },
    },
    setupTotp: {
      QR: {
        totpIssuer: 'test issuer',
        totpUsername: 'amplify_qr_test_user',
      },
      confirmation_code: {
        label: 'New Label',
        placeholder: 'Enter your Confirmation Code:',
        isRequired: false,
      },
    },
    confirmSignIn: {
      confirmation_code: {
        label: 'New Label',
        placeholder: 'Enter your Confirmation Code:',
        isRequired: true,
      },
    },
  };
</script>

<template>
  <authenticator :form-fields="formFields">

    <!-- SIGN UP WINDOW -->
    <template v-slot:confirm-sign-up-header>
      <h3
        class="amplify-heading"
        style="padding: var(--amplify-space-xl) 0 0 var(--amplify-space-xl)"
      >
        Please check your email!
      </h3>
    </template>


    <template v-slot:confirm-sign-up-footer>
      <div>Footer Information</div>
    </template>


    <template v-slot:setup-totp-footer>
      <div>Footer Information</div>
    </template>


    <template v-slot:confirm-sign-in-header>
      <h3
        class="amplify-heading"
        style="padding: var(--amplify-space-xl) 0 0 var(--amplify-space-xl)"
      >
        Enter Information:
      </h3>
    </template>
    
    <template v-slot:confirm-sign-in-footer>
      <div>Footer Information</div>
    </template>


    <!-- PASSWORD RESET WINDOW -->
    <template v-slot:forgot-password-header>
      <h3
        class="amplify-heading"
        style="padding: var(--amplify-space-xl) 0 0 var(--amplify-space-xl); text-align: center"
      >
        Uh Oh! You forgor your password?!
      </h3>
    </template>


    <template v-slot:forgot-password-footer>
      <p></p>
    </template>


    <template v-slot:confirm-reset-password-header>
      <h3
        class="amplify-heading"
        style="padding: var(--amplify-space-xl) 0 0 var(--amplify-space-xl)"
      >
        Enter Information:
      </h3>
    </template>

    
    <template v-slot:confirm-reset-password-footer>
      <div></div>
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
    <template v-slot="{ user, signOut }">
      <h1>Hello {{ user.username }}!</h1>
      <button @click="signOut">Sign Out</button>
    </template>

  </authenticator>
</template>


<style>
  [data-amplify-authenticator] {    
    --amplify-components-authenticator-router-box-shadow: 0 0 16px var(--amplify-colors-overlay-10);
    --amplify-components-authenticator-router-border-width: 0;

    --amplify-components-authenticator-form-padding: var(--amplify-space-medium) var(--amplify-space-xl) var(--amplify-space-xl);

    --amplify-components-primary-button-border-radius: 5px;
    --amplify-components-button-primary-background-color: var(--amplify-colors-neutral-100);
    --amplify-components-button-primary-hover-background-color: var(--amplify-colors-purple-100);

    --amplify-components-accordion-item-border-radius: 5px;
    --amplify-components-fieldcontrol-focus-box-shadow: 0 0 0 2px var(--amplify-colors-purple-60);
    
    --amplify-components-tabs-item-color: var(--amplify-colors-neutral-80);
    --amplify-components-tabs-item-active-color: var(--amplify-colors-purple-100);
    --amplify-components-tabs-item-hover-color: var(--amplify-colors-purple-100);
    --amplify-components-tabs-item-hover-background-color: var(--amplify-colors-overlay-10);
    
    --amplify-components-button-link-color: var(--amplify-colors-purple-60); 
    --amplify-components-button-link-hover-background-color: var(--amplify-colors-overlay-10);
    --amplify-components-button-link-hover-color: var(--amplify-colors-purple-80); 

    [data-amplify-router] {
      border-radius: 5px;
      opacity: 85%;
    }
  }
</style>
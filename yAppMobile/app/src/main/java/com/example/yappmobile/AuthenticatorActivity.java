package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.core.Amplify;

public class AuthenticatorActivity extends AppCompatActivity
{
    // NOTE: You no longer need to override onActivityResult.

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Check if we need to sign in or not
        Amplify.Auth.getCurrentUser(
                result ->
                {
                    Log.d("Auth", "There is already a user signed in!");
                },
                error ->
                {
                    Log.e("Auth", "There is no user that is signed in!");
                    invokeSignIn();
                }
        );
        Log.i("Re-route", "Rerouting to Home Activity...");
        startActivity(new Intent(AuthenticatorActivity.this,
                PublicPostsActivity.class));
    }

    private void invokeSignIn()
    {
        // Trigger the Hosted UI sign-in process
        Amplify.Auth.signInWithWebUI(
                this,
                result ->
                {
                    // Handle success
                    Log.d("Auth", "Sign in success");
                },
                error ->
                {
                    // Handle failure
                    Log.e("AuthQuickStart", "Sign in failed", error);
                }
        );
    }
}

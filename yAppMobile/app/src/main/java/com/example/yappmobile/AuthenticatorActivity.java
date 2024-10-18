package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.core.Amplify;

public class AuthenticatorActivity extends AppCompatActivity
{
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_authenticator);

        // Check if we need to sign in or not
        Amplify.Auth.getCurrentUser(
                result ->
                {
                    Log.d("Auth", "There is already a user signed in!");
                },
                error ->
                {
                    Log.e("Auth", "There's a problem signing in!", error);
                    invokeSignIn();
                }
        );
        // TODO: Fix user pool configuration bug and move rerouting
        rerouteToHome();
    }

    // Trigger the Hosted UI sign-in process
    private void invokeSignIn()
    {
        Amplify.Auth.getCurrentUser(
                result ->
                {
                    Log.d("Auth", String.valueOf(result));
                },
                error ->
                {
                    Log.e("Auth", "Oh shit! Current user is giving us problems", error);
                });

        Amplify.Auth.signInWithWebUI(
                this,
                result ->
                {
                    Log.d("Auth", "Sign in success");
                },
                error ->
                {
                    Log.e("Auth", "Sign in failed", error);
                }
        );
    }

    private void rerouteToHome()
    {
        Log.i("Routing", "Rerouting to Public Posts Activity...");
        startActivity(new Intent(AuthenticatorActivity.this,
                PublicPostsActivity.class));
    }
}

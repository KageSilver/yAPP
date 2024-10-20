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
                    Log.i("Auth", "There is already a user signed in!");
                },
                error ->
                {
                    Log.i("Auth", "There is no user signed in!!");
                }
        );
        invokeSignIn();
    }

    // Trigger the Hosted UI sign-in process
    private void invokeSignIn()
    {
        Amplify.Auth.signInWithWebUI(
                this,
                result ->
                {
                    Log.i("Auth", "Sign in success");
                    rerouteToHome();
                },
                error ->
                {
                    Amplify.Auth.signOut(
                            result ->
                            {
                                Log.i("Auth", "Signing out previous user...");
                                invokeSignIn();
                            }
                    );
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

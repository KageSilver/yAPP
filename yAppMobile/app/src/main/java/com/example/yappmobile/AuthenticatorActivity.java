package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.NaviBarDestinations.PublicPostsActivity;

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
                    Log.i("Auth", "Continuing as " + result.getUsername() + "...");
                    rerouteToHome();
                },
                error ->
                {
                    Log.i("Auth", "There is no user signed in!!");
                    invokeSignIn();
                }
        );
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
                    Log.e("Auth", "Sign in failed. Trying again...", error);
                    Amplify.Auth.signOut(
                            result ->
                            {
                                Log.i("Auth", "Signing out previous user...");
                                invokeSignIn();
                            }
                    );
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

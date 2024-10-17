package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.core.Amplify;

public class AuthenticatorActivity extends AppCompatActivity
{
    // Reference:
    // https://docs.amplify.aws/gen1/android/prev/build-a-backend/auth/sign-in-with-web-ui/

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
                    Log.e("Auth", "There is no user that is signed in!");
                    invokeSignIn();
                }
        );
        Log.i("Routing", "Rerouting to Public Posts Activity...");
        startActivity(new Intent(AuthenticatorActivity.this,
                PublicPostsActivity.class));
    }

    private void invokeSignIn()
    {
        // Trigger the Hosted UI sign-in process and log end result
        Amplify.Auth.signInWithWebUI(
                this,
                result ->
                {
                    Log.d("Auth", "Sign in success");
                },
                error ->
                {
                    Log.e("AuthQuickStart", "Sign in failed", error);
                }
        );
    }
}

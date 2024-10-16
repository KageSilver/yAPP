package com.example.yappmobile;

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
        setContentView(R.layout.activity_main);

        Log.d("Auth", "Before signInWithWebUI");

        // Trigger the Hosted UI sign-in process
        Amplify.Auth.signInWithWebUI(
                this,
                result -> {
                    // Handle success: Redirect to a new screen
                    Log.d("Auth", "Sign in success");
                },
                error -> {
                    // Handle failure
                    Log.e("AuthQuickStart", "Sign in failed", error);
                }
        );
        Log.d("Auth", "After signInWithWebUI");

        //startActivity(new Intent(AuthenticatorActivity.this,PublicPostsActivity.class));
    }
    // You no longer need to override onActivityResult.
}

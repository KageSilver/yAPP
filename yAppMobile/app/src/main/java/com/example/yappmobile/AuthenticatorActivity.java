package com.example.yappmobile;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.NaviBarDestinations.PublicPostsActivity;

public class AuthenticatorActivity extends AppCompatActivity {
    private SharedPreferences sharedPreferences; // Declare SharedPreferences

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_authenticator);

        // Initialize SharedPreferences
        sharedPreferences = getSharedPreferences("yAppPreferences", Context.MODE_PRIVATE);

        // Check if we need to sign in or not
        Amplify.Auth.getCurrentUser(result -> {
            Log.i("Auth", "Continuing as " + result.getUsername() + "...");
            // Set a preference value
            SharedPreferences.Editor editor = sharedPreferences.edit();
            editor.putString("username", result.getUsername()); // Save the username
            editor.putString("uuid",result.getUserId());
            editor.apply(); // Save changes asynchronously

            rerouteToHome();
        }, error -> {
            Log.i("Auth", "There is no user signed in!!");
            invokeSignIn();
        });
    }

    // Trigger the Hosted UI sign-in process
    private void invokeSignIn() {
        Amplify.Auth.signInWithWebUI(this, result -> {
            Log.i("Auth", "Sign in success");

            // Get current user information
            Amplify.Auth.getCurrentUser(
                    currentUser -> {
                        Log.i("Auth", "User signed in: " + currentUser.getUsername());

                        // Save username to SharedPreferences
                        SharedPreferences.Editor editor = sharedPreferences.edit();
                        editor.putString("username", currentUser.getUsername());
                        editor.putString("uuid", currentUser.getUserId());
                        editor.apply();
                    },
                    error -> {
                        Log.e("Auth", "Failed to get current user after sign-in", error);
                    }
            );

            rerouteToHome();
        }, error -> {
            Log.e("Auth", "Sign in failed. Trying again...", error);
            Amplify.Auth.signOut(result -> {
                Log.i("Auth", "Signing out previous user...");
                invokeSignIn();
            });
        });
    }


    private void rerouteToHome() {
        Log.i("Routing", "Rerouting to Public Posts Activity...");
        startActivity(new Intent(AuthenticatorActivity.this, PublicPostsActivity.class));
    }
}


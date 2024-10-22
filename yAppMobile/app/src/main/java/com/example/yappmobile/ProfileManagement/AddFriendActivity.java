package com.example.yappmobile.ProfileManagement;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.auth.AuthUser;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.R;
import com.google.android.material.textfield.TextInputLayout;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.CompletableFuture;

public class AddFriendActivity extends AppCompatActivity
{
    private AlertDialog success;
    private AlertDialog failure;
    private JSONObject newRequest;
    private TextInputLayout requestField;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_add_friend);

        // Set the activity to be full screen
        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        requestField = findViewById(R.id.request);
        initializeSuccessDialog();
        initializeFailureDialog();
        initializeNewRequest();

        // Set up back button code
        ImageButton backButton = findViewById(R.id.back_button);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(AddFriendActivity.this, MyRequestsActivity.class);
                startActivity(intent);
            }
        });

        // Set up add friend button code
        Button addFriendButton = findViewById(R.id.send_request_button);
        addFriendButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                createRequest();
            }
        });
    }

    private void createRequest()
    {
        String receiver = requestField.getEditText().getText().toString();
        if (!receiver.equals(""))
        {
            CompletableFuture<AuthUser> future = new CompletableFuture<>();
            Amplify.Auth.getCurrentUser(future::complete, error -> {
                Log.e("Auth", "Error occurred when getting current user. Redirecting to authenticator");
                startActivity(new Intent(AddFriendActivity.this, AuthenticatorActivity.class));
            });

            future.thenAccept(user -> {
                runOnUiThread(() -> {
                    if (user.getUsername().equals(receiver) || user.getUserId().equals(receiver))
                    {
                        Log.d("Faulty Form Field", "You can't add yourself as a friend, silly!");
                    }
                    else
                    {
                        sendPostRequest(user.getUsername(), receiver);
                    }
                    success.show();
                });
            });
        }
        else
        {
            Log.d("Faulty Form Field", "You have to enter in something, silly!");
        }
    }

    private void sendPostRequest(String sender, String receiver)
    {
        // Fill out newRequest with valid information
        try
        {
            newRequest.put("fromUserName", sender);
            newRequest.put("toUserId", receiver);
        }
        catch (JSONException e)
        {
            Log.e("JSON", "Exception occurred when adding elements to JSONObject", e);
        }

        // Send out newRequest through API call
        try
        {
            String stringPost = newRequest.toString();
            String apiUrl = "/api/friends/friendRequest";
            RestOptions options = RestOptions.builder()
                                             .addPath(apiUrl)
                                             .addBody(stringPost.getBytes())
                                             .addHeader("Content-Type", "application/json")
                                             .build();
            Amplify.API.post(options,
                             response -> Log.i("API", "POST response: " + response.getData().asString()),
                             error -> Log.e("API", "POST request failed", error));
        }
        catch (Exception e)
        {
            e.printStackTrace();
            failure.show();
        }
    }

    private void initializeSuccessDialog()
    {
        success = new AlertDialog.Builder(this).create();
        success.setTitle("Successfully sent friend request!");
        success.setButton(AlertDialog.BUTTON_POSITIVE, "Heck yeah", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                startActivity(new Intent(AddFriendActivity.this, MyRequestsActivity.class));
            }
        });
    }

    private void initializeFailureDialog()
    {
        failure = new AlertDialog.Builder(this).create();
        failure.setTitle("Failed to send friend request... Try again!");
        failure.setButton(AlertDialog.BUTTON_POSITIVE, "Aw man...", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                failure.dismiss();
            }
        });
    }

    private void initializeNewRequest()
    {
        newRequest = new JSONObject();
        try
        {
            newRequest.put("fromUserName", "");
            newRequest.put("toUserId", "");
            newRequest.put("status", 0);
        }
        catch (JSONException e)
        {
            Log.e("JSONException", "Something went wrong when initializing a JSONObject", e);
        }
    }
}

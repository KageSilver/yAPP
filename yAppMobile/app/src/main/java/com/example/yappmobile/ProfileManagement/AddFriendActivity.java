package com.example.yappmobile.ProfileManagement;

import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.R;
import com.google.android.material.textfield.TextInputLayout;

import org.json.JSONException;
import org.json.JSONObject;

public class AddFriendActivity extends AppCompatActivity
{
    private AlertDialog success;
    private AlertDialog failure;
    private TextInputLayout requestField;
    private boolean isValidReceiver = true;
    private final int RED = Color.parseColor("#FF0000");

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        // Set the activity to be full screen
        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        setContentView(R.layout.activity_add_friend);

        requestField = findViewById(R.id.request);
        initializeSuccessDialog();
        initializeFailureDialog();

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
                createRequest(requestField.getEditText().getText().toString());
            }
        });
    }

    private void createRequest(String receiver)
    {
        JSONObject newRequest = new JSONObject();
        checkFields(receiver);
        if(isValidReceiver)
        {
            // Fill out newPost
            Amplify.Auth.getCurrentUser(
                    result ->
                    {
                        String userName = result.getUsername();
                        try
                        {
                            // TODO
                            newRequest.put("", "");
                            newRequest.put("", "");
                            newRequest.put("", "");
                        }
                        catch (JSONException e)
                        {
                            Log.e("JSON", "Exception occurred when adding elements to JSONObject", e);
                        }
                    },
                    error ->
                    {
                        Log.e("Auth", "Failed to get current user", error);
                    }
            );

            // Send out newRequest through API call
            try
            {
                String stringPost = newRequest.toString();
                sendPostRequest("", stringPost);
            }
            catch (Exception e)
            {
                e.printStackTrace();
                failure.show();
            }
        }
    }

    private void sendPostRequest(String apiUrl, String requestData)
    {
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addBody(requestData.getBytes())
                .addHeader("Content-Type", "application/json")
                .build();
        Amplify.API.post(options,
                response -> Log.i("API", "POST response: " + response.getData().asString()),
                error -> Log.e("API", "POST request failed", error)
        );
    }

    private void checkFields(String sentReceiver)
    {
        if(sentReceiver.equals(""))
        {
            isValidReceiver = false;
            Log.d("Faulty Form Field", "You have to enter in something, silly!");
            // TODO: envoke some kind of feedback
        }
        else
        {
            Amplify.Auth.getCurrentUser(
                    result ->
                    {
                        if(result.getUserId().equals(sentReceiver) || result.getUsername().equals(sentReceiver))
                        {
                            isValidReceiver = false;
                            Log.d("Faulty Form Field", "You can't add yourself as a friend, silly!");
                            // TODO: envoke some kind of feedback
                        }
                    },
                    error ->
                    {
                        Log.e("Auth", "Error occurred when getting current user. Redirecting to authenticator");
                        Intent intent = new Intent(AddFriendActivity.this, AuthenticatorActivity.class);
                        startActivity(intent);
                    }
            );
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
                Intent intent = new Intent(AddFriendActivity.this, MyRequestsActivity.class);
                startActivity(intent);
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

    private JSONObject initializeNewRequest()
    {
        JSONObject newRequest = new JSONObject();
        try
        {
            // TODO find out the names needed lol
            newRequest.put("", "");
            newRequest.put("", "");
            newRequest.put("status", 0);
        }
        catch(JSONException e)
        {
            Log.e("JSONException", "Something went wrong when initializing a JSONObject", e);
        }
        return newRequest;
    }
}

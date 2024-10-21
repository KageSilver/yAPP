package com.example.yappmobile.NaviBarDestinations;

import android.content.DialogInterface;
import android.content.Intent;
import android.content.res.ColorStateList;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.R;
import com.google.android.material.textfield.TextInputLayout;

import org.json.JSONException;
import org.json.JSONObject;

public class CreatePostActivity extends AppCompatActivity
{
    private JSONObject newPost;
    private TextInputLayout titleText;
    private TextInputLayout contentText;
    private String postTitle;
    private String postBody;
    private AlertDialog successDialog;
    private AlertDialog failureDialog;
    private AlertDialog discardDialog;
    private final int RED = Color.parseColor("#FF0000");

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        // Set the activity to be full screen
        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        setContentView(R.layout.activity_create_post);

        newPost = new JSONObject();
        try
        {
            newPost.put("userName", "");
            newPost.put("postTitle", "");
            newPost.put("postBody", "");
            newPost.put("diaryEntry", false);
            newPost.put("anonymous", true);
        }
        catch (Exception e)
        {
            System.out.println("Exception occurred when adding elements to json object: " + e);
        }

        titleText = findViewById(R.id.post_title);
        contentText = findViewById(R.id.post_content);

        postTitle = titleText.getEditText().getText().toString();
        postBody = contentText.getEditText().getText().toString();

        initializeSuccessDialog();
        initializeFailureDialog();
        initializeDiscardDialog();

        // Set up discard button code
        Button discardPostButton = findViewById(R.id.discard_button);
        discardPostButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                if(isEmptyPost(postTitle, postBody))
                {
                    Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                    startActivity(intent);
                }
                else
                {
                    discardDialog.show();
                }
            }
        });

        // Set up discard button code
        Button createPostButton = findViewById(R.id.create_button);
        createPostButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                createPost(v);
            }
        });
    }

    public void createPost(View view)
    {
        // Make API call when invoked
        if (!isEmptyPost(postTitle, postBody))
        {
            // Fill out newPost
            Amplify.Auth.getCurrentUser(
                    result ->
                    {
                        String userName = result.getUsername();
                        try
                        {
                            newPost.put("postTitle", postTitle);
                            newPost.put("postBody", postBody);
                            newPost.put("userName", userName);
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

            // send out newPost through API call
            try
            {
                String stringPost = newPost.toString();
                sendPost("/api/posts/createPost", stringPost);
                successDialog.show();
            }
            catch (Exception e)
            {
                e.printStackTrace();
                failureDialog.show();
            }
        }
        else if (postTitle.equals(""))
        {
            titleText.setBoxStrokeColor(RED);
            titleText.setHint("Title required!!");
            titleText.setHintTextColor(ColorStateList.valueOf(RED));
        }
        else
        {
            contentText.setBoxStrokeColor(RED);
            contentText.setHint("Post content required!!");
            contentText.setHintTextColor(ColorStateList.valueOf(RED));
        }
    }

    private void sendPost(String apiUrl, String postData)
    {
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addBody(postData.getBytes())
                .addHeader("Content-Type", "application/json")
                .build();
        Amplify.API.post(options,
                response -> Log.i("API", "POST response: " + response.getData().asString()),
                error -> Log.e("API", "POST request failed", error)
                );
    }

    private boolean isEmptyPost(String postTitle, String postBody)
    {
        return postTitle.trim().isEmpty() && postBody.trim().isEmpty();
    }

    private void initializeSuccessDialog()
    {
        // Create post alert dialog
        successDialog = new AlertDialog.Builder(this).create();
        successDialog.setTitle("Post successfully created!");
        successDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Heck yeah", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });
    }

    private void initializeFailureDialog()
    {
        // Create post alert dialog
        failureDialog = new AlertDialog.Builder(this).create();
        failureDialog.setTitle("Post failed to create... Try again!");
        failureDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Aw man...", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                failureDialog.dismiss();
            }
        });
    }

    private void initializeDiscardDialog()
    {
        discardDialog = new AlertDialog.Builder(this).create();
        discardDialog.setTitle("Discard Changes?");
        discardDialog.setMessage("Are you really sure that you want to discard your changes??");
        discardDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Yes", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });
        discardDialog.setButton(AlertDialog.BUTTON_NEGATIVE, "No, keep editing", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                System.out.println("Keep editing");
            }
        });
    }
}

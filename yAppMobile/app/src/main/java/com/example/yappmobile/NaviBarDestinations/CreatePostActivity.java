package com.example.yappmobile.NaviBarDestinations;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;

import androidx.appcompat.app.AlertDialog;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.BasePostActivity;
import com.example.yappmobile.R;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.CompletableFuture;

public class CreatePostActivity extends BasePostActivity
{

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        initializeSuccessDialog();
        initializeDiaryLimitDialog();
        initializeFailureDialog();
        initializeDiscardDialog();
        initializeNewPost();

        // set up toggles
        diaryEntry.setVisibility(View.VISIBLE);
        diaryEntry.setChecked(false);
        anonymous.setVisibility(View.GONE);
        anonymous.setChecked(true);
        findViewById(R.id.divider).setVisibility(View.VISIBLE);

        discardButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                postTitle = titleText.getEditText().getText().toString();
                postBody = contentText.getEditText().getText().toString();

                if (postTitle.equals("")&&postBody.equals("")) {
                    Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                    startActivity(intent);
                } else {
                    discardDialog.show();
                }
            }
        });


        actionButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                createPost();
                actionButton.setEnabled(false);
            }
        });

    }

    private void sendPost(String postData)
    {
        String apiUrl = "/api/posts/createPost";
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addBody(postData.getBytes())
                .addHeader("Content-Type", "application/json")
                .build();
        Amplify.API.post(options,
                response -> {
                    Log.i("API", "POST response: " + response.getData().asString());
                    runOnUiThread(() -> {
                        if(response.getData().asString().equals("\"Cannot make more than one diary entry a day\""))
                        {
                            System.out.println("diary limit dialog");
                            diaryLimitDialog.show();
                        }
                        else
                        {
                            System.out.println("success dialog");
                            successDialog.show();
                        }
                        actionButton.setEnabled(true);
                    });
                },
                error -> {
                    Log.e("API", "POST request failed", error);
                });
    }

    private void createPost()
    {
        postTitle = titleText.getEditText().getText().toString();
        postBody = contentText.getEditText().getText().toString();

        // Make API call when invoked
        if (hasFilledForms(postTitle, postBody))
        {
            CompletableFuture<String> future = new CompletableFuture<>();
            Amplify.Auth.getCurrentUser(result -> {
                future.complete(result.getUserId());
            }, error -> {
                Log.e("Auth", "Error occurred when getting current user. Redirecting to authenticator");
                Intent intent = new Intent(CreatePostActivity.this, AuthenticatorActivity.class);
                startActivity(intent);
            });
            future.thenAccept(uid -> {
                runOnUiThread(() -> {
                    try
                    {
                        post.put("postTitle", postTitle);
                        post.put("postBody", postBody);
                        post.put("uid", uid);
                        post.put("diaryEntry", diaryEntry.isChecked());
                        post.put("anonymous", anonymous.isChecked());
                    }
                    catch (JSONException e)
                    {
                        Log.e("JSON", "Exception occurred when adding elements to JSONObject", e);
                    }

                    // send out newPost through API call
                    try
                    {
                        String stringPost = post.toString();
                        sendPost(stringPost);
                    }
                    catch (Exception e)
                    {
                        e.printStackTrace();
                        failureDialog.show();
                    }
                });
            }).exceptionally(throwable -> {
                Log.e("API", "Error fetching data", throwable);
                return null;
            });
        }
        else if (postTitle.equals(""))
        {
            Log.d("Faulty Form Field", "You have to put in a title, silly!");
            titleText.setError("You have to put in a title, silly!");
        }
        else
        {
            Log.d("Faulty Form Field", "You have to add content, silly!");
            contentText.setError("You have to add content, silly!");
        }
    }


    public void initializeSuccessDialog()
    {
        // Create post alert dialog
        successDialog = new AlertDialog.Builder(this).create();
        successDialog.setTitle("Post successfully created!");
        successDialog.setButton(AlertDialog.BUTTON_POSITIVE,
                                "Heck yeah", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });
    }

    public void initializeDiaryLimitDialog()
    {
        // Create post alert dialog
        diaryLimitDialog = new AlertDialog.Builder(this).create();
        diaryLimitDialog.setTitle("You can't make more than one diary entry per day :(");
        diaryLimitDialog.setButton(AlertDialog.BUTTON_POSITIVE,
                "Aw man...", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                diaryLimitDialog.dismiss();
            }
        });
    }

    public void initializeFailureDialog()
    {
        // Create post alert dialog
        failureDialog = new AlertDialog.Builder(this).create();
        failureDialog.setTitle("Post failed to create... Try again!");
        failureDialog.setButton(AlertDialog.BUTTON_POSITIVE,
                                "Aw man...", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                failureDialog.dismiss();
            }
        });
    }

    public void initializeDiscardDialog()
    {
        discardDialog = new AlertDialog.Builder(this).create();
        discardDialog.setTitle("Woah there!");
        discardDialog.setMessage("Are you really sure that you want to discard your changes?");
        discardDialog.setButton(AlertDialog.BUTTON_POSITIVE,
                                "Yes", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });
        discardDialog.setButton(AlertDialog.BUTTON_NEGATIVE,
                                "No, keep editing", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                System.out.println("Keep editing");
            }
        });
    }

    private void initializeNewPost()
    {
        post = new JSONObject();
        try
        {
            post.put("postTitle", "");
            post.put("postBody", "");
            post.put("uid", "");
            post.put("diaryEntry", false);
            post.put("anonymous", true);
        }
        catch (JSONException e)
        {
            Log.e("JSON", "Exception occurred when creating a JSONObject", e);
        }
    }
}

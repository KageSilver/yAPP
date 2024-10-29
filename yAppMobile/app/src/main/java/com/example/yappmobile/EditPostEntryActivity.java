package com.example.yappmobile;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;

import org.json.JSONException;
import org.json.JSONObject;


public class EditPostEntryActivity extends BasePostActivity {

    private JSONObject _currentPost;
    private final String LOG_NAME ="EDIT_POST";
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //set up

        initializeDiscardDialog();
        initializeFailureDialog();
        initializeSuccessDialog();
        //get the intent value
        Intent intent = getIntent();
        String jsonString = intent.getStringExtra("post");
        if (jsonString != null) {
            try {
                // Convert the String back into JSONObject
                _currentPost = new JSONObject(jsonString);

                // Now you can access the data inside the JSONObject
                String title = _currentPost.get("postTitle").toString();
                String body= _currentPost.get("postBody").toString();
                boolean isDiary  = Boolean.parseBoolean(_currentPost.getString("diaryEntry"));
                diaryEntry.setChecked(isDiary);
                titleEditText.setText(title);
                contentEditText.setText(body);


            } catch (JSONException e) {
                Toast.makeText(getApplicationContext(),"Something happened!Please try again", Toast.LENGTH_SHORT).show();
                Log.e(LOG_NAME,e.getMessage());
            }
        }

        discardButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                postTitle = titleText.getEditText().getText().toString();
                postBody = contentText.getEditText().getText().toString();
                if (!hasFilledForms(postTitle, postBody)) {
                    try {
                        redirectToPostEntry();
                    } catch (JSONException e) {
                        Toast.makeText(getApplicationContext(),"Something happened!Please try again", Toast.LENGTH_SHORT).show();
                        Log.e(LOG_NAME,"Failed to discard! "+e.getMessage());
                    }
                } else {
                    discardDialog.show();
                }
            }
        });


        actionButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                updatePost();
            }
        });

    }

    private void redirectToPostEntry() throws JSONException {
        Intent intent = new Intent(EditPostEntryActivity.this, PostEntryActivity.class);
        intent.putExtra("pid",_currentPost.get("pid").toString());
        intent.putExtra("uid",_currentPost.get("uid").toString());
        startActivity(intent);
        finish();
    }

    private void sendPost(String postData)
    {
        String apiUrl = "/api/posts/updatePost";
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addBody(postData.getBytes())
                .addHeader("Content-Type", "application/json")
                .build();
        Amplify.API.put(options,
                response -> {  // Corrected "response" spelling
                    Log.i(LOG_NAME, "PUT response: " + response.getData().asString());
                    runOnUiThread(() -> {
                        Toast.makeText(getApplicationContext(),"Yipee! Update successfully",Toast.LENGTH_SHORT).show();
                        try {
                            redirectToPostEntry();
                        } catch (JSONException e) {
                            Toast.makeText(getApplicationContext(),"Something happened!Please try again", Toast.LENGTH_SHORT).show();
                            Log.e(LOG_NAME,"Redirect "+e.getMessage());
                        }
                    });
                },
                error -> {
                    Log.e(LOG_NAME, "PUT failed: ", error);
                    runOnUiThread(() -> {
                        Toast.makeText(getApplicationContext(),"Error! Failed to update",Toast.LENGTH_SHORT).show();
                    });
                });
    }

    private void updatePost()
    {
        postTitle = titleText.getEditText().getText().toString();
        postBody = contentText.getEditText().getText().toString();

        // Make API call when invoked
        if (hasFilledForms(postTitle, postBody))
        {
            try{

                _currentPost.put("postBody", postBody);
                _currentPost.put("diaryEntry", diaryEntry.isChecked());
                _currentPost.put("postTitle", postTitle);
                Log.i(LOG_NAME,post.toString());

            }catch (Exception error){
                    Log.e(LOG_NAME, "Get all values for post: " +error.getMessage());
            }
            //after having all that

            sendPost(_currentPost.toString());


        }
        else if (postTitle.equals(""))
        {
            Log.d(LOG_NAME, "You have to put in a title, silly!");
            titleText.setError("You have to put in a title, silly!");
        }
        else
        {
            Log.d(LOG_NAME, "You have to add content, silly!");
            contentText.setError("You have to add content, silly!");
        }
    }


    public void initializeSuccessDialog()
    {
        // Create post alert dialog
        successDialog = new AlertDialog.Builder(this).create();
        successDialog.setTitle("Update successfully created!");
        successDialog.setButton(AlertDialog.BUTTON_POSITIVE,
                "Heck yeah", new DialogInterface.OnClickListener()
                {
                    public void onClick(DialogInterface dialog, int id)
                    {
                        Intent intent = new Intent(EditPostEntryActivity.this, PostEntryActivity.class);
                        startActivity(intent);
                    }
                });
    }


    public void initializeFailureDialog()
    {
        // Create post alert dialog
        failureDialog = new AlertDialog.Builder(this).create();
        failureDialog.setTitle("Update failed to create... Try again!");
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
                        Intent intent = new Intent(EditPostEntryActivity.this, PostEntryActivity.class);
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
}
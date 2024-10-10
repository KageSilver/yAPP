package com.example.yappmobile;

import android.content.DialogInterface;
import android.content.res.ColorStateList;
import android.graphics.Color;
import android.os.Bundle;

import android.content.Intent;
import android.view.View;
import android.widget.Button;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.app.AlertDialog;

import com.google.android.material.textfield.TextInputLayout;

import org.json.JSONObject;

import android.util.Log;
import com.amplifyframework.core.Amplify;
import com.amplifyframework.api.rest.RestOptions;

public class CreatePostActivity extends AppCompatActivity {
    private JSONObject newPost;
    private TextInputLayout titleText;
    private TextInputLayout contentText;
    private AlertDialog successDialog;
    private AlertDialog failureDialog;

    @Override
    public void onCreate(Bundle savedInstanceState){
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_create_post);

        newPost = new JSONObject();
        try {
            newPost.put("userName", "");
            newPost.put("postTitle", "");
            newPost.put("postBody", "");
            newPost.put("diaryEntry", false);
            newPost.put("anonymous", true);
        } catch (Exception e) {
            System.out.println("Exception occurred when adding elements to json object: " + e);
        }
        titleText = findViewById(R.id.post_title);
        contentText = findViewById(R.id.post_content);

        // Discard button alert dialog
        Button discardPost = findViewById(R.id.discard_button);
        AlertDialog discardDialog = new AlertDialog.Builder(this).create();
        discardDialog.setTitle("Discard Changes??");
        discardDialog.setMessage("Are you really sure that you want to discard your changes??");
        discardDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Yes", new DialogInterface.OnClickListener() {
            public void onClick(DialogInterface dialog, int id) {
                Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });
        discardDialog.setButton(AlertDialog.BUTTON_NEGATIVE, "No, keep editing", new DialogInterface.OnClickListener() {
            public void onClick(DialogInterface dialog, int id) {
                System.out.println("Keep editing");
            }
        });
        discardPost.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                if ( discardPost() ) {
                    discardDialog.show();
                } else {
                    Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                    startActivity(intent);
                }
            }
        });

        // Create post alert dialog
        successDialog = new AlertDialog.Builder(this).create();
        successDialog.setTitle("Post successfully created!");
        successDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Heck yeah", new DialogInterface.OnClickListener() {
            public void onClick(DialogInterface dialog, int id) {
                Intent intent = new Intent(CreatePostActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });

        // Create post alert dialog
        failureDialog = new AlertDialog.Builder(this).create();
        failureDialog.setTitle("Post failed to create... Try again!");
        failureDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Aw man...", new DialogInterface.OnClickListener() {
            public void onClick(DialogInterface dialog, int id) {
                failureDialog.dismiss();
            }
        });
    }//end onCreate

    public void createPost(View view) {
        // Make API call when invoked
        String postTitle = titleText.getEditText().getText().toString();
        String postBody = contentText.getEditText().getText().toString();
        if ( !postTitle.equals("") && !postBody.equals("") ) {
            // TODO: change to actual user when that part is ready
            String userName = "taralb6@gmail.com";
            try {
                newPost.put("postTitle", postTitle);
                newPost.put("postBody", postBody);
                newPost.put("userName", userName);
            } catch (Exception e) {
                System.out.println("Exception occurred when adding elements to json object: " + e);
            }
            try {
                String stringPost = newPost.toString();
                sendPost("/api/posts/createPost", stringPost);
                successDialog.show();
            } catch (Exception e) {
                e.printStackTrace();
                failureDialog.show();
            }
        } else if ( postTitle.equals("") ) {
            int red = Color.parseColor("#FF0000");
            titleText.setBoxStrokeColor(red);
            titleText.setHint("Title required!!");
            titleText.setHintTextColor(ColorStateList.valueOf(red));
        } else {
            int red = Color.parseColor("#FF0000");
            contentText.setBoxStrokeColor(red);
            contentText.setHint("Post content required!!");
            contentText.setHintTextColor(ColorStateList.valueOf(red));
        }
    }//end createPost

    private void sendPost(String apiUrl, String postData) throws Exception {
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addBody(postData.getBytes())
                .addHeader("Content-Type", "application/json")
                .build();
        System.out.println("POST DATA IS HERE: " + postData);
        Amplify.API.post(options,
                response -> Log.i("API", "POST response: " + response.getData().asString()),
                error -> Log.e("API", "POST request failed", error)
                );
    }

    private boolean discardPost() {
        // Go back to previous page after warning
        boolean hasText = false;
        String postTitle = titleText.getEditText().getText().toString();
        String postBody = contentText.getEditText().getText().toString();
        if ( !postTitle.equals("") || !postBody.equals("") ) {
            System.out.println("Throwing away post...");
            hasText = true;
        }
        return hasText;
    }//end createPost

}

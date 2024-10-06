package com.example.yappmobile;

import android.content.DialogInterface;
import android.os.Bundle;

import java.util.ArrayList;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.CompoundButton;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.EditText;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.app.AlertDialog;

import org.json.JSONObject;

public class CreatePostActivity extends AppCompatActivity {
    private JSONObject post;
    private EditText titleText;
    private EditText contentText;
    @Override
    public void onCreate(Bundle savedInstanceState){
        super.onCreate(savedInstanceState);
        setContentView(R.layout.create_post);

        post = new JSONObject();
        try {
            post.put("UserName", "");
            post.put("PostTitle", "");
            post.put("PostBody", "");
            post.put("DiaryEntry", false);
            post.put("Anonymous", true);
        } catch (Exception e) {
            System.out.println("Exception occurred when adding elements to json object: " + e);
        }
        titleText = findViewById(R.id.post_title);
        contentText = findViewById(R.id.post_content);
    }//end onCreate

    public void createPost(View view) {
        // Make API call when invoked
        String postTitle = titleText.getText().toString();
        String postBody = titleText.getText().toString();
        if ( !postTitle.equals("") && !postBody.equals("") ) {
            // TODO: change to actual user when that part is ready
            String userName = "cs0716934@gmail.com";
            try {
                post.put("PostTitle", postTitle);
                post.put("PostBody", postBody);
                post.put("UserName", userName);
            } catch (Exception e) {
                System.out.println("Exception occurred when adding elements to json object: " + e);
            }
            // Make API call to create the post
            /*fetch("/api/posts/createPost", {
                    method: "POST",
                    body: jsonPost,
                    headers: {
                "Content-type": "application/json; charset=UTF-8"
            });*/

            // Send to "home" page, later on to the post's page itself
            Intent intent = new Intent(CreatePostActivity.this, MainActivity.class);
            startActivity(intent);
        }
    }//end createPost

    public void discardPost(View view) {
        // Go back to previous page after warning
        String postTitle = titleText.getText().toString();
        String postBody = titleText.getText().toString();
        if ( !postTitle.equals("") || !postBody.equals("") ) {
            System.out.println("Throwing away post...");
            AlertDialog alertDialog = new AlertDialog.Builder(this).create();
            alertDialog.setTitle("Discard Changes??");
            alertDialog.setMessage("Are you really sure that you want to discard your changes??");
            alertDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Start", new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int id) {
                    Intent intent = new Intent(CreatePostActivity.this, MainActivity.class);
                    startActivity(intent);
                }
            });
        }
    }//end createPost

}

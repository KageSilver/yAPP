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

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

public class CreatePostActivity extends AppCompatActivity {
    private JSONObject post;
    private TextInputLayout titleText;
    private TextInputLayout contentText;
    private Button discardPost;
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

        discardPost = findViewById(R.id.discard_button);
        AlertDialog alertDialog = new AlertDialog.Builder(this).create();
        alertDialog.setTitle("Discard Changes??");
        alertDialog.setMessage("Are you really sure that you want to discard your changes??");
        alertDialog.setButton(AlertDialog.BUTTON_POSITIVE, "Yes", new DialogInterface.OnClickListener() {
            public void onClick(DialogInterface dialog, int id) {
                Intent intent = new Intent(CreatePostActivity.this, MainActivity.class);
                startActivity(intent);
            }
        });
        alertDialog.setButton(AlertDialog.BUTTON_NEGATIVE, "No, keep editing", new DialogInterface.OnClickListener() {
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
                    alertDialog.show();
                } else {
                    Intent intent = new Intent(CreatePostActivity.this, MainActivity.class);
                    startActivity(intent);
                }
            }
        });
    }//end onCreate

    public void createPost(View view) {
        // Make API call when invoked
        String postTitle = titleText.getEditText().getText().toString();
        String postBody = contentText.getEditText().getText().toString();
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
            try {
                String stringPost = post.toString();
                String response = sendPost("/api/posts/createPost", stringPost);
                System.out.println(response);
            } catch (Exception e) {
                e.printStackTrace();
            }
            // Send to "home" page, later on to the post's page itself
            Intent intent = new Intent(CreatePostActivity.this, MainActivity.class);
            startActivity(intent);
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

    private String sendPost(String apiUrl, String postData) throws Exception {
        // Create the URL
        URL url = new URL(apiUrl);
        HttpURLConnection connection = (HttpURLConnection) url.openConnection();

        // Set the request method to POST
        connection.setRequestMethod("POST");
        connection.setRequestProperty("Content-Type", "application/json; utf-8");
        connection.setRequestProperty("Accept", "application/json");
        connection.setDoOutput(true); // Enable writing output to the connection

        // Send the POST data
        try (OutputStream os = connection.getOutputStream()) {
            byte[] input = postData.getBytes(StandardCharsets.UTF_8);
            os.write(input, 0, input.length);
        }

        // Get the response code
        int responseCode = connection.getResponseCode();
        if (responseCode == HttpURLConnection.HTTP_OK || responseCode == HttpURLConnection.HTTP_CREATED) { // success
            BufferedReader in = new BufferedReader(new InputStreamReader(connection.getInputStream(), StandardCharsets.UTF_8));
            String inputLine;
            StringBuffer response = new StringBuffer();

            while ((inputLine = in.readLine()) != null) {
                response.append(inputLine);
            }
            in.close();

            // Return the response as a string
            return response.toString();
        } else {
            return "POST request failed: " + responseCode;
        }
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

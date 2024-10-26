package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.NaviBarDestinations.PublicPostsActivity;
import com.google.android.material.floatingactionbutton.FloatingActionButton;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.CompletableFuture;

public class PostEntryActivity extends AppCompatActivity
{
    private TextView postTitle, postBody;
    private CardListHelper postListHelper;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_post_entry);

        // Back button code
        FloatingActionButton backButton = findViewById(R.id.back_button);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(PostEntryActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });

        postListHelper = new CardListHelper(this);

        // Setup content view to display post content
        String pid = getIntent().getStringExtra("pid");
        postTitle = findViewById(R.id.post_title);
        postBody = findViewById(R.id.post_body);

        String getPostAPI = "/api/posts/getPostById?pid=" + pid;
        loadPost(getPostAPI);
    }

    private void loadPost(String apiUrl)
    {
        // Fetch post (only singular, should return the same thing based on the url passed
        CompletableFuture<String> future = postListHelper.getItemsFromAPI(apiUrl);
        future.thenAccept(jsonData -> {
            try
            {
                // Handle API response
                Log.d("API", "Received data: " + jsonData);

                // Get the post's title and body from the response JSON
                JSONObject jsonObject = new JSONObject(jsonData);
                String title = jsonObject.get("postTitle").toString();
                String body = jsonObject.get("postBody").toString();

                // Update fields:
                runOnUiThread(() -> {
                    postTitle.setText(title);
                    postBody.setText(body);
                });
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }).exceptionally(throwable -> {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
    }
}
package com.example.yappmobile;

import android.os.Bundle;

import android.content.Intent;
import android.util.Log;
import android.view.View;
import android.widget.Button;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.*;

import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.time.LocalDateTime;

public class PublicPostsActivity extends AppCompatActivity implements ItemListCardInterface
{
    RecyclerView rvPosts;
    private List<JSONObject> postList;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_public_posts);

        // setup recycler view to display quiz list cards
        rvPosts = (RecyclerView) findViewById(R.id.public_posts_list);

        // Post creation button code
        Button newPost = findViewById(R.id.new_post_button);
        newPost.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(PublicPostsActivity.this, CreatePostActivity.class);
                startActivity(intent);
            }
        });

        // My posts button code view
        Button myPosts = findViewById(R.id.my_posts_button);
        myPosts.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(PublicPostsActivity.this, MyPostsActivity.class);
                startActivity(intent);
            }
        });

        // setup recycler view to display post list cards
        rvPosts = (RecyclerView) findViewById(R.id.public_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        // Initially setup the adapter with an empty list that'll then be populated after
        postList = new ArrayList<>();
        PostListAdapter adapter = new PostListAdapter(this, postList, this);
        rvPosts.setAdapter(adapter);

        // Fetch posts
        LocalDateTime since = LocalDateTime.now();
        String maxResults = "10";
        String myPostsAPI = "/api/posts/getRecentPosts?since="+since+"&maxResults="+maxResults;
        CompletableFuture<String> future = MyPostsActivity.getPosts(myPostsAPI);

        future.thenAccept(jsonData ->
        {
            // Handle API response
            Log.d("API", "Received data: " + jsonData);
            postList = MyPostsActivity.handleData(jsonData);
            // Notify the adapter that the list updated:
            runOnUiThread(() ->
            {
                adapter.updatePostList(postList);
                adapter.notifyDataSetChanged();
            });
        }).exceptionally(throwable ->
        {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
    }//end onCreate

    @Override
    public void onItemClick(int position) {
        // setup activity switch when a post list card is pressed
        Intent intent = new Intent(PublicPostsActivity.this, PostEntryActivity.class);
        startActivity(intent);
    }
}
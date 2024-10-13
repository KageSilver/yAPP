package com.example.yappmobile;

import android.os.Bundle;

import android.content.Intent;
import android.view.View;
import android.widget.Button;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.*;

import java.time.LocalDateTime;

public class PublicPostsActivity extends AppCompatActivity implements ItemListCardInterface
{
    private RecyclerView rvPosts;

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

        // Calling function helper class to keep repetition down
        PostListHelper functionHelper = new PostListHelper(this, this);

        // setup recycler view to display post list cards
        rvPosts = (RecyclerView) findViewById(R.id.public_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        LocalDateTime since = LocalDateTime.now();
        String maxResults = "10";
        String publicPostsAPI = "/api/posts/getRecentPosts?since="+since+"&maxResults="+maxResults;

        functionHelper.loadPosts(publicPostsAPI, rvPosts);
    }//end onCreate

    @Override
    public void onItemClick(int position) {
        // setup activity switch when a post list card is pressed
        Intent intent = new Intent(PublicPostsActivity.this, PostEntryActivity.class);
        startActivity(intent);
    }
}
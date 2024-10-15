package com.example.yappmobile;

import android.os.Bundle;

import android.content.Intent;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.*;

import java.time.LocalDateTime;

public class PublicPostsActivity extends AppCompatActivity implements ItemListCardInterface
{
    private RecyclerView rvPosts;
    private ProgressBar loadingSpinner;
    private PostListHelper functionHelper;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_public_posts);

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

        loadingSpinner = (ProgressBar) findViewById(R.id.indeterminateBar);
        // Calling function helper class to keep repetition down
        functionHelper = new PostListHelper(this, this, loadingSpinner);

        LocalDateTime since = LocalDateTime.now();
        String maxResults = "10";
        String publicPostsAPI = "/api/posts/getRecentPosts?since="+since+"&maxResults="+maxResults;

        // Setup recycler view to display post list cards
        rvPosts = (RecyclerView) findViewById(R.id.public_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        functionHelper.loadPosts(publicPostsAPI, rvPosts);
    }//end onCreate

    @Override
    public void onItemClick(int position) {
        // Setup activity switch when a post list card is pressed
        Intent intent = new Intent(PublicPostsActivity.this, PostEntryActivity.class);
        String pid = functionHelper.getPID(position);
        intent.putExtra("pid", pid);
        startActivity(intent);
    }
}
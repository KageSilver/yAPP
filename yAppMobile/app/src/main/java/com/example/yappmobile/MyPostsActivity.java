package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;


public class MyPostsActivity extends AppCompatActivity implements ItemListCardInterface
{
    private RecyclerView rvPosts;
    private ProgressBar loadingSpinner;
    private PostListHelper functionHelper;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_my_posts);

        // Post creation button code
        Button newPost = findViewById(R.id.new_post_button);
        newPost.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(MyPostsActivity.this, CreatePostActivity.class);
                startActivity(intent);
            }
        });

        // Global posts button code view
        Button globalPosts = findViewById(R.id.global_posts_button);
        globalPosts.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(MyPostsActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });

        loadingSpinner = (ProgressBar) findViewById(R.id.indeterminateBar);
        // Calling function helper class to keep repetition down
        functionHelper = new PostListHelper(this, this, loadingSpinner);

        // TODO: change to actual user when that part is ready
        String userName = "taralb6@gmail.com";
        boolean diaryEntry = false;
        String myPostsAPI = "/api/posts/getPostsByUser?userName="+userName+"&diaryEntry="+diaryEntry;

        // Setup recycler view to display post list cards
        rvPosts = (RecyclerView) findViewById(R.id.my_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        functionHelper.loadPosts(myPostsAPI, rvPosts);

    }//end onCreate

    @Override
    public void onItemClick(int position)
    {
        // Setup activity switch when a post list card is pressed
        Intent intent = new Intent(MyPostsActivity.this, PostEntryActivity.class);
        String pid = functionHelper.getPID(position);
        intent.putExtra("pid", pid);
        startActivity(intent);
    }
}

package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.google.android.material.navigation.NavigationBarView;

import java.time.LocalDateTime;

public class PublicPostsActivity extends AppCompatActivity implements ItemListCardInterface
{
    private PostListHelper functionHelper;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_public_posts);

        ProgressBar loadingSpinner = findViewById(R.id.indeterminateBar);
        functionHelper = new PostListHelper(this, this, loadingSpinner);

        // "Load more posts" button code
        String since = LocalDateTime.now().toString();
        Button loadMore = findViewById(R.id.load_more_button);
        loadMore.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                refreshPosts(functionHelper.getLastPostTime());
            }
        });
        refreshPosts(since);

        // References:
        // https://stackoverflow.com/a/75360800
        // https://github.com/material-components/material-components-android/blob/master/docs/components/BottomNavigation.md
        // TODO: make public posts and calendar into fragments instead
        NavigationBarView bottomNav = findViewById(R.id.bottom_navigation);
        bottomNav.setOnItemSelectedListener(new NavigationBarView.OnItemSelectedListener()
        {
            @Override
            public boolean onNavigationItemSelected(@NonNull MenuItem item)
            {
                final int PROFILE = R.id.profile;
                final int HOME = R.id.public_posts;
                final int CREATE_POST = R.id.create_post;

                int itemId = item.getItemId();

                Intent intent;

                if(itemId == PROFILE)
                {
                    // Switch to profile page, which will have no nav bar
                    intent = new Intent(PublicPostsActivity.this,
                            ProfileActivity.class);
                    startActivity(intent);
                }
                else if (itemId == HOME)
                {
                    // Swap content for public posts
                    intent = new Intent(PublicPostsActivity.this,
                            PublicPostsActivity.class);
                }
                else if (itemId == CREATE_POST)
                {
                    // Swap content for diary entries
                    intent = new Intent(PublicPostsActivity.this,
                            CreatePostActivity.class);
                }
                else
                {
                    Log.i("Rerouting", "OOF! We haven't done this yet");
                }
                return true;
            }
        });
    }

    private void refreshPosts(String since)
    {
        int maxResults = 10;
        String publicPostsAPI = "/api/posts/getRecentPosts?since="+since+"&maxResults="+ maxResults;

        // Setup recycler view to display post list cards
        RecyclerView rvPosts = findViewById(R.id.public_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        functionHelper.loadPosts(publicPostsAPI, rvPosts);
    }

    @Override
    public void onItemClick(int position)
    {
        // Setup activity switch when a post list card is pressed
        Intent intent = new Intent(PublicPostsActivity.this, PostEntryActivity.class);
        String pid = functionHelper.getPID(position);
        intent.putExtra("pid", pid);
        startActivity(intent);
    }
}
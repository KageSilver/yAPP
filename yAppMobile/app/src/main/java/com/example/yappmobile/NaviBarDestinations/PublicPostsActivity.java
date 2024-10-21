package com.example.yappmobile.NaviBarDestinations;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.NavBar;
import com.example.yappmobile.PostEntryActivity;
import com.example.yappmobile.R;

import java.time.LocalDateTime;

public class PublicPostsActivity extends AppCompatActivity implements IListCardItemInteractions
{
    private CardListHelper postListHelper;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_public_posts);

        NavBar.establishNavBar(this, "HOME");

        ProgressBar loadingSpinner = findViewById(R.id.indeterminateBar);
        postListHelper = new CardListHelper(this, loadingSpinner, "POST", this);

        // "Load more posts" button code
        String since = LocalDateTime.now().toString();
        Button loadMore = findViewById(R.id.load_more_button);
        loadMore.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                refreshPosts(postListHelper.getLastPostTime());
            }
        });
        refreshPosts(since);

        ImageButton logOutButton = findViewById(R.id.log_out_button);
        logOutButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Amplify.Auth.signOut(
                        result ->
                        {
                            Log.i("Auth", "Signing out user...");
                            Intent intent = new Intent(PublicPostsActivity.this, AuthenticatorActivity.class);
                            startActivity(intent);
                        }
                );
            }
        });
    }

    private void refreshPosts(String since)
    {
        // Created formatted API path
        final int MAX_RESULTS = 10;
        String apiPath = "/api/posts/getRecentPosts?since=%s&maxResults=%d";
        String formattedPath = String.format(apiPath, since, MAX_RESULTS);

        // Setup recycler view to display post cards
        RecyclerView rvPosts = findViewById(R.id.public_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        postListHelper.loadItems(formattedPath, rvPosts);
    }

    @Override
    public void onItemClick(int position)
    {
        // Setup activity switch when a post list card is pressed
        Intent intent = new Intent(PublicPostsActivity.this, PostEntryActivity.class);
        String pid = postListHelper.getPID(position);
        intent.putExtra("pid", pid);
        startActivity(intent);
    }
}
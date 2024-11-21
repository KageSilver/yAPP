package com.example.yappmobile.NaviBarDestinations;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ProgressBar;

import androidx.activity.result.ActivityResult;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.NavBar;
import com.example.yappmobile.Posts.PostEntryActivity;
import com.example.yappmobile.R;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.TimeZone;
import java.util.ArrayList;

public class PublicPostsActivity extends AppCompatActivity implements IListCardItemInteractions
{
    private CardListHelper postListHelper;
    private ArrayList<String> dates;
    private int currentDatePosition;
    private Button goBack;
    private Button loadMore;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_public_posts);
        NavBar.establishNavBar(this, "HOME");

        ProgressBar loadingSpinner = findViewById(R.id.indeterminateBar);
        postListHelper = new CardListHelper(this, loadingSpinner, "POST", this);

        // Load more posts button code
        // Formats current time in GMT
        // This needs to be formatted in GMT or else the newest posts won't load on the home page
        SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        format.setTimeZone(TimeZone.getTimeZone("GMT"));
        Date date = new Date();
        String since = format.format(date);

        currentDatePosition = 0;
        dates = new ArrayList<String>();
        dates.add(since);

        loadMore = findViewById(R.id.load_more_button);
        loadMore.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                pageForwards();
            }
        });
        refreshPosts(since);

        goBack = findViewById(R.id.go_back_button);
        goBack.setVisibility(View.INVISIBLE);
        goBack.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                pageBackwards();
            }
        });

        // Set up logout button code
        ImageButton logOutButton = findViewById(R.id.log_out_button);
        logOutButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Amplify.Auth.signOut(result -> {
                    Log.i("Auth", "Signing out user...");
                    Intent intent = new Intent(PublicPostsActivity.this, AuthenticatorActivity.class);
                    startActivity(intent);
                });
            }
        });
    }

    private void pageForwards() {
        int listSize = postListHelper.getCardListSize();
        if ( listSize == 10 )
        {
            goBack.setVisibility(View.VISIBLE);
            String since = postListHelper.getLastPostTime();
            currentDatePosition++;
            dates.add(since);
            refreshPosts(since);
        }
        else
        {
            loadMore.setVisibility(View.INVISIBLE);
        }
    }

    private void pageBackwards() {
        if ( currentDatePosition != 0 )
        {
            loadMore.setVisibility(View.VISIBLE);
            currentDatePosition--;
            String since = dates.get(currentDatePosition);
            refreshPosts(since);
        }
        else
        {
            goBack.setVisibility(View.INVISIBLE);
        }
        // This needs to be here to get rid of the button upon going back to the first page
        if ( currentDatePosition == 0 )
            goBack.setVisibility(View.INVISIBLE);
    }

    private void refreshPosts(String since)
    {
        // Created formatted API path
        final int MAX_RESULTS = 10;
        String apiPath = "/api/posts/getRecentPosts?since=" + since + "&maxResults=" + MAX_RESULTS;

        // Setup recycler view to display post cards
        RecyclerView rvPosts = findViewById(R.id.public_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        postListHelper.loadItems(apiPath, rvPosts);
    }

    @Override
    public void onItemClick(int position)
    {
        // Switch activity to view an individual post entry when a card is clicked
        Intent intent = new Intent(PublicPostsActivity.this, PostEntryActivity.class);
        // passing the whole object so we don't need to fetch
        String postObject = postListHelper.getItem(position).toString();
        intent.putExtra("currentPost",postObject);
        activityLauncher.launch(intent);
    }

    @Override
    public void onRestart()
    {
        super.onRestart();
        finish();
        startActivity(getIntent());
    }

    ActivityResultLauncher<Intent> activityLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            new ActivityResultCallback<ActivityResult>() {
                @Override
                public void onActivityResult(ActivityResult result){
                    if (result.getResultCode() == RESULT_OK)
                    {
                        Intent intent = result.getData();
                        try
                        {
                            String deleted = intent.getStringExtra("delete");
                            postListHelper.removePost(deleted);
                        }
                        catch (Exception e)
                        {
                            Log.i("POST", "Post was not deleted");
                        }
                    }
                }
        });
}
package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;

public class MyPostsActivity extends AppCompatActivity implements ItemListCardInterface
{
    private RecyclerView rvPosts;
    private List<JSONObject> postList;

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

        // setup recycler view to display post list cards
        rvPosts = (RecyclerView) findViewById(R.id.my_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        // Initially setup the adapter with an empty list that'll then be populated after
        postList = new ArrayList<>();
        PostListAdapter adapter = new PostListAdapter(this, postList, this);
        rvPosts.setAdapter(adapter);

        // Fetch posts
        // TODO: change to actual user when that part is ready
        String userName = "taralb6@gmail.com";
        boolean diaryEntry = false;
        String myPostsAPI = "/api/posts/getPostsByUser?userName="+userName+"&diaryEntry="+diaryEntry;
        CompletableFuture<String> future = getPosts(myPostsAPI);

        future.thenAccept(jsonData ->
        {
            // Handle API response
            Log.d("API", "Received data: " + jsonData);
            postList = handleData(jsonData);
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

    public static CompletableFuture<String> getPosts(String apiUrl)
    {
        CompletableFuture<String> future = new CompletableFuture<>();
        // Invoke an API call and return a list of JSON objects containing the posts
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addHeader("Content-Type", "application/json")
                .build();
        retryAPICall(options, future, 5);
        return future;
    }

    private static void retryAPICall(RestOptions options, CompletableFuture<String> future, int retriesLeft)
    {
        Amplify.API.get(options,
            response ->
            {
                Log.i("API", "GET response: " + response.getData().asString());
                future.complete(response.getData().asString());
            },
            error ->
            {
                if (retriesLeft > 0 && error.getCause() instanceof java.net.SocketTimeoutException)
                {
                    Log.i("API", "Retrying... Attempts left: " + retriesLeft);
                    retryAPICall(options, future, retriesLeft - 1); // Retry the request
                }
                else
                {
                    Log.e("API", "GET request failed", error);
                    future.completeExceptionally(error);
                }
            });
    }

    public static List<JSONObject> handleData( String jsonData )
    {
        // Convert the data returned by the function
        List<JSONObject> parsedPosts = new ArrayList<>();
        try {
            JSONArray jsonArray = new JSONArray(jsonData);
            for ( int i=0; i<jsonArray.length(); i++ )
            {
                parsedPosts.add(jsonArray.getJSONObject(i));
            }
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }
        return parsedPosts;
    }

    @Override
    public void onItemClick(int position)
    {
        // setup activity switch when a post list card is pressed
        Intent intent = new Intent(MyPostsActivity.this, PostEntryActivity.class);
        startActivity(intent);
    }
}

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

import org.json.JSONObject;
import java.util.List;
import java.util.concurrent.CompletableFuture;

public class MyPostsActivity extends AppCompatActivity implements ItemListCardInterface {
    private RecyclerView rvPosts;
    private List<JSONObject> postList;

    @Override
    public void onCreate(Bundle savedInstanceState){
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
        CompletableFuture<String> future = getMyPosts();

        future.thenAccept(jsonData -> {
            // Handle API response
            Log.d("API", "Received data: " + jsonData);
            postList = handleData(jsonData);
        }).exceptionally(throwable -> {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });

        PostListAdapter adapter = new PostListAdapter(this, postList, this);
        rvPosts.setAdapter(adapter);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));
    }//end onCreate

    private CompletableFuture<String> getMyPosts() {
        CompletableFuture<String> future = new CompletableFuture<>();
        // Invoke an API call and return a list of JSON objects containing the posts
        // TODO: change to actual user when that part is ready
        String userName = "taralb6@gmail.com";
        boolean diaryEntry = false;
        String apiUrl = "/api/posts/getPostsByUser?userName="+userName+"&diaryEntry="+diaryEntry;
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addHeader("Content-Type", "application/json")
                .build();
        Amplify.API.get(options,
                response -> {
                    Log.i("API", "GET response: " + response.getData().asString());
                    future.complete(response.getData().asString());
                },
                error -> {
                    Log.e("API", "GET request failed", error);
                    future.completeExceptionally(error);
                }
        );
        return future;
    }

    private List<JSONObject> handleData( String jsonData ) {
        // Convert the data returned by the function
        System.out.println("JSON DATA" + jsonData);
        return null;
    }

    @Override
    public void onItemClick(int position)
    {
        // setup activity switch when a post list card is pressed
        Intent intent = new Intent(MyPostsActivity.this, PostEntryActivity.class);
        startActivity(intent);
    }
}

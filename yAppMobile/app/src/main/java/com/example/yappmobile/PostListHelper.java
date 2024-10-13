package com.example.yappmobile;

import android.content.Context;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.atomic.AtomicReference;


public class PostListHelper extends AppCompatActivity
{
    private final Context context;
    private final ItemListCardInterface postListCardInterface;

    public PostListHelper(Context context, ItemListCardInterface itemListCardInterface)
    {
        this.context = context;
        this.postListCardInterface = itemListCardInterface;
    }

    public void loadPosts(String apiUrl, RecyclerView rvPosts)
    {
        // Initially setup the adapter with an empty list that'll then be populated after
        AtomicReference<List<JSONObject>> postList = new AtomicReference<>(new ArrayList<>());
        PostListAdapter adapter = new PostListAdapter(context, postList.get(), postListCardInterface);
        rvPosts.setAdapter(adapter);

        // Fetch posts
        CompletableFuture<String> future = getPosts(apiUrl);

        future.thenAccept(jsonData ->
        {
            // Handle API response
            Log.d("API", "Received data: " + jsonData);
            postList.set(handleData(jsonData));
            // Notify the adapter that the list updated:
            runOnUiThread(() ->
            {
                // Hide loading spinner
                adapter.updatePostList(postList.get());
                adapter.notifyDataSetChanged();
            });
        }).exceptionally(throwable ->
        {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
    }

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

}

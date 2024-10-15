package com.example.yappmobile;

import android.content.Context;
import android.util.Log;
import android.view.View;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;


public class PostListHelper extends AppCompatActivity
{
    private final Context context;
    private final ItemListCardInterface postListCardInterface;
    private final ProgressBar loadingSpinner;
    private List<JSONObject> postList;

    public PostListHelper(Context context)
    {
        this.context = context;
        this.postListCardInterface = null;
        this.loadingSpinner = null;
    }

    public PostListHelper(Context context, ItemListCardInterface itemListCardInterface, ProgressBar loadingSpinner)
    {
        this.context = context;
        this.postListCardInterface = itemListCardInterface;
        this.loadingSpinner = loadingSpinner;
    }

    public void loadPosts(String apiUrl, RecyclerView rvPosts)
    {
        loadingSpinner.setVisibility(View.VISIBLE);
        // Initially setup the adapter with an empty list that'll then be populated after
        postList = new ArrayList<>();
        PostListAdapter adapter = new PostListAdapter(context, postList, postListCardInterface);
        rvPosts.setAdapter(adapter);

        // Fetch posts
        CompletableFuture<String> future = getPosts(apiUrl);

        future.thenAccept(jsonData ->
        {
            // Handle API response
            Log.d("API", "Received data: " + jsonData);
            postList = handleData(jsonData);
            // Notify the adapter that the list updated:
            runOnUiThread(() ->
            {
                // Hide loading spinner
                loadingSpinner.setVisibility(View.GONE);
                adapter.updatePostList(postList);
                adapter.notifyDataSetChanged();
            });
        }).exceptionally(throwable ->
        {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
    }

    public CompletableFuture<String> getPosts(String apiUrl)
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

    private void retryAPICall(RestOptions options, CompletableFuture<String> future, int retriesLeft)
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

    public List<JSONObject> handleData( String jsonData )
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

    public String getPID( int position )
    {
        String pid = null;
        try
        {
            pid = postList.get(position).get("pid").toString();
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }
        return pid;
    }

    public String getLastPostTime()
    {
        String since = null;
        try
        {
            since = postList.get(postList.size()-1).get("createdAt").toString();
            since = since.substring(0, since.length()-6);
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }

        return since;
    }

}

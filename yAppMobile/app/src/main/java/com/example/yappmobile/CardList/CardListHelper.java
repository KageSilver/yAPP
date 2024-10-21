package com.example.yappmobile.CardList;

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
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;

public class CardListHelper extends AppCompatActivity
{
    private final Context context; // Where CardItems are Contained
    private final ProgressBar loadingSpinner;
    private final String cardType;
    private List<JSONObject> cardItemList; // List of CardItems
    private final IListCardItemInteractions itemInteractions;

    public CardListHelper(Context context)
    {
        this.context = context;
        this.loadingSpinner = null;
        this.cardType = null;
        this.cardItemList = new ArrayList<>();
        this.itemInteractions = null;
    }

    public CardListHelper(Context context, ProgressBar loadingSpinner,
                          String cardType, IListCardItemInteractions itemInteractions)
    {
        this.context = context;
        this.loadingSpinner = loadingSpinner;
        this.cardType = cardType;
        this.cardItemList = new ArrayList<>();
        this.itemInteractions = itemInteractions;
    }

    public void loadItems(String apiUrl, RecyclerView recyclerView)
    {
        // Make loading spinner visible while we populate our CardItemAdapter
        loadingSpinner.setVisibility(View.VISIBLE);

        // Setup the adapter with an empty list that will be updated later
        CardListAdapter adapter = new CardListAdapter(context, cardItemList, cardType, itemInteractions);
        recyclerView.setAdapter(adapter);

        // Fetch card items from API
        CompletableFuture<String> future = getItemsFromAPI(apiUrl);

        future.thenAccept(jsonData ->
        {
            // Convert API response into a list of CardItems
            cardItemList = handleData(jsonData);

            // Once response is received and parsed successfully,
            // Hide loading spinner and update UI display
            runOnUiThread(() ->
            {
                loadingSpinner.setVisibility(View.GONE);
                adapter.updateList(cardItemList);
                adapter.notifyDataSetChanged();
            });
        }).exceptionally(throwable ->
        {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
    }

    public CompletableFuture<String> getItemsFromAPI(String apiUrl)
    {
        // Attempt to connect to API endpoint `MAX_RETRIES` times
        final int MAX_RETRIES = 5;
        CompletableFuture<String> future = new CompletableFuture<>();
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addHeader("Content-Type", "application/json")
                .build();
        retryAPICall(options, future, MAX_RETRIES);
        return future;
    }

    private void retryAPICall(RestOptions options,
                              CompletableFuture<String> future, int retriesLeft)
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
                        retryAPICall(options, future, retriesLeft - 1);
                    }
                    else
                    {
                        Log.e("API", "GET request failed", error);
                        future.completeExceptionally(error);
                    }
                });
    }

    // Convert the API response into a list of JSON objects
    private List<JSONObject> handleData(String jsonData)
    {
        List<JSONObject> parsedItems = new ArrayList<>();
        try
        {
            JSONArray jsonArray = new JSONArray(jsonData);
            for (int i = 0; i < jsonArray.length(); i++)
            {
                parsedItems.add(jsonArray.getJSONObject(i));
            }
        }
        catch (JSONException e)
        {
            Log.e("JSON", "Error parsing JSON", e);
        }
        return parsedItems;
    }

    public String getPID(int position)
    {
        String pid = null;
        try
        {
            if(cardType.equals("POST"))
            {
                pid = cardItemList.get(position).get("pid").toString();
            }
            else
            {
                Log.e("JSON", "Wrong card type! Tried to get PID on a non-PostCard item");
            }
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
        StringBuilder builder = new StringBuilder();
        try
        {
            if(cardType.equals("POST"))
            {
                since = cardItemList.get(cardItemList.size()-1).get("createdAt").toString();
                builder.append(since.substring(0, since.length()-6));
                builder.append("-");
                builder.append(since.substring(since.length()-5));
                since = builder.toString();
            }
            else
            {
                Log.e("JSON", "Wrong card type! Tried to get the last post's time on a non-PostCard item");
            }
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }
        return since;
    }
}

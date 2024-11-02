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

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Collections;
import java.util.Comparator;
import java.util.concurrent.CompletableFuture;
import java.util.Date;

public class CardListHelper extends AppCompatActivity
{
    private final Context context; // Where CardItems are Contained
    private final ProgressBar loadingSpinner;
    private final String cardType; // Currently 4 types: POST, DIARY, CURRENT_FRIEND, AND FRIEND_REQUEST
    private List<JSONObject> cardItemList; // List of CardItems
    private final IListCardItemInteractions itemInteractions; // Handles clicks of the CardItem
    private CardListAdapter adapter;

    private boolean myPosts;

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

    public CardListHelper(Context context, ProgressBar loadingSpinner,
                          String cardType, IListCardItemInteractions itemInteractions,
                          boolean myPosts)
    {
        this.context = context;
        this.loadingSpinner = loadingSpinner;
        this.cardType = cardType;
        this.cardItemList = new ArrayList<>();
        this.itemInteractions = itemInteractions;
        this.myPosts = myPosts;
    }

    public void loadItems(String apiUrl, RecyclerView recyclerView)
    {
        // Make loading spinner visible while we populate our CardItemAdapter
        loadingSpinner.setVisibility(View.VISIBLE);

        createAdapter(recyclerView);

        // Fetch card items from API
        CompletableFuture<String> future = getItemsFromAPI(apiUrl);
        future.thenAccept(jsonData ->
        {
            // Convert API response into a list of CardItems
            cardItemList = handleData(jsonData);

            if (myPosts)
            {
                sortPosts();
            }
            populateCard();
        }).exceptionally(throwable ->
        {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
    }

    private void sortPosts()
    {
        SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");

        // Sort the list by createdAt in descending order
        Collections.sort(cardItemList, new Comparator<JSONObject>()
        {
            @Override
            public int compare(JSONObject post1, JSONObject post2)
            {
                try
                {
                    // createdAt needs to have the milliseconds truncated for this format
                    String createdAt1 = truncateToMS(post1.getString("createdAt"));
                    String createdAt2 = truncateToMS(post2.getString("createdAt"));

                    Date date1 = dateFormat.parse(createdAt1);
                    Date date2 = dateFormat.parse(createdAt2);
                    return date2.compareTo(date1);
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                    return 0; // Fail application if this broke
                }
            }
        });
    }

    private String truncateToMS(String date)
    {
        if (date.length() > 20)
        {
            // Replace the +Z
            return date.substring(0, 20);
        }
        else
        {
            return date;
        }
    }

    public void loadDiaries(String apiUrlUser, String apiUrlFriends, RecyclerView recyclerView, List<JSONObject> friends, String uid)
    {
        // Make loading spinner visible while we populate our CardItemAdapter
        loadingSpinner.setVisibility(View.VISIBLE);

        createAdapter(recyclerView);

        // Fetch card items from API
        CompletableFuture<String> future = getItemsFromAPI(apiUrlUser);
        future.thenAccept(jsonData ->
        {
            // Convert API response into a list of CardItems
            // add user diaries at the start of the list to appear at the top of the recycler view
            cardItemList.addAll(0, handleData(jsonData));
            // get usernames to appear on posts
            getUsernamesForDiaries(friends, uid);
            populateCard();
            runOnUiThread(() -> {
                // Make loading spinner visible while we populate our CardItemAdapter
                loadingSpinner.setVisibility(View.VISIBLE);
            });
        }).exceptionally(throwable ->
        {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });

        future = getItemsFromAPI(apiUrlFriends);
        future.thenAccept(jsonData ->
        {
            // Convert API response into a list of CardItems
            cardItemList.addAll(handleData(jsonData));
            // get usernames to appear on posts
            getUsernamesForDiaries(friends, uid);
            populateCard();
        }).exceptionally(throwable ->
        {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
    }

    private void getUsernamesForDiaries(List<JSONObject> friends, String uid)
    {
        for(int i = 0; i < cardItemList.size(); i++)
        {
            try
            {
                if(cardItemList.get(i).getString("uid").equals(uid))
                {
                    // display username for diaries from current user as "You"
                    cardItemList.get(i).put("username", "You");
                }
                else if(cardItemList.get(i).getBoolean("anonymous"))
                {
                    // displays username for anonymous diaries as "Anonymous"
                    cardItemList.get(i).put("username", "Anonymous");
                }
                else
                {
                    // find friend's username based on their uid to display
                    String friendUid = cardItemList.get(i).getString("uid");

                    for(int j = 0; j < friends.size(); j++)
                    {
                        if(friendUid.equals(friends.get(j).getString("uid")))
                        {
                            cardItemList.get(i).put("username", friends.get(j).getString("userName"));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.e("JSON", "Error parsing JSON", e);
            }
        }
    }

    public void clearItems()
    {
        // clears items in the recycler view for cleaner transitions between calendar dates
        cardItemList = new ArrayList<>();
        adapter.updateList(cardItemList);
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

    private void createAdapter(RecyclerView recyclerView)
    {
        runOnUiThread(() ->
        {
            // Setup the adapter with an empty list that will be updated later
            adapter = new CardListAdapter(context, cardItemList, cardType, itemInteractions);
            recyclerView.setAdapter(adapter);
        });
    }

    private void populateCard()
    {
        // Once response is received and parsed successfully,
        // Hide loading spinner and update UI display
        // NOTE: The adapter is what populates each card!
        runOnUiThread(() ->
        {
            loadingSpinner.setVisibility(View.GONE);
            adapter.updateList(cardItemList);
            adapter.notifyDataSetChanged();
        });
    }

    // Retries `retriesLeft` times and captures the response of the API call
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
                    if (retriesLeft > 0
                        && error.getCause() instanceof java.net.SocketTimeoutException)
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
    public List<JSONObject> handleData(String jsonData)
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
        return getPostKey(position, "pid");
    }

    public  String getUID(int position){
        return  getPostKey(position, "uid");
    }

    public void removePost(String pid)
    {
        try {
            boolean found = false;
            for(int i = 0; i < cardItemList.size() && !found; i++)
            {
                System.out.println(cardItemList.get(i).getString("pid") + " vs. " + pid);
                if(cardItemList.get(i).getString("pid").equals(pid))
                {
                    found = true;
                    cardItemList.remove(i);
                }
            }

            runOnUiThread(() ->
            {
                adapter.updateList(cardItemList);
                adapter.notifyDataSetChanged();
            });
        }
        catch (Exception e)
        {
            Log.e("JSON", "Error parsing JSON", e);
        }
    }

    public JSONObject getItem(int position){
        return cardItemList.get(position);
    }

    private String getPostKey(int position,String key){
        String value = null;
        try
        {
            if(cardType.equals("POST") || cardType.equals("DIARY"))
            {
                value = cardItemList.get(position).get(key).toString();
            }
            else
            {
                Log.d("CardListHelper",
                        "You're trying to invoke a method on the wrong card type");
            }
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }
        return value;

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
                Log.d("CardListHelper",
                      "Wrong card type! Tried to get the last post's time on a non-PostCard item");
            }
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }
        return since;
    }

    public int getCardListSize()
    {
        return cardItemList.size();
    }

    public JSONObject getFriendship(int position)
    {
        String sender = null;
        String receiver = null;
        try
        {
            if(cardType.equals("CURRENT_FRIEND") || cardType.equals("FRIEND_REQUEST"))
            {
                sender = cardItemList.get(position).get("FromUserName").toString();
                receiver = cardItemList.get(position).get("ToUserName").toString();
            }
            else
            {
                Log.d("CardListHelper", "You're trying to invoke a method on the wrong card type");
            }
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }

        // Create resulting object
        JSONObject result = new JSONObject();
        try
        {
            result.put("sender", sender);
            result.put("receiver", receiver);
        }
        catch(JSONException error)
        {
            Log.e("JSON", "Error creating a JSONObject", error);
        }
        return result;
    }
}

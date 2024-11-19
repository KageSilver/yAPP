package com.example.yappmobile.ProfileManagement;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.api.rest.RestResponse;
import com.amplifyframework.auth.AuthUser;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListRequestCardInteractions;
import com.example.yappmobile.NaviBarDestinations.ProfileActivity;
import com.example.yappmobile.R;
import com.google.android.material.floatingactionbutton.ExtendedFloatingActionButton;
import com.google.android.material.floatingactionbutton.FloatingActionButton;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.CompletableFuture;

public class MyRequestsActivity extends AppCompatActivity implements IListRequestCardInteractions
{
    private CardListHelper requestListHelper;
    private RecyclerView rvRequests;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_my_requests);

        ProgressBar loadingSpinner = findViewById(R.id.indeterminate_bar);
        requestListHelper = new CardListHelper(this, loadingSpinner,
                                               "FRIEND_REQUEST", this);

        // Setup recycler view to display request cards
        rvRequests = findViewById(R.id.request_list);
        rvRequests.setLayoutManager(new LinearLayoutManager(this));
        reloadRequestList();

        FloatingActionButton backButton = findViewById(R.id.back_button);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(MyRequestsActivity.this, ProfileActivity.class);
                startActivity(intent);
            }
        });

        ExtendedFloatingActionButton addFriendButton = findViewById(R.id.add_friend_button);
        addFriendButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(MyRequestsActivity.this, AddFriendActivity.class);
                startActivity(intent);
            }
        });
    }

    @Override
    public void onItemClick(int position)
    {
        // Do nothing hehe
    }

    @Override
    public void onAcceptClick(int position)
    {
        acceptRequest(position);
    }

    @Override
    public void onDeclineClick(int position)
    {
        declineRequest(position);
    }

    private void acceptRequest(int position)
    {
        // Based on the position, recreate the friendship as a JSONObject
        JSONObject result = requestListHelper.getFriendship(position);
        try
        {
            // NOTE: Backend only allows for one instance of the same relationship AB and BA
            String personA = result.get("sender").toString();
            String personB = result.get("receiver").toString();
            acceptRequest(personA, personB);
        }
        catch (JSONException error)
        {
            Log.e("JSON", "Getting from a JSONObject", error);
        }
    }

    private void acceptRequest(String personA, String personB)
    {
        JSONObject newFriendship = new JSONObject();
        try
        {
            newFriendship.put("fromUserName", personA);
            newFriendship.put("toUserName", personB);
            newFriendship.put("status", 1);
        }
        catch (JSONException error)
        {
            Log.e("JSON", "Error creating a JSONObject", error);
        }

        // Send API put request to delete friendship
        String apiUrl = "/api/friends/updateFriendRequest";
        sendPutRequest(apiUrl, newFriendship.toString());
    }

    private void declineRequest(int position)
    {
        // Based on the position, recreate the friendship as a JSONObject
        JSONObject result = requestListHelper.getFriendship(position);
        try
        {
            // NOTE: Backend only allows for one instance of the same relationship AB and BA
            String personA = result.get("sender").toString();
            String personB = result.get("receiver").toString();
            declineRequest(personA, personB);
        }
        catch (JSONException error)
        {
            Log.e("JSON", "Getting from a JSONObject", error);
        }
    }

    private void declineRequest(String personA, String personB)
    {
        // Create DELETE request's body
        JSONObject friendship = new JSONObject();
        try
        {
            friendship.put("fromUserName", personA);
            friendship.put("toUserName", personB);
        }
        catch (JSONException error)
        {
            Log.e("JSON", "Error creating a JSONObject", error);
        }

        // Send API put request to delete friendship
        String apiUrl = "/api/friends/deleteFriendship";
        sendDeleteRequest(apiUrl, friendship.toString());
    }

    private void sendPutRequest(String apiUrl, String putBody)
    {
        CompletableFuture<RestResponse> future = new CompletableFuture<>();

        RestOptions options = RestOptions.builder()
                                         .addPath(apiUrl)
                                         .addHeader("Content-Type", "application/json")
                                         .addBody(putBody.getBytes())
                                         .build();
        Amplify.API.put(options,
                        future::complete,
                        error -> Log.e("API", "PUT request failed", error));

        // Then update friend list
        future.thenAccept(restResponse -> {
            runOnUiThread(() -> {
                Log.i("API", restResponse.toString());
                reloadRequestList();
            });
        });
    }

    private void sendDeleteRequest(String apiUrl, String deleteBody)
    {
        CompletableFuture<RestResponse> future = new CompletableFuture<>();

        RestOptions options = RestOptions.builder()
                                         .addPath(apiUrl)
                                         .addHeader("Content-Type", "application/json")
                                         .addBody(deleteBody.getBytes())
                                         .build();
        Amplify.API.put(options,
                        future::complete,
                        error -> Log.e("API", "DELETE request failed", error));

        // Then update friend list
        future.thenAccept(restResponse -> {
            runOnUiThread(() -> {
                Log.i("API", restResponse.toString());
                reloadRequestList();
            });
        });
    }

    // Reload RecyclerView of FriendRequests
    private void reloadRequestList()
    {
        CompletableFuture<AuthUser> future = new CompletableFuture<>();

        Amplify.Auth.getCurrentUser(future::complete, error -> {
            Log.e("Auth", "Uh oh! THere's trouble getting the current user", error);
        });

        future.thenAccept(user -> {
            runOnUiThread(() -> {
                String username = user.getUsername();
                String myRequestsAPI = "/api/friends/getFriendsByStatus?userName=" + username + "&status=0";

                rvRequests = (RecyclerView) findViewById(R.id.request_list);
                rvRequests.setLayoutManager(new LinearLayoutManager(this.getBaseContext()));
                requestListHelper.loadItems(myRequestsAPI, rvRequests);
            });
        });
    }
}
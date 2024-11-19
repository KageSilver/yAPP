package com.example.yappmobile.ProfileManagement.ProfileTabs;

import android.content.DialogInterface;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;

import androidx.appcompat.app.AlertDialog;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.api.rest.RestResponse;
import com.amplifyframework.auth.AuthUser;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.R;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.CompletableFuture;

public class MyFriendsFragment extends Fragment implements IListCardItemInteractions
{
    private CardListHelper friendListHelper;
    private RecyclerView rvFriends;
    private AlertDialog confirmUnfollow;
    private int position;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        return inflater.inflate(R.layout.fragment_my_friends, container, false);
    }

    @Override
    public void onViewCreated(View view, Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        ProgressBar loadingSpinner = view.findViewById(R.id.indeterminateBar);
        friendListHelper = new CardListHelper(this.getContext(), loadingSpinner,
                                              "CURRENT_FRIEND", this);

        // Setup recycler view to display post list cards
        rvFriends = (RecyclerView) view.findViewById(R.id.my_friends_list);
        rvFriends.setLayoutManager(new LinearLayoutManager(this.getContext()));
        reloadFriendList();

        // Setup unfollow confirmation dialog
        confirmUnfollow = new AlertDialog.Builder(view.getContext()).create();
        confirmUnfollow.setTitle("Woah there!");
        confirmUnfollow.setMessage("Are you really sure that you want to unfollow them?");
        confirmUnfollow.setButton(AlertDialog.BUTTON_POSITIVE,
                                  "Yes", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                // If confirmed, send an API request to update your friendship
                removeFriendship(position);
            }
        });
        confirmUnfollow.setButton(AlertDialog.BUTTON_NEGATIVE,
                                  "No, I was just playing", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                Log.i("OnClick", "Friendship saved!");
            }
        });
    }

    @Override
    public void onItemClick(int position)
    {
        // Show confirmUnfollow dialogue
        confirmUnfollow.show();
        this.position = position;
    }

    @Override
    public void refreshUI() {

    }

    private void removeFriendship(int position)
    {
        // Based on the position, recreate the friendship as a JSONObject
        JSONObject result = friendListHelper.getFriendship(position);
        try
        {
            // NOTE: Backend only allows for one instance of the same relationship AB and BA
            String personA = result.get("sender").toString();
            String personB = result.get("receiver").toString();
            removeFriendship(personA, personB);
        }
        catch (JSONException error)
        {
            Log.e("JSON", "Getting from a JSONObject", error);
        }
    }

    private void removeFriendship(String personA, String personB)
    {
        JSONObject newFriendship = new JSONObject();
        try
        {
            newFriendship.put("fromUserName", personA);
            newFriendship.put("toUserName", personB);
            newFriendship.put("status", 2);
        }
        catch (JSONException error)
        {
            Log.e("JSON", "Error creating a JSONObject", error);
        }

        // Send API put request to delete friendship
        String apiUrl = "/api/friends/updateFriendRequest";
        sendPutRequest(apiUrl, newFriendship.toString());
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
            getActivity().runOnUiThread(() -> {
                Log.i("API", restResponse.toString());
                reloadFriendList();
            });
        });
    }

    // Reload RecyclerView of FriendCards
    private void reloadFriendList()
    {
        CompletableFuture<AuthUser> future = new CompletableFuture<>();

        Amplify.Auth.getCurrentUser(future::complete, error -> {
            Log.e("Auth", "Uh oh! THere's trouble getting the current user", error);
        });

        future.thenAccept(user -> {
            getActivity().runOnUiThread(() -> {
                String username = user.getUsername();
                String myRequestsAPI = "/api/friends/getFriendsByStatus?userName=" + username + "&status=1";

                rvFriends = (RecyclerView) this.getView().findViewById(R.id.my_friends_list);
                rvFriends.setLayoutManager(new LinearLayoutManager(this.getContext()));
                friendListHelper.loadItems(myRequestsAPI, rvFriends);
            });
        });
    }
}

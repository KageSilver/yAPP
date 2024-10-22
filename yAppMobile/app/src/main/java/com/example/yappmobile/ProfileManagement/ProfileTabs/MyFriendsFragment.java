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
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.R;

import org.json.JSONException;
import org.json.JSONObject;

public class MyFriendsFragment extends Fragment implements IListCardItemInteractions
{
    private CardListHelper friendListHelper;
    private RecyclerView rvFriends;
    private AlertDialog confirmUnfollow;
    private boolean confirmed = false;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        return inflater.inflate(R.layout.fragment_my_friends, container, false);
    }

    @Override
    public void onViewCreated(View view, Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        ProgressBar loadingSpinner = (ProgressBar) view.findViewById(R.id.indeterminateBar);
        friendListHelper = new CardListHelper(this.getContext(),
                loadingSpinner, "CURRENT_FRIEND", this);

        // Setup recycler view to display post list cards
        rvFriends = (RecyclerView) view.findViewById(R.id.my_friends_list);
        rvFriends.setLayoutManager(new LinearLayoutManager(this.getContext()));
        reloadFriendList();

        // Setup unfollow confirmation dialog
        confirmUnfollow = new AlertDialog.Builder(view.getContext()).create();
        confirmUnfollow.setTitle("Woah there!");
        confirmUnfollow.setMessage("Are you really sure that you want to unfollow them?");
        confirmUnfollow.setButton(AlertDialog.BUTTON_POSITIVE, "Yes", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                confirmed = true;
            }
        });
        confirmUnfollow.setButton(AlertDialog.BUTTON_NEGATIVE, "No, I was just playing", new DialogInterface.OnClickListener()
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
        // If confirmed, send an API request to update your friendship
        // And reload the displayed friendship list
        confirmUnfollow.show();
        if(confirmed)
        {
            removeFriendship(position);
            reloadFriendList();
        }
    }

    private void removeFriendship(int position)
    {
        // Based on the position, recreate the friendship as a JSONObject
        JSONObject result = friendListHelper.getFriendship(position);
        JSONObject newFriendship = new JSONObject();
        try
        {
            newFriendship.put("fromUserName", result.get("sender").toString());
            newFriendship.put("toUserName", result.get("receiver").toString());
            newFriendship.put("status", 2);
        }
        catch(JSONException error)
        {
            Log.e("JSON", "Error creating a JSONObject", error);
        }

        // Send API put request to delete friendship
        String apiUrl = "/api/friends/updateFriendRequest";
        sendPutRequest(apiUrl, newFriendship.toString());
    }

    private void sendPutRequest(String apiUrl, String putBody)
    {
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addHeader("Content-Type", "application/json")
                .addBody(putBody.getBytes())
                .build();
        Amplify.API.put(options,
                response -> Log.i("API", "PUT RESPONSE:" + response.getData()),
                error -> Log.e("API", "PUT request failed", error)
        );
    }

    // Reload RecyclerView of FriendCards
    private void reloadFriendList()
    {
        Amplify.Auth.getCurrentUser(
                result ->
                {
                    String userName = result.getUsername();
                    String myRequestsAPI = "/api/friends/getFriendsByStatus?userName="+userName+"&status=1";
                    friendListHelper.loadItems(myRequestsAPI, rvFriends);
                },
                error ->
                {
                    Log.e("Auth", "Uh oh! THere's trouble getting the current user", error);
                }
        );
    }
}

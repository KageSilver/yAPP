package com.example.yappmobile;

import android.os.Bundle;
import android.util.Log;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;

public class MyRequestsActivity extends AppCompatActivity implements IListCardItemInteractions
{
    private CardListHelper requestListHelper;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_my_requests);

        ProgressBar loadingSpinner = (ProgressBar) findViewById(R.id.progressBar);
        requestListHelper = new CardListHelper(this, loadingSpinner, "FRIEND_REQUEST", this);

        // Setup recycler view to display request cards
        RecyclerView rvRequests = (RecyclerView) findViewById(R.id.request_list);
        rvRequests.setLayoutManager(new LinearLayoutManager(this));

        Amplify.Auth.getCurrentUser(
                result ->
                {
                    String userName = result.getUsername();
                    String myRequestsAPI = "/api/friends/getFriendsByStatus?userName="+userName+"&status=0";
                    requestListHelper.loadItems(myRequestsAPI, rvRequests);
                },
                error ->
                {
                    Log.e("Auth", "Uh oh! THere's trouble getting the current user", error);
                }
        );
    }

    @Override
    public void onItemClick(int position)
    {
        // TODO
    }
}

package com.example.yappmobile;

import android.os.Bundle;
import android.util.Log;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.yappmobile.CardList.CardListHelper;

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

        // TODO: change to actual user when that part is ready
        String userName = "taralb6@gmail.com";
        String myRequestsAPI = "api/friends/getFriendsByStatus?userName="+userName+"?status=0";

        // Setup recycler view to display post list cards
        RecyclerView rvPosts = (RecyclerView) findViewById(R.id.request_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this));

        requestListHelper.loadItems(myRequestsAPI, rvPosts);
    }

    @Override
    public void onItemClick(int position)
    {
        Log.d("Click", "You clicked: ");
    }
}

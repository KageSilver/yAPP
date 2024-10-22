package com.example.yappmobile.ProfileManagement;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ProgressBar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.NaviBarDestinations.ProfileActivity;
import com.example.yappmobile.R;
import com.google.android.material.floatingactionbutton.ExtendedFloatingActionButton;
import com.google.android.material.floatingactionbutton.FloatingActionButton;

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
        Log.d("Click", "You clicked: ");
    }
}

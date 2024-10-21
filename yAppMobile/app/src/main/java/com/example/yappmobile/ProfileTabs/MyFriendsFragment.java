package com.example.yappmobile.ProfileTabs;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;

import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.R;

public class MyFriendsFragment extends Fragment implements IListCardItemInteractions
{
    private CardListHelper friendListHelper;

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
        friendListHelper = new CardListHelper(this.getContext(), loadingSpinner, "CURRENT_FRIEND", this);

        // Setup recycler view to display post list cards
        RecyclerView rvFriends = (RecyclerView) view.findViewById(R.id.my_friends_list);
        rvFriends.setLayoutManager(new LinearLayoutManager(this.getContext()));

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

    @Override
    public void onItemClick(int position)
    {
        Log.d("Click", "You clicked something! ");
    }
}

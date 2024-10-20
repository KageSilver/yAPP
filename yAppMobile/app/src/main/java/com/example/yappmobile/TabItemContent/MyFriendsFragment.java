package com.example.yappmobile.TabItemContent;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;

import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.IListCardItemInteractions;
import com.example.yappmobile.R;

public class MyFriendsFragment extends Fragment implements IListCardItemInteractions
{
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
        CardListHelper friendListHelper = new CardListHelper(this.getContext(), loadingSpinner, "CURRENT_FRIEND", this);

        // TODO: change to actual user when that part is ready
        String userName = "taralb6@gmail.com";
        String myRequestsAPI = "api/friends/getFriendsByStatus?userName="+userName+"?status=1";

        // Setup recycler view to display post list cards
        RecyclerView rvFriends = (RecyclerView) view.findViewById(R.id.my_friends_list);
        rvFriends.setLayoutManager(new LinearLayoutManager(this.getContext()));

        friendListHelper.loadItems(myRequestsAPI, rvFriends);
    }

    @Override
    public void onItemClick(int position)
    {
        Log.d("Click", "You clicked something! ");
    }
}

package com.example.yappmobile.ProfileManagement.ProfileTabs;

import android.content.Intent;
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
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.R;

import java.util.concurrent.CompletableFuture;

public class MyAwardsFragment extends Fragment implements IListCardItemInteractions
{
    private CardListHelper awardListHelper;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        return inflater.inflate(R.layout.fragment_my_awards, container, false);
    }

    @Override
    public void onViewCreated(View view, Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        ProgressBar loadingSpinner = view.findViewById(R.id.indeterminateBar);
        awardListHelper = new CardListHelper(this.getContext(), loadingSpinner, "AWARD", this);

        RecyclerView rvAwards = view.findViewById(R.id.my_awards_list);
        rvAwards.setLayoutManager(new LinearLayoutManager(this.getContext()));

        CompletableFuture<String> future = new CompletableFuture<>();
        Amplify.Auth.getCurrentUser(result -> {
            future.complete(result.getUserId());
        }, error -> {
            Log.e("AUTH", "Error when getting current user. Redirecting to authenticator");
            Intent intent = new Intent(view.getContext(), AuthenticatorActivity.class);
            startActivity(intent);
        });

        future.thenAccept(uid -> {
           getActivity().runOnUiThread(() -> {
               awardListHelper.loadAwardsByUser(uid, rvAwards);
           });
        });
    }

    @Override
    public void onItemClick(int position)
    {
        Log.d("This is awkward...", "Interactions with awards are a stretch goal.");
    }
}

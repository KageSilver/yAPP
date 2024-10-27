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
import com.example.yappmobile.PostEntryActivity;
import com.example.yappmobile.R;

import java.util.concurrent.CompletableFuture;

public class MyPostsFragment extends Fragment implements IListCardItemInteractions
{
    private CardListHelper postListHelper;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        return inflater.inflate(R.layout.fragment_my_posts, container, false);
    }

    @Override
    public void onViewCreated(View view, Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        ProgressBar loadingSpinner = view.findViewById(R.id.indeterminateBar);
        postListHelper = new CardListHelper(this.getContext(), loadingSpinner, "POST", this);

        // Setup recycler view to display post list cards
        RecyclerView rvPosts = view.findViewById(R.id.my_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this.getContext()));

        CompletableFuture<String> future = new CompletableFuture<>();
        Amplify.Auth.getCurrentUser(result -> {
            future.complete(result.getUserId());
        }, error -> {
            Log.e("Auth", "Error occurred when getting current user. Redirecting to authenticator");
            Intent intent = new Intent(view.getContext(), AuthenticatorActivity.class);
            startActivity(intent);
        });

        future.thenAccept(uid -> {
            getActivity().runOnUiThread(() -> {
                String myPostsAPI = "/api/posts/getPostsByUser?uid=" + uid + "&diaryEntry=false";
                postListHelper.loadItems(myPostsAPI, rvPosts);
            });
        });
    }

    public void onItemClick(int position)
    {
        // Setup activity switch when a post list card is pressed
        Intent intent = new Intent(this.getContext(), PostEntryActivity.class);
        String pid = postListHelper.getPID(position);
        String uid = postListHelper.getUID(position);
        intent.putExtra("pid", pid);
        intent.putExtra("uid",uid);
        startActivity(intent);
    }
}

package com.example.yappmobile.ProfileManagement.ProfileTabs;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;

import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.PostEntryActivity;
import com.example.yappmobile.R;

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

        ProgressBar loadingSpinner = (ProgressBar) view.findViewById(R.id.indeterminateBar);
        postListHelper = new CardListHelper(this.getContext(), loadingSpinner, "POST", this);

        // Setup recycler view to display post list cards
        RecyclerView rvPosts = (RecyclerView) view.findViewById(R.id.my_posts_list);
        rvPosts.setLayoutManager(new LinearLayoutManager(this.getContext()));

        // TODO: fix this. there's some weird error im getting w the hierarchy/life cycle shit so ask the team
        String userName ="tara";
        String myPostsAPI = "/api/posts/getPostsByUser?userName="+userName+"&diaryEntry=false";
        postListHelper.loadItems(myPostsAPI, rvPosts);
    }

    public void onItemClick(int position)
    {
        // Setup activity switch when a post list card is pressed
        Intent intent = new Intent(this.getContext(), PostEntryActivity.class);
        String pid = postListHelper.getPID(position);
        intent.putExtra("pid", pid);
        startActivity(intent);
    }
}

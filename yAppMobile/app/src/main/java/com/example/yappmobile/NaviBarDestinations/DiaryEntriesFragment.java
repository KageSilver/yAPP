package com.example.yappmobile.NaviBarDestinations;

import static android.app.Activity.RESULT_OK;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;

import androidx.activity.result.ActivityResult;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.auth.AuthUser;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.PostEntryActivity;
import com.example.yappmobile.R;

import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;

public class DiaryEntriesFragment extends Fragment implements IListCardItemInteractions
{
    private RecyclerView diaries;
    private CardListHelper diaryEntryHelper;
    private List<JSONObject> friendUsernames;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        return inflater.inflate(R.layout.fragment_diary_entries, container, false);
    }
    @Override
    public void onViewCreated(View view, Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        ProgressBar loadingSpinner = view.findViewById(R.id.indeterminate_bar);
        diaryEntryHelper = new CardListHelper(this.getContext(), loadingSpinner, "DIARY", this);

        friendUsernames = diaryEntryHelper.handleData(requireArguments().getString("friendUsernames"));
        String selectedDate = requireArguments().getString("selectedDate");

        // sets up recycler view
        diaries = view.findViewById(R.id.diary_list);
        diaries.setLayoutManager(new LinearLayoutManager(this.getContext()));

        getDiaries(selectedDate);
    }

    private void getDiaries(String date)
    {
        CompletableFuture<AuthUser> future = new CompletableFuture<>();
        Amplify.Auth.getCurrentUser(future::complete, error -> {
            Log.e("Auth", "Uh oh! THere's trouble getting the current user", error);
        });

        future.thenAccept(user -> {
            getActivity().runOnUiThread(() -> {
                String uid = user.getUserId();

                // gets diary entries from user for date selected
                String apiUrlUser = "/api/posts/getDiariesByUser?uid=" + uid + "&current=" + date;
                // gets diary entries from users friends for date selected
                String apiUrlFriends = "/api/posts/getDiariesByFriends?uid=" + uid + "&current=" + date;
                diaryEntryHelper.loadDiaries(apiUrlUser, apiUrlFriends, diaries, friendUsernames, uid);
            });
        });
    }

    @Override
    public void onItemClick(int position)
    {
        // Switch activity to view an individual diary entry when a card is clicked
        Intent intent = new Intent(this.getContext(), PostEntryActivity.class);
        String currentPost = diaryEntryHelper.getItem(position).toString();
        intent.putExtra("currentPost", currentPost);
        activityLauncher.launch(intent);
    }

    ActivityResultLauncher<Intent> activityLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            new ActivityResultCallback<ActivityResult>() {
                @Override
                public void onActivityResult(ActivityResult result){
                    if (result.getResultCode() == RESULT_OK)
                    {
                        Intent intent = result.getData();
                        try
                        {
                            String deleted = intent.getStringExtra("delete");
                            diaryEntryHelper.removePost(deleted);
                        }
                        catch (Exception e)
                        {
                            Log.i("POST", "Post was not deleted");
                        }
                    }
                }
            });
}
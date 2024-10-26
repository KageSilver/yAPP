package com.example.yappmobile.Comments;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.Comments.Comment;
import com.example.yappmobile.Comments.CommentAdapter;
import com.example.yappmobile.R;
import com.google.android.material.bottomsheet.BottomSheetDialogFragment;
import org.json.JSONArray;
import org.json.JSONObject;
import java.util.ArrayList;
import java.util.List;

public class CommentsBottomSheet extends BottomSheetDialogFragment {

    private static final String ARG_PID = "pid";
    private String _pid;
    private List<Comment> commentList = new ArrayList<>();
    private CommentAdapter adapter;

    // Use the newInstance method to pass arguments to the fragment
    public static CommentsBottomSheet newInstance(String pid) {
        CommentsBottomSheet fragment = new CommentsBottomSheet();
        Bundle args = new Bundle();
        args.putString(ARG_PID, pid);  // Pass the pid value in the bundle
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Retrieve the pid from the arguments
        if (getArguments() != null) {
            _pid = getArguments().getString(ARG_PID);
        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.bottom_sheet_comments, container, false);

        // Set up RecyclerView
        RecyclerView recyclerView = view.findViewById(R.id.comments_recycler_view);
        recyclerView.setLayoutManager(new LinearLayoutManager(getContext()));
        adapter = new CommentAdapter(commentList);
        recyclerView.setAdapter(adapter);

        // Make API call to get comments
        RestOptions options = RestOptions.builder()
                .addPath("/api/comments/getCommentsByPid?pid=" + _pid)
                .build();

        Amplify.API.get(options,
                restResponse -> {
                    Log.i("MyAmplifyApp", "GET succeeded: " + restResponse.getData().asString());

                    // Parse response and update RecyclerView
                    try {
                        JSONArray commentsArray = new JSONArray(restResponse.getData().asString());
                        for (int i = 0; i < commentsArray.length(); i++) {
                            JSONObject commentObj = commentsArray.getJSONObject(i);
                            String uid = commentObj.getString("uid");
                            String cid = commentObj.getString("cid");
                            String pid = commentObj.getString("pid");
                            String commentBody = commentObj.getString("commentBody");
                            String createAt = commentObj.getString("createAt");
                            String updateAt = commentObj.getString("updateAt");

                            Comment comment = new Comment(uid, commentBody, pid, cid, createAt, updateAt);
                            commentList.add(comment);
                        }

                        // Notify the adapter that data has changed
                        getActivity().runOnUiThread(() -> adapter.notifyDataSetChanged());

                    } catch (Exception e) {
                        Log.e("MyAmplifyApp", "Error parsing comments", e);
                    }
                },
                apiFailure -> Log.e("MyAmplifyApp", "GET failed.", apiFailure)
        );

        return view;
    }
}


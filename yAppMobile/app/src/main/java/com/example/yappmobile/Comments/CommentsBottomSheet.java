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
import com.example.yappmobile.R;
import com.google.android.material.bottomsheet.BottomSheetDialogFragment;
import org.json.JSONArray;
import org.json.JSONObject;
import java.util.ArrayList;
import java.util.List;
import android.widget.Button;
import android.widget.EditText;
import java.util.HashMap;
import java.util.Map;

public class CommentsBottomSheet extends BottomSheetDialogFragment {

    private static final String ARG_PID = "pid";
    private static  final String ARG_UID = "uid";
    private String _pid;
    private String _uid;
    private List<Comment> commentList = new ArrayList<>();
    private CommentAdapter adapter;

    public static CommentsBottomSheet newInstance(String pid,String uid) {
        CommentsBottomSheet fragment = new CommentsBottomSheet();
        Bundle args = new Bundle();
        args.putString(ARG_PID, pid);
        args.putString(ARG_UID,uid);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            _pid = getArguments().getString(ARG_PID);
            _uid = getArguments().getString(ARG_UID);
        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.bottom_sheet_comments, container, false);

        RecyclerView recyclerView = view.findViewById(R.id.comments_recycler_view);
        recyclerView.setLayoutManager(new LinearLayoutManager(getContext()));
        adapter = new CommentAdapter(commentList);
        recyclerView.setAdapter(adapter);

        // Load existing comments
        loadComments();

        // Handle the "Send Comment" button click
        EditText newCommentInput = view.findViewById(R.id.new_comment_input);
        Button sendCommentButton = view.findViewById(R.id.send_comment_button);

        sendCommentButton.setOnClickListener(v -> {
            String newComment = newCommentInput.getText().toString().trim();
            if (!newComment.isEmpty()) {
                // Post the new comment
                postComment(newComment);
                newCommentInput.setText("");  // Clear the input field
            }
        });

        return view;
    }

    // Method to load existing comments
    private void loadComments() {
        RestOptions options = RestOptions.builder()
                .addPath("/api/comments/getCommentsByPid?pid=" + _pid)
                .build();

        Amplify.API.get(options,
                restResponse -> {
                    try {
                        JSONArray commentsArray = new JSONArray(restResponse.getData().asString());
                        for (int i = 0; i < commentsArray.length(); i++) {
                            JSONObject commentObj = commentsArray.getJSONObject(i);
                            String uid = commentObj.getString("uid");
                            String cid = commentObj.getString("cid");
                            String pid = commentObj.getString("pid");
                            String commentBody = commentObj.getString("commentBody");
                            String createdAt = commentObj.getString("createdAt");
                            String updatedAt = commentObj.getString("updatedAt");

                            Comment comment = new Comment(uid, commentBody, pid, cid, createdAt, updatedAt);
                            commentList.add(comment);
                        }
                        getActivity().runOnUiThread(() -> adapter.notifyDataSetChanged());
                    } catch (Exception e) {
                        Log.e("MyAmplifyApp", "Error parsing comments", e);
                    }
                },
                apiFailure -> Log.e("MyAmplifyApp", "GET failed.", apiFailure)
        );
    }

    // Method to post a new comment
    private void postComment(String commentBody) {
        try {
            // Create a map to hold the comment data
            Map<String, String> data = new HashMap<>();
            data.put("pid", _pid);  // Post ID
            data.put("uid", _uid);  // Replace with the actual user ID (get from user session)
            data.put("commentBody", commentBody);

            // Convert the map to a JSON string
            JSONObject postData = new JSONObject(data);

            // Set up the API request options for posting the comment
            RestOptions options = RestOptions.builder()
                    .addPath("/api/comments/postComment")
                    .addBody(postData.toString().getBytes())
                    .build();

            // Make the POST request
            Amplify.API.post(options,
                    response -> {
                        Log.i("Post comment", "POST succeeded: " + response.getData().asString());

                        // Add the new comment to the list and update the RecyclerView
//                        getActivity().runOnUiThread(() -> {
//                            Comment newComment = new Comment("USER_ID", commentBody, _pid, "NEW_CID", "Now", "Now");
//                            commentList.add(newComment);
//                            adapter.notifyItemInserted(commentList.size() - 1);
//                        });
                    },
                    apiFailure -> Log.e("Post comment", "POST failed.", apiFailure)
            );
        } catch (Exception e) {
            Log.e("MyAmplifyApp", "Error posting comment", e);
        }
    }
}



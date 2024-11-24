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
import com.example.yappmobile.Posts.PostEntryActivity;
import com.example.yappmobile.R;
import com.example.yappmobile.Votes.IRefreshListener;
import com.google.android.material.bottomsheet.BottomSheetDialogFragment;
import org.json.JSONArray;
import org.json.JSONObject;
import java.util.ArrayList;
import java.util.List;
import android.widget.Button;
import android.widget.EditText;
import java.util.HashMap;
import java.util.Map;
import android.widget.ProgressBar;

public class CommentsBottomSheet extends BottomSheetDialogFragment implements IRefreshListener {

    private static final String ARG_PID = "pid";
    private static  final String ARG_UID = "uid";

    private static  final String ARG_UUID = "uuid";

    private String _pid;
    private String _uid;

    private String _uuid;
    private final List<Comment> commentList = new ArrayList<>();
    private CommentAdapter _adapter;
    private ProgressBar _progressBar;
    
    private RecyclerView _recyclerView;

    private static final  String LOG_NAME = "COMMENTS";
    private static PostEntryActivity _parent;


    public static CommentsBottomSheet newInstance(String pid,String uid,String uuid, PostEntryActivity parent) {
        CommentsBottomSheet fragment = new CommentsBottomSheet();
        Bundle args = new Bundle();
        args.putString(ARG_PID, pid);
        args.putString(ARG_UID,uid);
        args.putString(ARG_UID,uuid);
        fragment.setArguments(args);
        _parent = parent;
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            _pid = getArguments().getString(ARG_PID);
            _uid = getArguments().getString(ARG_UID);
            _uuid = getArguments().getString(ARG_UUID);
        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        // Inflate the layout for the fragment
        return inflater.inflate(R.layout.bottom_sheet_comments, container, false);
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        // Initialize ProgressBar
        _progressBar = view.findViewById(R.id.progressBar);
        _progressBar.setVisibility(View.VISIBLE);  // Show the loading spinner

        // Initialize RecyclerView and Adapter
        _recyclerView = view.findViewById(R.id.comments_recycler_view);
        _recyclerView.setLayoutManager(new LinearLayoutManager(getContext()));

        // Initialize the adapter with the fragment reference
        _adapter = new CommentAdapter(commentList, _uid, this, _parent,this);
        _recyclerView.setAdapter(_adapter);

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
    }


    private void loadComments() {
        _progressBar.setVisibility(View.VISIBLE);
        RestOptions options = RestOptions.builder()
                .addPath("/api/comments/getCommentsByPid?pid=" + _pid)
                .build();

        Amplify.API.get(options,
                restResponse -> {
                    try {
                        commentList.clear(); // Clear the list before adding new items
                        JSONArray commentsArray = new JSONArray(restResponse.getData().asString());
                        for (int i = 0; i < commentsArray.length(); i++) {
                            JSONObject commentObj = commentsArray.getJSONObject(i);
                            String uid = commentObj.getString("uid");
                            String cid = commentObj.getString("cid");
                            String pid = commentObj.getString("pid");
                            String upvotes = commentObj.getString("upvotes");
                            String downvotes = commentObj.getString("downvotes");
                            String commentBody = commentObj.getString("commentBody");
                            String createdAt = commentObj.getString("createdAt");
                            String updatedAt = commentObj.getString("updatedAt");

                            Comment comment = new Comment(uid, commentBody, pid, cid, createdAt, updatedAt,upvotes,downvotes);
                            commentList.add(comment);
                        }

                        getActivity().runOnUiThread(() -> {
                            _adapter.notifyDataSetChanged();
                            _progressBar.setVisibility(View.GONE);  // Hide the loading spinner when done
                        });

                    } catch (Exception e) {
                        Log.e(LOG_NAME, "Error parsing comments", e);
                    }
                },
                apiFailure -> {
                    Log.e(LOG_NAME, "GET failed.", apiFailure);
                    getActivity().runOnUiThread(() -> _progressBar.setVisibility(View.GONE));  // Hide the loading spinner if failed
                }
        );
    }


    // Method to post a new comment
    private void postComment(String commentBody) {
        try {
            // Create a map to hold the comment data
            Map<String, String> data = new HashMap<>();
            data.put("pid", _pid);
            data.put("uid", _uid);
            data.put("commentBody", commentBody);


            // Convert the map to a JSON string
            JSONObject postData = new JSONObject(data);
            Log.d(LOG_NAME,postData.toString());


            // Set up the API request options for posting the comment
            String apiUrl = "/api/comments/createComment";
            RestOptions options = RestOptions.builder()
                    .addPath(apiUrl)
                    .addBody(postData.toString().getBytes())
                    .addHeader("Content-Type", "application/json")
                    .build();
            Amplify.API.post(options,
                    response -> {
                        Log.i(LOG_NAME, "POST response: " + response.getData().asString());

                        // Hide the progress bar once the response is received
                        getActivity().runOnUiThread(() -> {
                            _progressBar.setVisibility(View.GONE);
                            loadComments();
                        });
                    },
                    error -> {
                        Log.e(LOG_NAME, "POST request failed", error);

                        // Hide the progress bar if the request fails
                        getActivity().runOnUiThread(() -> {
                            _progressBar.setVisibility(View.GONE);
                        });
                    }
            );

        } catch (Exception e) {
            Log.e(LOG_NAME, "Error posting comment", e);
        }
    }


    @Override
    public void refreshUI() {
        loadComments();
    }
}



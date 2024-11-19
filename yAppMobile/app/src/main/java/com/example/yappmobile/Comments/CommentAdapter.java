package com.example.yappmobile.Comments;

import static com.example.yappmobile.Votes.Votes.checkVoted;

import android.annotation.SuppressLint;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.PostEntryActivity;
import com.example.yappmobile.R;
import com.example.yappmobile.Utils.DateUtils;
import com.example.yappmobile.Votes.IVoteHandler;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicReference;

public class CommentAdapter extends RecyclerView.Adapter<CommentAdapter.CommentViewHolder> implements IVoteHandler {

    private static final String LOG_NAME = "COMMENT_ADAPTER";
    private final ArrayList _commentList;
    private final String _uuid;
    private final Fragment _parentFragment;
    private final PostEntryActivity _parentActivity;
    private final IListCardItemInteractions _interaction;


    // Constructor to initialize the comment list
    public CommentAdapter(List<Comment> commentList, String uuid, Fragment parentFragment, PostEntryActivity parentActivity, IListCardItemInteractions refreshListener) {

        _commentList = (ArrayList) commentList;
        _uuid = uuid;
        _parentFragment = parentFragment;
        _parentActivity = parentActivity;
        _interaction = refreshListener;
    }

    @NonNull
    @Override
    public CommentViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.comment_item_layout, parent, false);
        return new CommentViewHolder(view);
    }


    @SuppressLint("ResourceAsColor")
    @Override
    public void onBindViewHolder(@NonNull CommentViewHolder holder, int position) {
        Comment comment = (Comment) _commentList.get(position);

        // Set comment body and time
        holder.commentBody.setText(comment.getCommentBody());
        holder.commentTime.setText(DateUtils.convertUtcToFormattedTime(comment.getCreateAt()));

        // Get and set upvotes and downvotes
        holder.commentUpvotes.setText(formatVoteCount(comment.getUpvotes()));
        holder.commentDownvotes.setText(formatVoteCount(comment.getDownvotes()));

        // Dynamically update vote UI on load
        updateVoteStatusOnLoad(holder, comment);

        // Check if the comment belongs to the current user
        if (comment.getUid() != null && comment.getUid().equals(_uuid)) {
            // Apply the custom drawable with border for current user's comment
            holder.itemView.setBackground(ContextCompat.getDrawable(holder.itemView.getContext(), R.drawable.item_border));

            // Set click listener to open the Edit/Delete Bottom Sheet
            holder.itemView.setOnClickListener(v -> {
                EditDeleteBottomSheet editDeleteBottomSheet = EditDeleteBottomSheet.newInstance(comment.getJsonObject().toString(), this);
                editDeleteBottomSheet.show(_parentFragment.getParentFragmentManager(), "EditDeleteBottomSheet");
            });
        } else {
            // Reset the background for non-current user's comments
            holder.itemView.setBackground(ContextCompat.getDrawable(holder.itemView.getContext(), R.drawable.rounded_edittext_background));

            // Remove the click listener for non-user comments
            holder.itemView.setOnClickListener(null);
        }

        // Set up button listeners
        holder.upButton.setOnClickListener(v -> handleVote(holder, comment, true));
        holder.downButton.setOnClickListener(v -> handleVote(holder, comment, false));
    }

    // Update vote status dynamically on load
    private void updateVoteStatusOnLoad(CommentViewHolder holder, Comment comment) {
        AtomicReference<AtomicBoolean> upVoted = new AtomicReference<>(new AtomicBoolean(false));
        AtomicReference<AtomicBoolean> downVoted = new AtomicReference<>(new AtomicBoolean(false));

        try {
            CompletableFuture<Boolean> upVoteFuture = checkVoted(true, false, _uuid, comment.getCid(), LOG_NAME);
            CompletableFuture<Boolean> downVoteFuture = checkVoted(false, false, _uuid, comment.getCid(), LOG_NAME);

            CompletableFuture.allOf(upVoteFuture, downVoteFuture).thenRun(() -> {
                updateVoteStatus(upVoteFuture, upVoted, "Upvoted");
                updateVoteStatus(downVoteFuture, downVoted, "Downvoted");

                _parentActivity.runOnUiThread(() -> updateVoteUI(holder, upVoted.get().get(), downVoted.get().get(),comment));
            }).exceptionally(e -> {
                Log.e(LOG_NAME, "Error during vote checks: " + e.getMessage(), e);
                return null;
            });
        } catch (UnsupportedEncodingException e) {
            Log.e(LOG_NAME, "Error encoding vote request: " + e.getMessage());
        }
    }

    // Format vote count, hide "0" values
    private String formatVoteCount(String voteCount) {
        return "0".equals(voteCount) ? "" : voteCount;
    }

    // Handle vote logic for upvotes and downvotes
    private void handleVote(CommentViewHolder holder, Comment comment, boolean isUpvote) {
        AtomicReference<AtomicBoolean> upVoted = new AtomicReference<>(new AtomicBoolean(false));
        AtomicReference<AtomicBoolean> downVoted = new AtomicReference<>(new AtomicBoolean(false));

        try {
            CompletableFuture<Boolean> upVoteFuture = checkVoted(true, false, _uuid, comment.getCid(), LOG_NAME);
            CompletableFuture<Boolean> downVoteFuture = checkVoted(false, false, _uuid, comment.getCid(), LOG_NAME);

            CompletableFuture.allOf(upVoteFuture, downVoteFuture).thenRun(() -> {
                updateVoteStatus(upVoteFuture, upVoted, "Upvoted");
                updateVoteStatus(downVoteFuture, downVoted, "Downvoted");

                _parentActivity.runOnUiThread(() -> {
                    onVote(false, isUpvote, _uuid, comment.getCid(), isUpvote ? holder.upButton : holder.downButton,
                            isUpvote ? upVoted.get() : downVoted.get(), LOG_NAME, upVoted.get(), downVoted.get());
                    updateVoteUI(holder, upVoted.get().get(), downVoted.get().get(),comment);
                    try {
                        Thread.sleep(500);
                    } catch (InterruptedException e) {
                        throw new RuntimeException(e);
                    }
                    _interaction.refreshUI();
                });

            }).exceptionally(e -> {
                Log.e(LOG_NAME, "Error during vote checks: " + e.getMessage(), e);
                return null;
            });
        } catch (UnsupportedEncodingException e) {
            Log.e(LOG_NAME, "Error encoding vote request: " + e.getMessage());
        }
    }



    // Update vote status from CompletableFuture
    private void updateVoteStatus(CompletableFuture<Boolean> future, AtomicReference<AtomicBoolean> voteRef, String logPrefix) {
        voteRef.set(new AtomicBoolean(false));
        future.thenAccept(result -> {
            voteRef.get().set(result);
            Log.i("Vote", logPrefix + ": " + voteRef.get().get());
        }).exceptionally(e -> {
            Log.e("Vote", "Error while checking vote status: " + e.getMessage(), e);
            return null;
        });
    }

    // Update the UI based on vote status
    private void updateVoteUI(CommentViewHolder holder, boolean upVoted, boolean downVoted, Comment comment) {
        holder.upButton.setBackgroundResource(upVoted ? R.drawable.ic_up_activated : R.drawable.ic_up);
        holder.downButton.setBackgroundResource(downVoted ? R.drawable.ic_down_activated : R.drawable.ic_down);

    }

    //helper method
    private void fetchCommentById(String cid, CommentViewHolder holder) {
        String path = "/api/comments/getCommentById?cid=" + cid;

        RestOptions options = RestOptions.builder()
                .addPath(path)
                .addHeader("Content-Type", "application/json")
                .build();

        Amplify.API.get(options, response -> {
            this._parentActivity.runOnUiThread(() -> {
                try {
                    String rawResponse = response.getData().asString();
                    JSONObject commentObj = new JSONObject(rawResponse);

                    String upvotes = commentObj.getString("upvotes");
                    String downvotes = commentObj.getString("downvotes");

                    // Update the Comment object
                    for (int i = 0; i < _commentList.size(); i++) {
                        Comment comment = (Comment) _commentList.get(i);
                        if (comment.getCid().equals(cid)) {
                            comment.setUpvotes(upvotes);
                            comment.setDownvotes(downvotes);
                            _commentList.set(i, comment);
                            break;
                        }
                    }

                    // Update the UI
                    holder.commentUpvotes.setText(upvotes.equals("0") ? "" : upvotes);
                    holder.commentDownvotes.setText(downvotes.equals("0") ? "" : downvotes);

                    notifyItemChanged(holder.getAdapterPosition());
                } catch (JSONException e) {
                    Log.e(LOG_NAME, "JSON Parsing Error: " + e.getMessage(), e);
                }
            });
        }, error -> {
            Log.e(LOG_NAME, "Error fetching comment: " + error.getMessage(), error);
            this._parentActivity.runOnUiThread(() -> {
                Toast.makeText(_parentActivity, "Failed to fetch updated comment.", Toast.LENGTH_SHORT).show();
            });
        });
    }



    public void removeComment(String cid) {
        boolean found = false;
        for (int i = 0; i < _commentList.size() && !found; i++) {
            if (((Comment) _commentList.get(i)).getCid().equals(cid)) {
                found = true;
                _commentList.remove(i);
                _parentActivity.runOnUiThread(() -> {
                    notifyDataSetChanged();
                });
            }
        }
    }

    public void updateComment(String cid, String newConent) {
        boolean found = false;
        for (int i = 0; i < _commentList.size() && !found; i++) {
            Comment current = (Comment) _commentList.get(i);

            if (current.getCid().equals(cid)) {
                found = true;
                current.setCommentBody(newConent);
                _commentList.set(i, current);
                _parentActivity.runOnUiThread(() -> {
                    notifyDataSetChanged();
                });
            }
        }
    }

    @Override
    public int getItemCount() {
        return _commentList.size();
    }


    // ViewHolder class for comment items
    public static class CommentViewHolder extends RecyclerView.ViewHolder {
        TextView commentBody, commentTime, commentUpvotes, commentDownvotes;
        ImageButton upButton, downButton;

        public CommentViewHolder(@NonNull View itemView) {
            super(itemView);
            commentBody = itemView.findViewById(R.id.comment_body);
            commentTime = itemView.findViewById(R.id.comment_time);
            commentUpvotes = itemView.findViewById(R.id.upvote_count);
            commentDownvotes = itemView.findViewById(R.id.downvote_count);
            upButton = itemView.findViewById(R.id.upvote_button);
            downButton = itemView.findViewById(R.id.downvote_button);
        }
    }
}

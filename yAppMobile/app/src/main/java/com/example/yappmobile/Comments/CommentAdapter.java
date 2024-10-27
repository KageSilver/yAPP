package com.example.yappmobile.Comments;
import android.annotation.SuppressLint;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.RecyclerView;
import com.example.yappmobile.R;

import java.util.ArrayList;
import java.util.List;

public class CommentAdapter extends RecyclerView.Adapter<CommentAdapter.CommentViewHolder> {

    private ArrayList _commentList;
    private String _uuid;

    private Fragment _parentFragment;


    // Constructor to initialize the comment list
    public CommentAdapter(List<Comment> commentList, String uuid, Fragment parentFragment) {

        _commentList = (ArrayList) commentList;
        _uuid = uuid;
        _parentFragment = parentFragment;
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
        holder.commentTime.setText(comment.getCreateAt());

        // Check if the comment belongs to the current user
        if (comment.getUid() != null && comment.getCommentBody() != null && comment.getCid() != null && comment.getUid().equals(_uuid)) {
            // Apply the custom drawable with border for current user's comment
            holder.itemView.setBackground(ContextCompat.getDrawable(holder.itemView.getContext(), R.drawable.item_border));

            // Open the EditDeleteBottomSheet when the comment is clicked
            // Set click listener to open the Edit/Delete Bottom Sheet
            holder.itemView.setOnClickListener(v -> {
                EditDeleteBottomSheet editDeleteBottomSheet = EditDeleteBottomSheet.newInstance(comment.getCommentBody(), comment.getCid());
                editDeleteBottomSheet.show(_parentFragment.getParentFragmentManager(), "EditDeleteBottomSheet");
            });
        } else {
            // Reset the background for non-current user's comments (to avoid reusing the view holder's appearance)
            holder.itemView.setBackground(ContextCompat.getDrawable(holder.itemView.getContext(), R.drawable.rounded_edittext_background));

            // Remove the click listener for non-user comments
            holder.itemView.setOnClickListener(null);
        }
    }

    @Override
    public int getItemCount() {
        return _commentList.size();
    }

    // ViewHolder class for comment items
    public static class CommentViewHolder extends RecyclerView.ViewHolder {
        TextView commentBody, commentTime;

        public CommentViewHolder(@NonNull View itemView) {
            super(itemView);
            commentBody = itemView.findViewById(R.id.comment_body);
            commentTime = itemView.findViewById(R.id.comment_time);
        }
    }
}

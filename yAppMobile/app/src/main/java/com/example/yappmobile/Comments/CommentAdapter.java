package com.example.yappmobile.Comments;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.yappmobile.R;

import java.util.ArrayList;
import java.util.List;

public class CommentAdapter extends RecyclerView.Adapter<CommentAdapter.CommentViewHolder> {

    private ArrayList commentList;

    // Constructor to initialize the comment list
    public CommentAdapter(List<Comment> commentList) {
        this.commentList = new ArrayList<>();  // Initialize the ArrayList
        this.commentList.add(new Comment(
                "tes",    // uid
                "dddd",   // commentBody
                "dd",     // pid
                "dd",     // cid
                "ddd",    // createAt
                "dd"      // updateAt
        ));
//        this.commentList = commentList;
    }

    @NonNull
    @Override
    public CommentViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.comment_item_layout, parent, false);
        return new CommentViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull CommentViewHolder holder, int position) {
        Comment comment = (Comment) commentList.get(position);
        holder.commentBody.setText(comment.getCommentBody());
        holder.commentTime.setText(comment.getCreateAt());
    }

    @Override
    public int getItemCount() {
        return commentList.size();
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

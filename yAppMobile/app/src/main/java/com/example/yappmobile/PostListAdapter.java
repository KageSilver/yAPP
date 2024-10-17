package com.example.yappmobile;

import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.cardview.widget.CardView;
import androidx.recyclerview.widget.RecyclerView;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.List;

public class PostListAdapter extends RecyclerView.Adapter<PostListAdapter.myViewHolder>
{
    private final Context context;
    private List<JSONObject> postList;
    private final ItemListCardInterface postListCardInterface;

    public PostListAdapter(Context context,
                           List<JSONObject> postList,
                           ItemListCardInterface itemListCardInterface)
    {
        this.context = context;
        this.postList = postList;
        this.postListCardInterface = itemListCardInterface;
    }

    @NonNull
    @Override
    public PostListAdapter.myViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType)
    {
        // Create each post list card (each view) to be displayed
        LayoutInflater inflater = LayoutInflater.from(context);
        View view = inflater.inflate(R.layout.post_list_row, parent, false);
        return new PostListAdapter.myViewHolder(view, postListCardInterface);
    }

    @Override
    public void onBindViewHolder(PostListAdapter.myViewHolder holder, int position)
    {
        // Setup what is displayed on each post list card (each of the views)
        // based on position of the recycler view
        try
        {
            String title = postList.get(position).get("postTitle").toString();
            String body = postList.get(position).get("postBody").toString();

            holder.postTitle.setText(title);
            holder.postBody.setText(body);
        }
        catch (JSONException jsonException)
        {
            Log.e("JSON", "Error parsing JSON", jsonException);
        }
    }

    @Override
    public int getItemCount()
    {
        return postList.size();
    }

    public void updatePostList(List<JSONObject> newPostList)
    {
        this.postList = newPostList;
    }

    // Class to create items inside a post list card (inside a view)
    public static class myViewHolder extends RecyclerView.ViewHolder
    {
        public TextView postTitle, postBody;
        public CardView postCard;
        public myViewHolder(View itemView, ItemListCardInterface itemListCardInterface)
        {
            super(itemView);

            postTitle = (TextView) itemView.findViewById(R.id.post_title);
            postBody = (TextView) itemView.findViewById(R.id.post_body);
            postCard = (CardView) itemView.findViewById(R.id.post_card);

            // Setup an on click listener for the post list card
            itemView.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    if(itemListCardInterface != null)
                    {
                        int position = getAdapterPosition();
                        if(position != RecyclerView.NO_POSITION)
                        {
                            itemListCardInterface.onItemClick(position);
                        }
                    }
                }
            });
        }
    }
}

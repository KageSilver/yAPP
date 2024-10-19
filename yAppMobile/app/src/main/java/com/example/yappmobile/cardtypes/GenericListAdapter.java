package com.example.yappmobile.cardtypes;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.yappmobile.R;

import java.util.List;

public class GenericListAdapter extends RecyclerView.Adapter<RecyclerView.ViewHolder>
{
    private final Context context;
    private final List<IListCardItem> itemList;

    public GenericListAdapter(Context context, List<IListCardItem> itemList)
    {
        this.context = context;
        this.itemList = itemList;
    }

    @Override
    public int getItemViewType(int position)
    {
        return itemList.get(position).getType();
    }

    @NonNull
    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType)
    {
        if (viewType == CardType.FRIEND_REQUEST.getValue())
        {
            View view = LayoutInflater.from(context).inflate(R.layout.friend_request_card, parent, false);
            return new FriendRequestViewHolder(view);
        }
        else if (viewType == CardType.CURRENT_FRIEND.getValue())
        {
            View view = LayoutInflater.from(context).inflate(R.layout.current_friend_card, parent, false);
            return new CurrentFriendViewHolder(view);
        }
        else if (viewType == CardType.POST.getValue())
        {
            View view = LayoutInflater.from(context).inflate(R.layout.post_card, parent, false);
            return new CurrentFriendViewHolder(view);
        }
        throw new RuntimeException("Unknown view type");
    }

    @Override
    public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position)
    {
        IListCardItem item = itemList.get(position);
        if (holder instanceof FriendRequestViewHolder)
        {
            ((FriendRequestViewHolder) holder).bind((FriendRequestCard) item);
        }
        else if (holder instanceof CurrentFriendViewHolder)
        {
            ((CurrentFriendViewHolder) holder).bind((CurrentFriendCard) item);
        }
        else if (holder instanceof PostViewHolder)
        {
            ((PostViewHolder) holder).bind((PostCard) item);
        }
    }

    @Override
    public int getItemCount()
    {
        return itemList.size();
    }

    // ViewHolder for friend requests
    public static class FriendRequestViewHolder extends RecyclerView.ViewHolder
    {
        public TextView name;
        public Button acceptButton, declineButton;

        public FriendRequestViewHolder(View itemView)
        {
            super(itemView);
            name = itemView.findViewById(R.id.friend_username);
            acceptButton = itemView.findViewById(R.id.accept_button);
            declineButton = itemView.findViewById(R.id.decline_button);
        }

        public void bind(FriendRequestCard card)
        {
            // name.setText(card.getName());
            // TODO: Add button click listeners here, if necessary
        }
    }

    // ViewHolder for current friends
    public static class CurrentFriendViewHolder extends RecyclerView.ViewHolder
    {
        public TextView friendName;
        public Button unfriendButton;

        public CurrentFriendViewHolder(View itemView)
        {
            super(itemView);
            friendName = itemView.findViewById(R.id.friend_username);
            unfriendButton = itemView.findViewById(R.id.unfollow_button);
        }

        public void bind(CurrentFriendCard card)
        {
            // friendName.setText(card.getFriendName());
            // TODO: Add button click listeners here, if necessary
        }
    }

    public static class PostViewHolder extends RecyclerView.ViewHolder
    {
        public TextView friendName;
        public Button unfriendButton;

        public PostViewHolder(View itemView)
        {
            super(itemView);
            friendName = itemView.findViewById(R.id.friend_username);
            unfriendButton = itemView.findViewById(R.id.unfollow_button);
        }

        public void bind(PostCard card)
        {
            // friendName.setText(card.getFriendName());
            // TODO: Add button click listeners here, if necessary
        }
    }
}

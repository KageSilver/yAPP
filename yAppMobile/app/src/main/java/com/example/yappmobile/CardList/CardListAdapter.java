package com.example.yappmobile.CardList;

import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.R;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.List;

// Manages the creation, data population, and click events of individual CardItems
public class CardListAdapter extends RecyclerView.Adapter<RecyclerView.ViewHolder>
{
    private final IListCardItemInteractions itemInteractions; // Handles CardItem click events
    private final Context context; // Where the CardItems are contained
    private final String cardType;
    private List<JSONObject> itemList; // List of CardItems

    public CardListAdapter(Context context, List<JSONObject> itemList,
                           String cardType, IListCardItemInteractions itemInteractions)
    {
        this.context = context;
        this.itemList = itemList;
        this.cardType = cardType;
        this.itemInteractions = itemInteractions;
    }

    @NonNull
    @Override
    public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType)
    {
        // Create each CardItem to be displayed
        LayoutInflater inflater = LayoutInflater.from(context);

        // Create custom ViewHolder depending on the card type
        switch (cardType)
        {
            case "FRIEND_REQUEST":
            {
                View view = inflater.inflate(R.layout.card_friend_request, parent, false);
                return new FriendRequestViewHolder(view);
            }
            case "CURRENT_FRIEND":
            {
                View view = inflater.inflate(R.layout.card_current_friend, parent, false);
                return new CurrentFriendViewHolder(view, itemInteractions);
            }
            case "POST":
            {
                View view = inflater.inflate(R.layout.card_post, parent, false);
                return new PostViewHolder(view, itemInteractions);
            }
        }
        throw new RuntimeException("Unknown view type");
    }

    @Override
    public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position)
    {
        // Set up what is displayed on each CardItem based on position of the recycler view
        JSONObject item = itemList.get(position);
        if (holder instanceof FriendRequestViewHolder)
        {
            ((FriendRequestViewHolder) holder).bind(item);
        }
        else if (holder instanceof CurrentFriendViewHolder)
        {
            ((CurrentFriendViewHolder) holder).bind(item);
        }
        else if (holder instanceof PostViewHolder)
        {
            ((PostViewHolder) holder).bind(item);
        }
    }

    // Return the size of your dataset (invoked by the layout manager)
    @Override
    public int getItemCount()
    {
        return itemList.size();
    }

    public void updateList(List<JSONObject> cardItemList)
    {
        this.itemList = cardItemList;
    }

    // Populate data into a FriendRequestCard
    public static class FriendRequestViewHolder extends RecyclerView.ViewHolder
    {
        public TextView sender;
        public Button acceptButton, declineButton;

        public FriendRequestViewHolder(View itemView)
        {
            super(itemView);
            sender = itemView.findViewById(R.id.friend_username);
            acceptButton = itemView.findViewById(R.id.accept_button);
            declineButton = itemView.findViewById(R.id.decline_button);

            acceptButton.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    // handle the api call here girlie
                }
            });

            declineButton.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    // handle the api call here girlie
                    Log.i("OnClick", "Wait...You don't want to be my friend!? D:");
                }
            });
        }

        public void bind(JSONObject card)
        {
            try
            {
                sender.setText(card.get("FromUserName").toString());
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }
    }

    // Populate data into a CurrentFriendCard
    public static class CurrentFriendViewHolder extends RecyclerView.ViewHolder
    {
        public TextView friendName;
        public Button unfriendButton;

        public CurrentFriendViewHolder(View itemView, IListCardItemInteractions friendCardInteractions)
        {
            super(itemView);
            friendName = itemView.findViewById(R.id.friend_username);
            unfriendButton = itemView.findViewById(R.id.unfollow_button);

            // Set up an onClickListener for the friend list card's unfollow button
            unfriendButton.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    if(friendCardInteractions != null)
                    {
                        int position = getAdapterPosition();
                        if (position != RecyclerView.NO_POSITION)
                        {
                            friendCardInteractions.onItemClick(position);
                        }
                    }
                }
            });
        }

        public void bind(JSONObject card)
        {
            try
            {
                String personA = card.get("FromUserName").toString();
                String personB = card.get("ToUserName").toString();
                Amplify.Auth.getCurrentUser(
                        result ->
                        {
                            if(personA.equals(result.getUsername()))
                            {
                                friendName.setText(personB);
                            }
                            else
                            {
                                friendName.setText(personA);
                            }
                        },
                        error ->
                        {
                            Log.e("CardListAdapter", "Error populating Friend card", error);
                        }
                );
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }
    }

    public static class PostViewHolder extends RecyclerView.ViewHolder
    {
        public TextView postTitle, postDate, postBody;

        public PostViewHolder(View itemView, IListCardItemInteractions postCardInteractions)
        {
            super(itemView);

            postTitle = itemView.findViewById(R.id.post_title);
            postDate = itemView.findViewById(R.id.post_date);
            postBody = itemView.findViewById(R.id.post_body);

            // Set up an onClickListener for the post list card
            itemView.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    if(postCardInteractions != null)
                    {
                        int position = getAdapterPosition();
                        if(position != RecyclerView.NO_POSITION)
                        {
                            postCardInteractions.onItemClick(position);
                        }
                    }
                }
            });
        }

        // Populate data into a PostCard
        public void bind(JSONObject card)
        {
            try
            {
                postTitle.setText(card.get("postTitle").toString());
                postDate.setText(card.get("createdAt").toString());
                postBody.setText(card.get("postBody").toString());
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }
    }
}

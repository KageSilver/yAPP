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

    // Triggers when creating each CardItem to be displayed
    @NonNull @Override
    public RecyclerView.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType)
    {
        LayoutInflater inflater = LayoutInflater.from(context);

        // Create custom ViewHolder depending on the card type
        switch (cardType)
        {
            case "FRIEND_REQUEST":
            {
                View view = inflater.inflate(R.layout.card_friend_request, parent, false);
                return new FriendRequestViewHolder(view, (IListRequestCardInteractions) itemInteractions);
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
            case "DIARY":
            {
                View view = inflater.inflate(R.layout.card_diary, parent, false);
                return new DiaryViewHolder(view, itemInteractions);
            }
            case "AWARD":
            {
                View view = inflater.inflate(R.layout.card_award, parent, false);
                return new AwardViewHolder(view, itemInteractions);
            }
        }
        throw new RuntimeException("Unknown view type");
    }

    // Set up what is displayed on each CardItem based on CardType and
    // the position of the recycler view
    @Override
    public void onBindViewHolder(@NonNull RecyclerView.ViewHolder holder, int position)
    {
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
        else if (holder instanceof DiaryViewHolder)
        {
            ((DiaryViewHolder) holder).bind(item);
        }
        else if (holder instanceof AwardViewHolder)
        {
            ((AwardViewHolder) holder).bind(item);
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

        public FriendRequestViewHolder(View itemView, IListRequestCardInteractions requestCardInteractions)
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
                    requestCardInteractions.onAcceptClick(getAdapterPosition());
                }
            });

            declineButton.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    requestCardInteractions.onDeclineClick(getAdapterPosition());
                }
            });
        }

        public void bind(JSONObject card)
        {
            try
            {
                String personA = card.get("FromUserName").toString();
                String personB = card.get("ToUserName").toString();
                Amplify.Auth.getCurrentUser(result -> {
                    if (personA.equals(result.getUsername()))
                    {
                        // If the current user is the sender, only view the request as "pending"
                        sender.setText(personB + ": pending");
                        acceptButton.setVisibility(View.GONE);
                        declineButton.setVisibility(View.GONE);
                    }
                    else
                    {
                        // If the current user is the receiver, enable "accept" and "decline"
                        sender.setText(personA);
                        acceptButton.setVisibility(View.VISIBLE);
                        declineButton.setVisibility(View.VISIBLE);
                    }
                }, error -> {
                    Log.e("CardListAdapter", "Error populating Friend card", error);
                });
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
                    if (friendCardInteractions != null)
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
                Amplify.Auth.getCurrentUser(result -> {
                    if (personA.equals(result.getUsername()))
                    {
                        friendName.setText(personB);
                    }
                    else
                    {
                        friendName.setText(personA);
                    }
                }, error -> {
                    Log.e("CardListAdapter", "Error populating Friend card", error);
                });
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }
    }

    // Populate data into a PostCard
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
                    if (postCardInteractions != null)
                    {
                        int position = getAdapterPosition();
                        if (position != RecyclerView.NO_POSITION)
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

    // Populate data into a DiaryCard
    public static class DiaryViewHolder extends RecyclerView.ViewHolder
    {
        public TextView postTitle, postDate, postBody, postAuthor;

        public DiaryViewHolder(View itemView, IListCardItemInteractions diaryCardInteractions)
        {
            super(itemView);

            postTitle = itemView.findViewById(R.id.diary_title);
            postDate = itemView.findViewById(R.id.diary_date);
            postBody = itemView.findViewById(R.id.diary_body);
            postAuthor = itemView.findViewById(R.id.diary_author);

            // Set up an onClickListener for the post list card
            itemView.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    if (diaryCardInteractions != null)
                    {
                        int position = getAdapterPosition();
                        if (position != RecyclerView.NO_POSITION)
                        {
                            diaryCardInteractions.onItemClick(position);
                        }
                    }
                }
            });
        }

        // Populate data into a DiaryCard
        public void bind(JSONObject card)
        {
            try
            {
                postTitle.setText(card.get("postTitle").toString());
                postDate.setText(card.get("createdAt").toString());
                postBody.setText(card.get("postBody").toString());
                postAuthor.setText(card.get("username").toString());
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }
    }

    // Populate data into an AwardCard
    public static class AwardViewHolder extends RecyclerView.ViewHolder
    {
        public TextView awardTitle, awardDate, awardTypeTier;

        public AwardViewHolder(View itemView, IListCardItemInteractions awardCardInteractions)
        {
            super(itemView);

            awardTitle = itemView.findViewById(R.id.award_title);
            awardDate = itemView.findViewById(R.id.award_date);
            awardTypeTier = itemView.findViewById(R.id.award_type_tier);

            // Set up an onClickListener for the award list card
            itemView.setOnClickListener(new View.OnClickListener()
            {
                @Override
                public void onClick(View v)
                {
                    if (awardCardInteractions != null)
                    {
                        int position = getAdapterPosition();
                        if (position != RecyclerView.NO_POSITION)
                        {
                            awardCardInteractions.onItemClick(position);
                        }
                    }
                }
            });
        }

        // Populate data into a DiaryCard
        public void bind(JSONObject card)
        {
            try
            {
                awardTitle.setText(card.get("name").toString());
                awardDate.setText(card.get("createdAt").toString());

                String type = card.get("type").toString();
                String tier = card.get("tier").toString();
                awardTypeTier.setText(type + " award: tier " + tier);
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }
    }
}

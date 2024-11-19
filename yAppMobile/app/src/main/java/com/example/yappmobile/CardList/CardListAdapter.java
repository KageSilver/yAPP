package com.example.yappmobile.CardList;

import static com.example.yappmobile.Votes.Votes.addVotes;
import static com.example.yappmobile.Votes.Votes.checkVoted;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.R;
import com.example.yappmobile.Utils.DateUtils;
import com.example.yappmobile.Votes.IVoteHandler;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.UnsupportedEncodingException;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicReference;

// Manages the creation, data population, and click events of individual CardItems
public class CardListAdapter extends RecyclerView.Adapter<RecyclerView.ViewHolder>
{
    private final IListCardItemInteractions itemInteractions; // Handles CardItem click events
    private final Context context; // Where the CardItems are contained
    private final String cardType;
    private List<JSONObject> itemList; // List of CardItems
    private SharedPreferences preferences;

    public CardListAdapter(Context context, List<JSONObject> itemList,
                           String cardType, IListCardItemInteractions itemInteractions, SharedPreferences preferences)
    {
        this.context = context;
        this.itemList = itemList;
        this.cardType = cardType;
        this.itemInteractions = itemInteractions;
        this.preferences = preferences;
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
                return new PostViewHolder(view, itemInteractions,context);
            }
            case "DIARY":
            {
                View view = inflater.inflate(R.layout.card_diary, parent, false);
                return new DiaryViewHolder(view, itemInteractions);
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

            try {
              CompletableFuture<Boolean> up =  checkVoted(true,true,preferences.getString("uuid",""),item.getString("pid"),"ADAPTER");
                up.thenAccept(voted -> {
                    // This block is executed once the CompletableFuture completes
                   ((PostViewHolder) holder).setUpvoted(voted);
                }).exceptionally(error -> {
                    Log.e("ADAPTER", "Error checking vote status", error);
                    return null;
                });

                CompletableFuture<Boolean> down =  checkVoted(false,true,preferences.getString("uuid",""),item.getString("pid"),"ADAPTER");
                down.thenAccept(voted -> {
                    // This block is executed once the CompletableFuture completes
                    ((PostViewHolder) holder).setDownvoted(voted);
                }).exceptionally(error -> {
                    Log.e("ADAPTER", "Error checking vote status", error);
                    return null;
                });

            } catch (UnsupportedEncodingException ignored) {
                throw new RuntimeException(ignored);
            } catch (JSONException e) {
                throw new RuntimeException(e);
            }

        }
        else if (holder instanceof DiaryViewHolder)
        {
            ((DiaryViewHolder) holder).bind(item);
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
                        // And prevent them from accepting or declining the request
                        sender.setText(personB + ": pending");
                        acceptButton.setVisibility(View.GONE);
                        declineButton.setVisibility(View.GONE);
                    }
                    else
                    {
                        sender.setText(personA);
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
    public static class PostViewHolder extends RecyclerView.ViewHolder implements IVoteHandler
    {
        public TextView postTitle, postDate, postBody, postUpvotes, postDownvotes;
        public ImageButton upButton, downButton;
        public Context context;
        public IListCardItemInteractions postCardInteractions;
        private static final String LOG_NAME = "POST_APDATER";

       // private final VoteHandler voteHandler;

        public PostViewHolder(View itemView, IListCardItemInteractions postCardInteractions, Context context)
        {
            super(itemView);
            this.context = context;
            this.postCardInteractions = postCardInteractions;

            postTitle = itemView.findViewById(R.id.post_title);
            postDate = itemView.findViewById(R.id.post_date);
            postBody = itemView.findViewById(R.id.post_body);
            postUpvotes = itemView.findViewById(R.id.upvote_count);
            postDownvotes = itemView.findViewById(R.id.downvote_count);
            upButton = itemView.findViewById(R.id.upvote_button);
            downButton = itemView.findViewById(R.id.downvote_button);


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
                postDate.setText(DateUtils.convertUtcToFormattedTime(card.get("createdAt").toString()));
                postBody.setText(card.get("postBody").toString());
                //check value
                String down = card.get("downvotes").toString();
                if(down.equals("0")){
                    down = "";
                }
                String up = card.get("upvotes").toString();
                if(up.equals("0")){
                    up = "";
                }
                postDownvotes.setText(down);
                postUpvotes.setText(up);
                AtomicReference<AtomicBoolean> upVoted = new AtomicReference<>(new AtomicBoolean(false));
                AtomicReference<AtomicBoolean> downVoted = new AtomicReference<>(new AtomicBoolean(false));
                SharedPreferences sharedPreferences = context.getSharedPreferences("yAppPreferences", Context.MODE_PRIVATE);
                String uuid = sharedPreferences.getString("uuid", "");
                String pid = card.getString("pid");
                CompletableFuture<Boolean> upVoteFuture = checkVoted(true, true, uuid, pid, LOG_NAME);
                CompletableFuture<Boolean> downVoteFuture = checkVoted(false, true, uuid, pid, LOG_NAME);
                CompletableFuture.allOf(upVoteFuture, downVoteFuture).thenRun(() -> {
                    // Get results from the completed futures
                    upVoted.set(new AtomicBoolean(false));

                    upVoteFuture.thenAccept(result -> {
                        upVoted.get().set(result); // Update AtomicBoolean value
                        Log.i("Vote", "Upvoted: " + upVoted.get().get());
                    }).exceptionally(e -> {
                        Log.e("Vote", "Error while checking vote status: " + e.getMessage(), e);
                        return null;
                    });


                    downVoted.set(new AtomicBoolean(false));

                    downVoteFuture.thenAccept(result -> {
                        downVoted.get().set(result); // Update AtomicBoolean value
                        Log.i("Vote", "downVoted: " + downVoted.get().get());
                    }).exceptionally(e -> {
                        Log.e("Vote", "Error while checking vote status: " + e.getMessage(), e);
                        return null;
                    });

                    Log.i(LOG_NAME, "upVoted: " + upVoted);
                    Log.i(LOG_NAME, "downVoted: " + downVoted);

                    // Update UI on the main thread
                    ((Activity)context).runOnUiThread(() -> {
                        if (upVoted.get().get()) {
                            upButton.setBackgroundResource(R.drawable.ic_up_activated);
                        } else {
                            upButton.setBackgroundResource(R.drawable.ic_up);
                        }

                        if (downVoted.get().get()) {
                            downButton.setBackgroundResource(R.drawable.ic_down_activated);
                        } else {
                            downButton.setBackgroundResource(R.drawable.ic_down);
                        }

                    });
                }).exceptionally(e -> {
                    Log.e(LOG_NAME, "Error during vote checks: " + e.getMessage(), e);
                    return null;
                });


                upButton.setOnClickListener(v -> {
                    onVote(true, true, uuid, pid, upButton, upVoted.get(), "PostAdapter", upVoted.get(), downVoted.get());
                    if (postCardInteractions != null) {
                        postCardInteractions.refreshUI();
                    }
                    }
                );
                downButton.setOnClickListener(v -> {
                    onVote(true, false, uuid, pid, downButton, downVoted.get(), "PostAdapter", upVoted.get(), downVoted.get());
                    if (postCardInteractions != null) {
                        postCardInteractions.refreshUI();
                    }
                });
            }
            catch (JSONException jsonException)
            {
                Log.e(LOG_NAME, "Error parsing JSON", jsonException);
            } catch (UnsupportedEncodingException e) {
                Log.e(LOG_NAME, "Error encoding", e);
            }

        }

        public void setUpvoted(boolean value){

            if(value) {
                upButton.setBackgroundResource(R.drawable.ic_up_activated);
            }
            else{
                upButton.setBackgroundResource(R.drawable.ic_up);
            }
        }

        public void setDownvoted(boolean value){
            if(value) {
               downButton.setBackgroundResource(R.drawable.ic_down_activated);
            }
            else{
                downButton.setBackgroundResource(R.drawable.ic_down);
            }

        }

        @Override
        public void onVote(boolean isPost, boolean voteType, String uuid, String pid, ImageButton button, AtomicBoolean voteStatus, String logName, AtomicBoolean upVoted, AtomicBoolean downVoted) {
            addVotes(isPost, voteType, uuid, pid, logName, upVoted.get(), downVoted.get())
                    .thenAccept(success -> {
                        if (success) {
                            try {
                                checkVoted(voteType, isPost, uuid, pid, logName)
                                        .thenAccept(result -> {
                                            voteStatus.set(result);

                                        })
                                        .exceptionally(e -> {
                                            Log.e(logName, "Error while checking vote status: " + e.getMessage(), e);
                                            return null;
                                        });
                            } catch (UnsupportedEncodingException e) {
                                throw new RuntimeException(e);
                            }
                        }
                    })
                    .exceptionally(error -> {
                        Log.e(logName, "Vote operation failed: " + error.getMessage(), error);
                        return null;
                    });
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
}

package com.example.yappmobile.Votes;

import static com.example.yappmobile.Votes.Votes.addVotes;
import static com.example.yappmobile.Votes.Votes.checkVoted;

import android.util.Log;
import android.widget.ImageButton;

import java.io.UnsupportedEncodingException;
import java.util.concurrent.atomic.AtomicBoolean;

public interface IVoteHandler {
    default void onVote(boolean isPost, boolean voteType, String uuid, String pid, ImageButton button, AtomicBoolean voteStatus, String logName, AtomicBoolean upvote, AtomicBoolean downvote){
        addVotes(isPost, voteType, uuid, pid, logName, upvote.get(), downvote.get())
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

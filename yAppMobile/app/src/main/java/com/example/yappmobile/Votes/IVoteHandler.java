package com.example.yappmobile.Votes;

import android.widget.ImageButton;

import java.util.concurrent.atomic.AtomicBoolean;

public interface IVoteHandler {
    void onVote(boolean isPost, boolean voteType, String uuid, String pid, ImageButton button, AtomicBoolean voteStatus, String logName, AtomicBoolean upvote, AtomicBoolean downvote);
}

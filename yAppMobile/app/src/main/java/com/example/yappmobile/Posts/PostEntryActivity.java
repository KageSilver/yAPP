package com.example.yappmobile.Posts;

import static com.example.yappmobile.Votes.Votes.addVotes;
import static com.example.yappmobile.Votes.Votes.checkVoted;

import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.PopupMenu;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.Comments.CommentsBottomSheet;
import com.example.yappmobile.R;
import com.example.yappmobile.Utils.DateUtils;
import com.example.yappmobile.Votes.IVoteHandler;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.UnsupportedEncodingException;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.atomic.AtomicBoolean;


public class PostEntryActivity extends AppCompatActivity implements IVoteHandler {
    private final static String LOG_NAME = "POST_ENTRY";
    private String _pid, _uid, _title, _createdAt, _content, _updatedAt, _uuid, _upvotes, _downvotes;
    private TextView _postTitle, _postBody, _postDate, _postUpvodates, _postDownvotes;
    private CardListHelper _postListHelper;
    private ImageButton _upvotesButton, _downvotesButton, _replyButton, _backButton;
    private ImageView _moreOptions;
    private JSONObject _currentPost;
    private AtomicBoolean _upVoted, _downVoted;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_post_entry);
        _postListHelper = new CardListHelper(this);
        _postTitle = findViewById(R.id.post_title);
        _postBody = findViewById(R.id.post_body);
        _postDate = findViewById(R.id.post_date);
        _postDownvotes = findViewById(R.id.downvoteText);
        _postUpvodates = findViewById(R.id.upvoteText);
        _moreOptions = findViewById(R.id.menuButton);
        // Back button code
        _backButton = findViewById(R.id.backButton);
        _upvotesButton = findViewById(R.id.upvoteButton);
        _downvotesButton = findViewById(R.id.downvoteButton);


        // Setup content view to display post content
        String jsonString = getIntent().getStringExtra("currentPost");
        try {
            _currentPost = new JSONObject(jsonString);
            updateUI();


        } catch (JSONException e) {
            Toast.makeText(getApplicationContext(), "Error! failed to read the post", Toast.LENGTH_SHORT).show();
            Log.e(LOG_NAME, "Error of parsing the intent extra: " + e.getMessage());
        }


        //get current user
        _replyButton = findViewById(R.id.replyButton);

        SharedPreferences sharedPreferences = getSharedPreferences("yAppPreferences", Context.MODE_PRIVATE);
        _uuid = sharedPreferences.getString("uuid", "");
        if (_uid.equals(_uuid)) {
            _moreOptions.setVisibility(View.VISIBLE);

        } else {
            _moreOptions.setVisibility(View.INVISIBLE);
        }
        _replyButton.setOnClickListener(v -> {
            CommentsBottomSheet bottomSheet = CommentsBottomSheet.newInstance(_pid, _uid, _uuid, this);
            bottomSheet.show(getSupportFragmentManager(), bottomSheet.getTag());
        });


        try {
            // Start both `checkVoted` calls in parallel
            CompletableFuture<Boolean> upVoteFuture = checkVoted(true, true, _uuid, _pid, LOG_NAME);
            CompletableFuture<Boolean> downVoteFuture = checkVoted(false, true, _uuid, _pid, LOG_NAME);

            // Combine both futures to wait for completion
            CompletableFuture.allOf(upVoteFuture, downVoteFuture).thenRun(() -> {
                // Get results from the completed futures
                _upVoted = new AtomicBoolean(false);

                upVoteFuture.thenAccept(result -> {
                    _upVoted.set(result); // Update AtomicBoolean value
                    Log.i("Vote", "Upvoted: " + _upVoted.get());
                }).exceptionally(e -> {
                    Log.e("Vote", "Error while checking vote status: " + e.getMessage(), e);
                    return null;
                });


                _downVoted = new AtomicBoolean(false);

                downVoteFuture.thenAccept(result -> {
                    _downVoted.set(result); // Update AtomicBoolean value
                    Log.i("Vote", "downVoted: " + _downVoted.get());
                }).exceptionally(e -> {
                    Log.e("Vote", "Error while checking vote status: " + e.getMessage(), e);
                    return null;
                });

                Log.i(LOG_NAME, "upVoted: " + _upVoted);
                Log.i(LOG_NAME, "downVoted: " + _downVoted);

                // Update UI on the main thread
                runOnUiThread(() -> {
                    if (_upVoted.get()) {
                        _upvotesButton.setBackgroundResource(R.drawable.ic_up_activated);
                    } else {
                        _upvotesButton.setBackgroundResource(R.drawable.ic_up);
                    }

                    if (_downVoted.get()) {
                        _downvotesButton.setBackgroundResource(R.drawable.ic_down_activated);
                    } else {
                        _downvotesButton.setBackgroundResource(R.drawable.ic_down);
                    }
                    _upvotesButton.setOnClickListener(new View.OnClickListener() {

                        @Override
                        public void onClick(View v) {
                            onVote(true, true, _uuid, _pid, _upvotesButton, _upVoted, LOG_NAME,_upVoted,_downVoted);
                        }
                    });

                    _downvotesButton.setOnClickListener(new View.OnClickListener() {

                        @Override
                        public void onClick(View v) {
                            onVote(true, false,  _uuid, _pid,_downvotesButton, _downVoted, LOG_NAME,_upVoted,_downVoted);

                        }
                    });
                });
            }).exceptionally(e -> {
                Log.e(LOG_NAME, "Error during vote checks: " + e.getMessage(), e);
                return null;
            });

        } catch (UnsupportedEncodingException e) {
            Log.e(LOG_NAME, "Error on check vote: " + e.getMessage());
        }


        _backButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                getOnBackPressedDispatcher().onBackPressed();
            }
        });

        _moreOptions.setOnClickListener(view -> {
            // Create a PopupMenu
            PopupMenu popup = new PopupMenu(PostEntryActivity.this, view);

            // Inflate the menu
            popup.getMenuInflater().inflate(R.menu.menu_popup, popup.getMenu());

            // Handle menu item clicks
            popup.setOnMenuItemClickListener(item -> {
                if (item.getItemId() == R.id.action_edit) {
                    // Handle Edit action
                    Intent intent = new Intent(this, EditPostEntryActivity.class);
                    intent.putExtra("post", _currentPost.toString());
                    startActivity(intent);
                    finish();
                    return true;
                } else if (item.getItemId() == R.id.action_delete) {
                    // Handle Delete action
                    showConfirmationDialog();
                    return true;
                }
                return false;
            });

            // Show the popup menu
            popup.show();
        });

        //set event listener
    }




    public void getPost() {
        String path = "/api/posts/getPostById?pid=" + _pid;
        Log.i(LOG_NAME, "Constructed API path: " + path);

        RestOptions options = RestOptions.builder()
                .addPath(path)
                .addHeader("Content-Type", "application/json")
                .build();

        // Perform the asynchronous API request
        Amplify.API.get(options, response -> {
            runOnUiThread(() -> {
                try {
                    // Log raw response for debugging
                    String rawResponse = response.getData().asString();
                    Log.i(LOG_NAME, "Raw Response: " + rawResponse);

                    // Parse JSON response
                    if (rawResponse.startsWith("{") && rawResponse.endsWith("}")) {
                        _currentPost = new JSONObject(rawResponse);
                        updateUI();

                    } else {
                        throw new JSONException("Invalid JSON response");
                    }
                } catch (JSONException e) {
                    Log.e(LOG_NAME, "JSON Parsing Error: " + e.getMessage(), e);
                    Toast.makeText(getApplicationContext(), "Error parsing post data!", Toast.LENGTH_SHORT).show();
                }
            });
        }, error -> {
            Log.e(LOG_NAME, "Fetch post error: " + error.getMessage(), error);
            runOnUiThread(() -> {
                Toast.makeText(getApplicationContext(), "Error fetching post!", Toast.LENGTH_SHORT).show();
            });
        });
    }

    private void updateUI() throws JSONException {
        // Extract data from JSON
        _pid = _currentPost.getString("pid");
        _uid = _currentPost.getString("uid");
        _title = _currentPost.getString("postTitle");
        _content = _currentPost.getString("postBody");
        _createdAt = _currentPost.getString("createdAt");
        _upvotes = _currentPost.getString("upvotes");
        _downvotes = _currentPost.getString("downvotes");

        // Update UI
        _postTitle.setText(_title);
        _postBody.setText(_content);
        _postDate.setText(DateUtils.convertUtcToFormattedTime(_createdAt));
        _postUpvodates.setText(_upvotes.equals("0") ? "" : _upvotes);
        _postDownvotes.setText(_downvotes.equals("0") ? "" : _downvotes);
    }


    public void showConfirmationDialog() {
        // Create the AlertDialog builder
        AlertDialog.Builder builder = new AlertDialog.Builder(this);

        // Set the title and message of the dialog
        builder.setTitle("Woah there")
                .setMessage("Are you sure you want to delete this post?");

        builder.setPositiveButton("Confirm", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                // create delete
                RestOptions options = RestOptions.builder()
                        .addPath("/api/posts/deletePost?pid=" + _pid)
                        .addHeader("Content-Type", "application/json")
                        .build();

                Amplify.API.delete(options,
                        response -> {
                            Log.i(LOG_NAME, "DELETE succeeded: " + response);

                            Intent intent = new Intent();
                            intent.putExtra("delete", _pid);
                            setResult(PostEntryActivity.RESULT_OK, intent);

                            // Show a Toast message on the main thread
                            runOnUiThread(() -> {
                                Toast.makeText(getApplicationContext(), "Delete successful!", Toast.LENGTH_SHORT).show();

                            });
                            finish();
                        },
                        error -> {
                            Log.e(LOG_NAME, "DELETE failed.", error);

                            // Optionally, show a Toast for failure as well
                            runOnUiThread(() -> {
                                Toast.makeText(getApplicationContext(), "Delete failed!", Toast.LENGTH_SHORT).show();
                            });
                        }
                );

            }
        });

        // Set the negative button (e.g., "No" or "Cancel")
        builder.setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                // Dismiss the dialog when the user cancels
                dialog.dismiss();
            }
        });

        // Create and show the dialog
        AlertDialog dialog = builder.create();
        dialog.show();
    }

    @Override
    public void onVote(boolean isPost, boolean voteType, String uuid, String pid, ImageButton button, AtomicBoolean voteStatus,String logName,AtomicBoolean upVoted, AtomicBoolean downVoted) {
        _downvotesButton.setEnabled(false);
        _upvotesButton.setEnabled(false);
        addVotes(isPost, voteType, uuid, pid, logName, upVoted.get(), downVoted.get())
                .thenAccept(success -> {
                    if (success) {
                        try {
                            CompletableFuture<Boolean> voteFuture = checkVoted(voteType, isPost, uuid, pid, logName);
                            voteFuture.thenAccept(result -> {
                                voteStatus.set(result);
                                runOnUiThread(() -> {
                                    if (voteStatus.get()) {
                                        button.setBackgroundResource(voteType ? R.drawable.ic_up_activated : R.drawable.ic_down_activated);
                                    } else {
                                        button.setBackgroundResource(voteType ? R.drawable.ic_up : R.drawable.ic_down);
                                    }
                                    _downvotesButton.setEnabled(true);
                                    _upvotesButton.setEnabled(true);
                                });
                            }).exceptionally(e -> {
                                runOnUiThread(() -> {
                                    _downvotesButton.setEnabled(true);
                                    _upvotesButton.setEnabled(true);
                                });
                                Log.e(logName, "Error while checking vote status: " + e.getMessage(), e);
                                return null;
                            });
                        } catch (UnsupportedEncodingException e) {
                            runOnUiThread(() -> {
                                _downvotesButton.setEnabled(true);
                                _upvotesButton.setEnabled(true);
                            });
                            Log.e(logName, "Error creating vote request: " + e.getMessage(), e);
                        }
                    }

                    getPost();
                })
                .exceptionally(error -> {
                    runOnUiThread(() -> {
                        _downvotesButton.setEnabled(true);
                        _upvotesButton.setEnabled(true);
                    });
                    Log.e(logName, "Vote operation failed: " + error.getMessage(), error);
                    return null;
                });
    }
}
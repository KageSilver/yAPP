package com.example.yappmobile;

import android.content.DialogInterface;
import android.content.Intent;
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

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.CompletableFuture;

public class PostEntryActivity extends AppCompatActivity
{
    private String _pid;
    private String _uid;

    private String _uuid;
    private  String _username;
    private TextView postTitle, postBody;
    private CardListHelper postListHelper;

    private ImageView _moreOptions;

    private final static  String LOG_NAME ="POST_ENTRY";

    private JSONObject _currentPost;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_post_entry);
        //get current user
        ImageButton replyButton = findViewById(R.id.replyButton);
        Amplify.Auth.getCurrentUser(result -> {
            Log.i(LOG_NAME, "Continuing as " + result.getUsername() + "...");
            _uuid = result.getUserId();
            _username = result.getUsername();
            runOnUiThread(() -> {
                replyButton.setOnClickListener(v -> {
                    CommentsBottomSheet bottomSheet = CommentsBottomSheet.newInstance(_pid, _uid, _uuid);
                    bottomSheet.show(getSupportFragmentManager(), bottomSheet.getTag());
                });
            });
        }, error -> {
            Log.i(LOG_NAME, "There is no user signed in!!");
        });

        // Back button code
       ImageButton backButton = findViewById(R.id.backButton);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                getOnBackPressedDispatcher().onBackPressed();
            }
        });

        postListHelper = new CardListHelper(this);

        // Setup content view to display post content
        _pid = getIntent().getStringExtra("pid");
        _uid = getIntent().getStringExtra("uid");
        postTitle = findViewById(R.id.post_title);
        postBody = findViewById(R.id.post_body);

        String getPostAPI = "/api/posts/getPostById?pid=" + _pid;
        loadPost(getPostAPI);

        //set up edit and delete

        _moreOptions = findViewById(R.id.menuButton);
        // check if it is the current user
        if(_uid.equals(_uuid)){
            _moreOptions.setVisibility(View.VISIBLE);

        }else{
            _moreOptions.setVisibility(View.INVISIBLE);
        }

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
                    intent.putExtra("post",_currentPost.toString());
                    startActivity(intent);
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



    }

    private void loadPost(String apiUrl)
    {
        // Fetch post (only singular, should return the same thing based on the url passed
        CompletableFuture<String> future = postListHelper.getItemsFromAPI(apiUrl);
        future.thenAccept(jsonData -> {
            try
            {
                // Handle API response
                Log.d(LOG_NAME, "Received data: " + jsonData);

                // Get the post's title and body from the response JSON
                _currentPost = new JSONObject(jsonData);
                String title = _currentPost.get("postTitle").toString();
                String body = _currentPost.get("postBody").toString();
                String uid = _currentPost.get("uid").toString();
                runOnUiThread(() -> {
                    // Ensure title and body are not null before updating
                    if (postTitle != null) {
                        postTitle.setText(title);
                    }
                    if (postBody != null) {
                        postBody.setText(body);
                    }
                    if (uid.equals(_uuid)){
                        _moreOptions.setVisibility(View.VISIBLE);
                    }else{
                        _moreOptions.setVisibility(View.INVISIBLE);
                    }
                });
            }
            catch (JSONException jsonException)
            {
                Log.e("JSON", "Error parsing JSON", jsonException);
            }
        }).exceptionally(throwable -> {
            Log.e("API", "Error fetching data", throwable);
            return null;
        });
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
                        .addPath("/api/posts/deletePost?pid="+_pid)
                        .addHeader("Content-Type","application/json")
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
                                finish();
                            });
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

}
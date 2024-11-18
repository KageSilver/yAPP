package com.example.yappmobile;

import android.annotation.SuppressLint;
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
import com.example.yappmobile.Utils.DateUtils;

import org.json.JSONException;
import org.json.JSONObject;

public class PostEntryActivity extends AppCompatActivity
{
    private String _pid, _uid, _title, _createdAt, _content, _updatedAt, _uuid, _upvotes, _downvotes;
    private TextView _postTitle, _postBody, _postDate, _postUpvodates, _postDownvotes;
    private CardListHelper _postListHelper;

    private ImageView _moreOptions;

    private final static  String LOG_NAME ="POST_ENTRY";

    private JSONObject _currentPost;

    @SuppressLint("MissingInflatedId")
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
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
        ImageButton backButton = findViewById(R.id.backButton);

        // Setup content view to display post content
        String jsonString = getIntent().getStringExtra("currentPost");
        try {
            _currentPost = new JSONObject(jsonString);
            _pid = _currentPost.getString("pid");
            _uid = _currentPost.getString("uid");
            _title =  _currentPost.getString("postTitle");
            _content = _currentPost.getString("postBody");
            _createdAt =  _currentPost.getString("createdAt");
            _upvotes = _currentPost.getString("upvotes");
            _downvotes = _currentPost.getString("downvotes");


            _postTitle.setText(_title);
            _postBody.setText(_content);
            _postDate.setText(DateUtils.convertUtcToFormattedTime(_createdAt));
            if(_upvotes.equals("0")){
                _upvotes = "";
            }
            if(_downvotes.equals("0")){
                _downvotes = "";
            }
            _postUpvodates.setText(_upvotes);
            _postDownvotes.setText(_downvotes);


        } catch (JSONException e) {
            Toast.makeText(getApplicationContext(),"Error! failed to read the post", Toast.LENGTH_SHORT).show();
            Log.e(LOG_NAME,"Error of parsing the intent extra: "+e.getMessage());
        }

        //get current user
        ImageButton replyButton = findViewById(R.id.replyButton);
        Amplify.Auth.getCurrentUser(result -> {
            Log.i(LOG_NAME, "Continuing as " + result.getUsername() + "...");
            _uuid = result.getUserId();
            Log.i(LOG_NAME,"uid: "+_uid);
            Log.i(LOG_NAME,"uuid"+_uuid);

            runOnUiThread(() -> {
                if(_uid.equals(_uuid)){
                    _moreOptions.setVisibility(View.VISIBLE);

                }else{
                    _moreOptions.setVisibility(View.INVISIBLE);
                }

                replyButton.setOnClickListener(v -> {
                    CommentsBottomSheet bottomSheet = CommentsBottomSheet.newInstance(_pid, _uid, _uuid, this);
                    bottomSheet.show(getSupportFragmentManager(), bottomSheet.getTag());
                });
            });
        }, error -> {
            Log.i(LOG_NAME, "There is no user signed in!!");
        });


        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
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
                    intent.putExtra("post",_currentPost.toString());
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

}
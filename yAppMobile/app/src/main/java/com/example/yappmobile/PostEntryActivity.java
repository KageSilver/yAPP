package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.PopupMenu;

import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.Comments.CommentsBottomSheet;
import com.example.yappmobile.NaviBarDestinations.PublicPostsActivity;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.CompletableFuture;

public class PostEntryActivity extends AppCompatActivity
{
    private String _pid;
    private TextView postTitle, postBody;
    private CardListHelper postListHelper;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_post_entry);

        // Back button code
       ImageButton backButton = findViewById(R.id.backButton);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(PostEntryActivity.this, PublicPostsActivity.class);
                startActivity(intent);
            }
        });

        postListHelper = new CardListHelper(this);

        // Setup content view to display post content
        _pid = getIntent().getStringExtra("pid");
        postTitle = findViewById(R.id.post_title);
        postBody = findViewById(R.id.post_body);

        String getPostAPI = "/api/posts/getPostById?pid=" + _pid;
        loadPost(getPostAPI);

        //set up edit and delete
        ImageView moreOptions = findViewById(R.id.menuButton);
        moreOptions.setOnClickListener(view -> {
            // Create a PopupMenu
            PopupMenu popup = new PopupMenu(PostEntryActivity.this, view);

            // Inflate the menu
            popup.getMenuInflater().inflate(R.menu.menu_popup, popup.getMenu());

            // Handle menu item clicks
            popup.setOnMenuItemClickListener(item -> {
                switch (item.getItemId()) {
//                    case R.id.action_edit:
//                        // Handle Edit action
//                        return true;
//                    case R.id.action_delete:
//                        // Handle Delete action
//                        return true;
                    default:
                        return false;
                }
            });

            // Show the popup menu
            popup.show();
        });

        // Find the reply button
        // Find the reply button in the layout
        ImageButton replyButton = findViewById(R.id.replyButton);

        // Set a click listener on the reply button to open the BottomSheetDialogFragment
        replyButton.setOnClickListener(v -> {
            // Create and show the BottomSheetDialogFragment
            CommentsBottomSheet bottomSheet = CommentsBottomSheet.newInstance(_pid);
            bottomSheet.show(getSupportFragmentManager(), bottomSheet.getTag());
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
                Log.d("API", "Received data: " + jsonData);

                // Get the post's title and body from the response JSON
                JSONObject jsonObject = new JSONObject(jsonData);
                String title = jsonObject.get("postTitle").toString();
                String body = jsonObject.get("postBody").toString();

                // Update fields:
                runOnUiThread(() -> {
                    postTitle.setText(title);
                    postBody.setText(body);
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

}
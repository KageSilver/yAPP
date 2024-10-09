package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

import androidx.appcompat.app.AppCompatActivity;

public class MyPostsActivity extends AppCompatActivity {

    @Override
    public void onCreate(Bundle savedInstanceState){
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_my_posts);

        // Post creation button code
        Button newPost = findViewById(R.id.new_post_button);
        newPost.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(MyPostsActivity.this, CreatePostActivity.class);
                startActivity(intent);
            }
        });

        // Global posts button code view
        Button globalPosts = findViewById(R.id.global_posts_button);
        globalPosts.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(MyPostsActivity.this, MainActivity.class);
                startActivity(intent);
            }
        });

    }//end onCreate
}

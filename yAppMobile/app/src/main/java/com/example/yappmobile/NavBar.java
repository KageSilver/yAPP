package com.example.yappmobile;

import android.content.Intent;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;

import com.example.yappmobile.NaviBarDestinations.CreatePostActivity;
import com.example.yappmobile.NaviBarDestinations.ProfileActivity;
import com.example.yappmobile.NaviBarDestinations.PublicPostsActivity;
import com.google.android.material.navigation.NavigationBarView;

public class NavBar
{
    // NavBar menu items which represent the options of possible destinations
    private static final int PROFILE = R.id.profile;
    private static final int HOME = R.id.public_posts;
    private static final int CREATE_POST = R.id.create_post;

    public static void establishNavBar(AppCompatActivity container, String menuItem)
    {
        // Set the activity to be full screen
        View decorView = container.getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        NavigationBarView bottomNav = container.findViewById(R.id.bottom_navigation);

        // Set the default chosen menu item of the navbar
        // Reminder: CreatePost page does not initialize a navbar
        switch (menuItem)
        {
            case "HOME":
                bottomNav.setSelectedItemId(HOME);
                break;
            case "PROFILE":
                bottomNav.setSelectedItemId(PROFILE);
                break;
        }

        bottomNav.setOnItemSelectedListener(new NavigationBarView.OnItemSelectedListener()
        {
            @Override
            public boolean onNavigationItemSelected(@NonNull MenuItem item)
            {
                int itemId = item.getItemId();
                Intent intent;
                if (itemId == PROFILE)
                {
                    // Reroute to profile page
                    intent = new Intent(container, ProfileActivity.class);
                    container.startActivity(intent);
                }
                else if (itemId == HOME)
                {
                    // Reroute to public posts
                    intent = new Intent(container, PublicPostsActivity.class);
                    container.startActivity(intent);
                }
                else if (itemId == CREATE_POST)
                {
                    // Reroute to create post page
                    intent = new Intent(container, CreatePostActivity.class);
                    container.startActivity(intent);
                }
                else
                {
                    Log.i("Rerouting", "OOF! Sorry, we haven't done this yet");
                }
                return true;
            }
        });
    }
}

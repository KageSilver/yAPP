package com.example.yappmobile;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageButton;

import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.tabs.TabLayout;

public class ProfileActivity extends AppCompatActivity
{
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        setContentView(R.layout.activity_profile);

        ImageButton viewRequests = findViewById(R.id.profile_view_requests_button);
        viewRequests.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(ProfileActivity.this,
                        MyRequestsActivity.class);
                startActivity(intent);
            }
        });

        TabLayout tabLayout = findViewById(R.id.profile_tabs);
        tabLayout.addOnTabSelectedListener(new TabLayout.OnTabSelectedListener()
        {
            @Override
            public void onTabSelected(TabLayout.Tab tab)
            {
                int chosenTabId = tab.getId();
                Log.d("OnClick", String.format("Woah! You clicked me: %d", chosenTabId));
            }

            @Override
            public void onTabUnselected(TabLayout.Tab tab)
            {
                // idk what to put here
            }

            @Override
            public void onTabReselected(TabLayout.Tab tab)
            {
                // idk what to put here
            }
        });
    }
}

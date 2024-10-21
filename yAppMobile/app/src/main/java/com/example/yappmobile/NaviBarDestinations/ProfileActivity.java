package com.example.yappmobile.NaviBarDestinations;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ImageButton;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.viewpager2.widget.ViewPager2;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.MyRequestsActivity;
import com.example.yappmobile.NavBar;
import com.example.yappmobile.R;
import com.example.yappmobile.ProfileTabs.ViewPagerAdapter;
import com.google.android.material.tabs.TabLayout;

public class ProfileActivity extends AppCompatActivity
{
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);

        NavBar.establishNavBar(this, "PROFILE");

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

        TextView usernameText = findViewById(R.id.profile_username_text);
        Amplify.Auth.getCurrentUser(
                result ->
                {
                    usernameText.setText(result.getUsername());
                },
                error ->
                {
                    usernameText.setText("????");
                }
        );
        String username = usernameText.getText().toString();

        ViewPager2 viewPager2 = findViewById(R.id.view_pager_2);
        ViewPagerAdapter viewPagerAdapter = new ViewPagerAdapter(this);
        TabLayout tabLayout = findViewById(R.id.profile_tabs);

        viewPager2.setAdapter(viewPagerAdapter);

        tabLayout.addOnTabSelectedListener(new TabLayout.OnTabSelectedListener()
        {
            @Override
            public void onTabSelected(TabLayout.Tab tab)
            {
                viewPager2.setCurrentItem(tab.getPosition());
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
        viewPager2.registerOnPageChangeCallback(new ViewPager2.OnPageChangeCallback()
        {
            @Override
            public void onPageSelected(int position)
            {
                super.onPageSelected(position);
                tabLayout.getTabAt(position).select();
            }
        });
    }
}

package com.example.yappmobile.NaviBarDestinations;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageButton;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.viewpager2.widget.ViewPager2;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.NavBar;
import com.example.yappmobile.ProfileManagement.MyRequestsActivity;
import com.example.yappmobile.ProfileManagement.ProfileTabs.ViewPagerAdapter;
import com.example.yappmobile.ProfileManagement.ResetPasswordActivity;
import com.example.yappmobile.R;
import com.google.android.material.tabs.TabLayout;

public class ProfileActivity extends AppCompatActivity
{
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profile);
        NavBar.establishNavBar(this, "PROFILE");

        // Set up view requests button code
        ImageButton viewRequests = findViewById(R.id.profile_view_requests_button);
        viewRequests.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(ProfileActivity.this, MyRequestsActivity.class);
                startActivity(intent);
            }
        });

        // Set up profile settings button code
        ImageButton viewSettings = findViewById(R.id.profile_settings_button);
        viewSettings.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(ProfileActivity.this, ResetPasswordActivity.class);
                startActivity(intent);
            }
        });

        TextView usernameText = findViewById(R.id.profile_username_text);
        TextView uuidText = findViewById(R.id.profile_uuid_text);
        Amplify.Auth.getCurrentUser(result -> {
            usernameText.setText(result.getUsername());
            uuidText.setText(result.getUserId());
        }, error -> {
            Log.e("Auth", "Error occurred when getting current user. Redirecting to authenticator");
            Intent intent = new Intent(ProfileActivity.this, AuthenticatorActivity.class);
            startActivity(intent);
        });

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
            }

            @Override
            public void onTabReselected(TabLayout.Tab tab)
            {
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

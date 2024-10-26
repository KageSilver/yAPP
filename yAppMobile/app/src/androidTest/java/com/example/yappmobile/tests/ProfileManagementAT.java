package com.example.yappmobile.tests;


import androidx.test.ext.junit.runners.AndroidJUnit4;
import androidx.test.filters.LargeTest;
import androidx.test.rule.ActivityTestRule;

import com.example.yappmobile.NaviBarDestinations.PublicPostsActivity;

import org.junit.Rule;
import org.junit.runner.RunWith;

@RunWith(AndroidJUnit4.class)
@LargeTest
public class ProfileManagementAT {
    @Rule
    public ActivityTestRule<PublicPostsActivity> activityRule = new ActivityTestRule<>(PublicPostsActivity.class);

    //@Test
    public void accountCreation()
    {
        // TODO
    }

    //@Test
    public void accountLogin()
    {
        // TODO
    }

    //@Test
    public void makeFriends()
    {
        // TODO
    }

    //@Test
    public void editAccountDetails()
    {
        // TODO
    }
}

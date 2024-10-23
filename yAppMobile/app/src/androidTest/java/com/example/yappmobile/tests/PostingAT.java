package com.example.yappmobile.tests;

import static androidx.test.espresso.Espresso.onView;
import static androidx.test.espresso.action.ViewActions.click;
import static androidx.test.espresso.action.ViewActions.typeText;
import static androidx.test.espresso.matcher.ViewMatchers.withId;

import androidx.test.ext.junit.runners.AndroidJUnit4;
import androidx.test.filters.LargeTest;
import androidx.test.rule.ActivityTestRule;

import com.example.yappmobile.NaviBarDestinations.PublicPostsActivity;
import com.example.yappmobile.R;

import org.junit.Rule;
import org.junit.Test;
import org.junit.runner.RunWith;

@RunWith(AndroidJUnit4.class)
@LargeTest
public class PostingAT {
    @Rule
    public ActivityTestRule<PublicPostsActivity> activityRule = new ActivityTestRule<>(PublicPostsActivity.class);

    @Test
    public void makePublicPost()
    {
        // navigate to the Create Post screen
        onView(withId(R.id.new_post_button)).perform(click());

        // enter post title and body
        onView(withId(R.id.post_title)).perform(typeText("This is the post title"));
        onView(withId(R.id.post_content)).perform(typeText("this is the post body"));

        // submit the post
        onView(withId(R.id.create_button)).perform(click());

        // TODO: check if post has been made under My Posts page
        // TODO: add teardown
    }

    //@Test
    public void seeMyPosts()
    {
        // TODO
    }

    //@Test
    public void seePublicPosts()
    {
        // TODO
    }
}

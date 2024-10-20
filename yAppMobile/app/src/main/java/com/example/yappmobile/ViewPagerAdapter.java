package com.example.yappmobile;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentActivity;
import androidx.viewpager2.adapter.FragmentStateAdapter;

import com.example.yappmobile.TabItemContent.MyAwardsFragment;
import com.example.yappmobile.TabItemContent.MyFriendsFragment;
import com.example.yappmobile.TabItemContent.MyPostsFragment;

public class ViewPagerAdapter extends FragmentStateAdapter
{
    public ViewPagerAdapter(@NonNull FragmentActivity fragmentActivity)
    {
        super(fragmentActivity);
    }

    @NonNull
    @Override
    public Fragment createFragment(int position)
    {
        switch (position)
        {
            case 1: return new MyFriendsFragment();
            case 2: return new MyAwardsFragment();
            default: return new MyPostsFragment();
        }
    }

    @Override
    public int getItemCount()
    {
        return 3;
    }
}

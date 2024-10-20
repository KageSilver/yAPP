package com.example.yappmobile.TabItemContent;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.fragment.app.Fragment;

import com.example.yappmobile.R;

public class MyAwardsFragment extends Fragment
{
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        // TODO: modify this
        return inflater.inflate(R.layout.fragment_my_posts, container, false);
    }
}

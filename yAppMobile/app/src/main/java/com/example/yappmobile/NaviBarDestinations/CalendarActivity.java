package com.example.yappmobile.NaviBarDestinations;

import android.os.Bundle;

import androidx.appcompat.app.AppCompatActivity;

import com.example.yappmobile.NavBar;
import com.example.yappmobile.R;

public class CalendarActivity extends AppCompatActivity
{
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_calendar);
        NavBar.establishNavBar(this, "CALENDAR");
    }
}

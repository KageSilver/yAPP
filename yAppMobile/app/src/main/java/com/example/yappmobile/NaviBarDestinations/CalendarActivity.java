package com.example.yappmobile.NaviBarDestinations;

import android.os.Bundle;
import android.widget.CalendarView;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.example.yappmobile.NavBar;
import com.example.yappmobile.R;

import java.text.SimpleDateFormat;
import java.util.Calendar;

public class CalendarActivity extends AppCompatActivity
{
    private CalendarView calendar;
    private TextView selectedDate;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_calendar);
        NavBar.establishNavBar(this, "CALENDAR");

        calendar = findViewById(R.id.calendarView);
        selectedDate = findViewById(R.id.selectedDate);

        Calendar cal = Calendar.getInstance();
        // sets the calendar to the current date
        calendar.setDate(cal.getTimeInMillis());
        selectedDate.setText(formatDate(cal));

        calendar.setOnDateChangeListener(new CalendarView.OnDateChangeListener() {
            @Override
            public void onSelectedDayChange(CalendarView calendarView, int year, int month, int day) {
                // set calendar to the new date selected
                Calendar cal = Calendar.getInstance();
                cal.set(year, month, day);
                calendar.setDate(cal.getTimeInMillis());
                selectedDate.setText(formatDate(cal));
            }
        });
    }

    private String formatDate(Calendar cal)
    {
        StringBuilder date = new StringBuilder();

        SimpleDateFormat formatter = new SimpleDateFormat("MMM");
        date.append(formatter.format(cal.getTime()));
        date.append(" ");
        formatter = new SimpleDateFormat("dd");
        date.append(formatter.format(cal.getTime()));
        date.append(" ");
        formatter = new SimpleDateFormat("yyyy");
        date.append(formatter.format(cal.getTime()));

        return date.toString();
    }
}

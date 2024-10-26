package com.example.yappmobile.NaviBarDestinations;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.CalendarView;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.NavBar;
import com.example.yappmobile.R;

import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.concurrent.CompletableFuture;

public class CalendarActivity extends AppCompatActivity implements IListCardItemInteractions
{
    private CalendarView calendar;
    private TextView selectedDate;
    private CardListHelper diaryEntryHelper;
    private RecyclerView userDiaries;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_calendar);
        NavBar.establishNavBar(this, "CALENDAR");

        ProgressBar loadingSpinner = findViewById(R.id.indeterminateBar);
        diaryEntryHelper = new CardListHelper(this, loadingSpinner, "DIARY", this);

        // fetch all diary entries for current user
        getUserDiaries();

        calendar = findViewById(R.id.calendarView);
        selectedDate = findViewById(R.id.selectedDate);
        userDiaries = findViewById(R.id.user_diary_list);

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

                diaryEntryHelper.loadDiaries(getUserDiariesFromDate(cal), userDiaries);
            }
        });

        diaryEntryHelper.loadDiaries(getUserDiariesFromDate(cal), userDiaries);
    }

    private String getUserDiariesFromDate(Calendar cal)
    {
        return "";
    }

    private void getUserDiaries()
    {
        CompletableFuture<String> future = new CompletableFuture<>();

        Amplify.Auth.getCurrentUser(result -> {
            future.complete(result.getUserId());
        }, error -> {
            Log.e("Auth", "Error occurred when getting current user. Redirecting to authenticator");
            Intent intent = new Intent(CalendarActivity.this, AuthenticatorActivity.class);
            startActivity(intent);
        });

        future.thenAccept(uid -> {
            String apiUrl = "/api/posts/getPostsByUser?uid=" + uid + "&diaryEntry=true";
            diaryEntryHelper.getItemsFromAPI(apiUrl);
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

    @Override
    public void onItemClick(int position) {

    }
}

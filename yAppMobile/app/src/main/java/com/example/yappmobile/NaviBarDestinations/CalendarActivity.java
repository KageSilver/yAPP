package com.example.yappmobile.NaviBarDestinations;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.CalendarView;
import android.widget.ImageButton;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.activity.result.ActivityResult;
import androidx.activity.result.ActivityResultCallback;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AppCompatActivity;
import androidx.cardview.widget.CardView;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.AuthenticatorActivity;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.CardList.IListCardItemInteractions;
import com.example.yappmobile.NavBar;
import com.example.yappmobile.PostEntryActivity;
import com.example.yappmobile.R;

import org.json.JSONObject;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.concurrent.CompletableFuture;

public class CalendarActivity extends AppCompatActivity implements IListCardItemInteractions
{
    private CalendarView calendar;
    private CardView calendarCard;
    private TextView selectedDate;
    private CardListHelper diaryEntryHelper;
    private ImageButton collapseCalendar;
    private RecyclerView diaries;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_calendar);
        NavBar.establishNavBar(this, "CALENDAR");

        ProgressBar loadingSpinner = findViewById(R.id.indeterminate_bar);
        diaryEntryHelper = new CardListHelper(this, loadingSpinner, "DIARY", this);

        // sets up recycler view
        diaries = findViewById(R.id.diary_list);
        diaries.setLayoutManager(new LinearLayoutManager(this));

        // finds elements in the xml file that will be changed later
        calendar = findViewById(R.id.calendar_view);
        selectedDate = findViewById(R.id.selected_date);
        calendarCard = findViewById(R.id.calendar_card);
        collapseCalendar = findViewById(R.id.collapse_calendar);

        // gets current date and time
        Calendar cal = Calendar.getInstance();
        // sets the calendar to the current date
        calendar.setDate(cal.getTimeInMillis());
        selectedDate.setText(formatDisplayDate(cal));

        calendar.setOnDateChangeListener(new CalendarView.OnDateChangeListener() {
            @Override
            public void onSelectedDayChange(CalendarView calendarView, int year, int month, int day) {
                // set calendar to the new date selected
                Calendar cal = Calendar.getInstance();
                cal.set(year, month, day);
                calendar.setDate(cal.getTimeInMillis());
                selectedDate.setText(formatDisplayDate(cal));
                // loads diary entries for the new selected date from user and users friends
                getDiaries(cal);
            }
        });

        // sets up button to collapse and open calendar
        collapseCalendar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                // changes arrow image on button and changes visibility of calendar
                if(calendarCard.getVisibility() == View.VISIBLE)
                {
                    calendarCard.setVisibility(View.GONE);
                    collapseCalendar.setImageResource(R.drawable.baseline_keyboard_arrow_down_24);
                }
                else
                {
                    calendarCard.setVisibility(View.VISIBLE);
                    collapseCalendar.setImageResource(R.drawable.baseline_keyboard_arrow_up_24);
                }
            }
        });

        getDiaries(cal);
    }

    private void getDiaries(Calendar cal)
    {
        CompletableFuture<String> future = new CompletableFuture<>();

        // gets user information
        Amplify.Auth.getCurrentUser(result -> {
            future.complete(result.getUserId());
        }, error -> {
            Log.e("Auth", "Error occurred when getting current user. Redirecting to authenticator");
            Intent intent = new Intent(CalendarActivity.this, AuthenticatorActivity.class);
            startActivity(intent);
        });

        // gets diary entries from user for date selected
        String apiUrlUser = "/api/posts/getDiariesByUser?uid=" + uid + "&current=" + formatCompareDate(cal);
        // gets diary entries from users friends for date selected
        String apiUrlFriends = "/api/posts/getDiariesByFriends?uid=" + uid + "&current=" + formatCompareDate(cal);
        diaryEntryHelper.loadDiaries(apiUrlUser, apiUrlFriends, diaries);
    }

    private String formatDisplayDate(Calendar cal)
    {
        // formats date to display as Oct 26 2024
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

    private String formatCompareDate(Calendar cal)
    {
        // formats date to compare with values in the api response
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.FFF'Z'");
        return formatter.format(cal.getTime());
    }

    @Override
    public void onItemClick(int position)
    {
        // Switch activity to view an individual diary entry when a card is clicked
        Intent intent = new Intent(CalendarActivity.this, PostEntryActivity.class);
        String pid = diaryEntryHelper.getPID(position);
        String uid = diaryEntryHelper.getUID(position);
        intent.putExtra("pid", pid);
        intent.putExtra("uid",uid);
        activityLauncher.launch(intent);
    }

    ActivityResultLauncher<Intent> activityLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            new ActivityResultCallback<ActivityResult>() {
                @Override
                public void onActivityResult(ActivityResult result){
                    if (result.getResultCode() == RESULT_OK)
                    {
                        Intent intent = result.getData();
                        try
                        {
                            String deleted = intent.getStringExtra("delete");
                            diaryEntryHelper.removePost(deleted);
                        }
                        catch (Exception e)
                        {
                            Log.i("POST", "Post was not deleted");
                        }
                    }
                }
            });
}

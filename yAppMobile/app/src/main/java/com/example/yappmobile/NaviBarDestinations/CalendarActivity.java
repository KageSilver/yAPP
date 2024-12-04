package com.example.yappmobile.NaviBarDestinations;

import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.CalendarView;
import android.widget.ImageButton;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.cardview.widget.CardView;

import com.amplifyframework.auth.AuthUser;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.CardList.CardListHelper;
import com.example.yappmobile.NavBar;
import com.example.yappmobile.R;

import org.json.JSONObject;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import java.util.concurrent.CompletableFuture;

public class CalendarActivity extends AppCompatActivity
{
    private CalendarView calendar;
    private CardView calendarCard;
    private TextView selectedDate;
    private CardListHelper diaryEntryHelper;
    private ImageButton collapseCalendar;
    private List<JSONObject> friends, friendUsernames;
    private String username;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_calendar);
        NavBar.establishNavBar(this, "CALENDAR");
        friendUsernames = new ArrayList<>();

        diaryEntryHelper = new CardListHelper(this);

        getFriends();

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

                Bundle bundle = new Bundle();
                bundle.putString("friendUsernames", friendUsernames.toString());
                bundle.putString("selectedDate", formatCompareDate(cal));

                getSupportFragmentManager().beginTransaction()
                        .setReorderingAllowed(true)
                        .replace(R.id.fragment_container_view, DiaryEntriesFragment.class, bundle)
                        .commit();
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
    }
    
    @Override
    public void onRestart()
    {
        super.onRestart();
        finish();
        startActivity(getIntent());
    }

    private void createDiaryFragment()
    {
        Bundle bundle = new Bundle();
        bundle.putString("friendUsernames", friendUsernames.toString());
        bundle.putString("selectedDate", formatCompareDate(Calendar.getInstance()));

        getSupportFragmentManager().beginTransaction()
                .setReorderingAllowed(true)
                .add(R.id.fragment_container_view, DiaryEntriesFragment.class, bundle)
                .commit();
    }

    private void getFriends()
    {
        CompletableFuture<AuthUser> future = new CompletableFuture<>();
        Amplify.Auth.getCurrentUser(future::complete, error -> {
            Log.e("Auth", "Uh oh! THere's trouble getting the current user", error);
        });

        future.thenAccept(user -> {
            runOnUiThread(() -> {
                username = user.getUsername();
                String apiUrl = "/api/friends/getFriendsByStatus?userName=" + username + "&status=1";

                CompletableFuture<String> future2 = diaryEntryHelper.getItemsFromAPI(apiUrl);
                future2.thenAccept(jsonData ->
                {
                    // Convert API response into a list of CardItems
                    friends = diaryEntryHelper.handleData(jsonData);
                    // get uids of all friends
                    getFriendUIDs();
                }).exceptionally(throwable ->
                {
                    Log.e("API", "Error fetching data", throwable);
                    return null;
                });
            });
        });
    }

    private void getFriendUIDs()
    {
        for(int i = 0; i < friends.size(); i++)
        {
            try
            {
                String friendUsername;

                // find friend's username from friendship
                if(friends.get(i).get("FromUserName").toString().equals(username))
                {
                    friendUsername = friends.get(i).get("ToUserName").toString();
                }
                else
                {
                    friendUsername = friends.get(i).get("FromUserName").toString();
                }

                // get friend's info to get their uid
                String apiUrl = "/api/users/getUserByName?userName=" + friendUsername;

                CompletableFuture<String> future = diaryEntryHelper.getItemsFromAPI(apiUrl);
                future.thenAccept(jsonData ->
                {
                    try {
                        // Convert API response into a list of CardItems
                        JSONObject thisFriend = new JSONObject(jsonData);

                        // make new json object with just friends username and uid to easier searching
                        String friendInfo = "{ \"userName\": " + friendUsername + ", \"uid\": " + thisFriend.getString("id") +"}";
                        friendUsernames.add(new JSONObject(friendInfo));
                    }
                    catch (Exception e)
                    {
                        Log.e("JSON", "Error parsing JSON", e);
                    }
                }).exceptionally(throwable ->
                {
                    Log.e("API", "Error fetching data", throwable);
                    return null;
                });
            }
            catch (Exception e)
            {
                Log.e("JSON", "Error parsing JSON", e);
            }
        }

        createDiaryFragment();
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
        SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd'T00:00:00.000'Z");
        System.out.println("***************************************"+formatter.format(cal.getTime()));
        return formatter.format(cal.getTime());
    }
}

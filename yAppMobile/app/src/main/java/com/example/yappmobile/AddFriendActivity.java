package com.example.yappmobile;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ImageButton;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

public class AddFriendActivity extends AppCompatActivity
{
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        // Set the activity to be full screen
        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        setContentView(R.layout.activity_add_friend);

        ImageButton backButton = findViewById(R.id.back_button);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(AddFriendActivity.this, MyRequestsActivity.class);
                startActivity(intent);
            }
        });

        // Create successful friend request alert
        AlertDialog success = new AlertDialog.Builder(this).create();
        success.setTitle("Friend request successfully sent!");
        success.setButton(AlertDialog.BUTTON_POSITIVE, "Heck yeah", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                Intent intent = new Intent(AddFriendActivity.this, MyRequestsActivity.class);
                startActivity(intent);
            }
        });

        // Create failed friend request alert
        AlertDialog failure = new AlertDialog.Builder(this).create();
        failure.setTitle("Friend request failed to send... Try again!");
        failure.setButton(AlertDialog.BUTTON_POSITIVE, "Aw man...", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                failure.dismiss();
            }
        });

        // TODO set up feedback message (prevent them from adding themselves as friends and notify them if a user doesnt exist)
    }

    private void sendPostRequest()
    {
        // TODO
    }
}

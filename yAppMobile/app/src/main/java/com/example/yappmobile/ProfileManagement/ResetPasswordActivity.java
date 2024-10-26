package com.example.yappmobile.ProfileManagement;

import android.content.Intent;
import android.os.Bundle;
import android.text.method.LinkMovementMethod;
import android.view.View;
import android.widget.ImageButton;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.example.yappmobile.NaviBarDestinations.ProfileActivity;
import com.example.yappmobile.R;

public class ResetPasswordActivity extends AppCompatActivity
{
    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        // Set the activity to be full screen
        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);
        setContentView(R.layout.activity_reset_password);

        TextView link = findViewById(R.id.redirect_button);
        link.setMovementMethod(LinkMovementMethod.getInstance());

        ImageButton backButton = findViewById(R.id.back_button);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Intent intent = new Intent(ResetPasswordActivity.this, ProfileActivity.class);
                startActivity(intent);
            }
        });
    }
}

package com.example.yappmobile.ProfileManagement;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageButton;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.NaviBarDestinations.ProfileActivity;
import com.example.yappmobile.R;

public class ResetPasswordActivity extends AppCompatActivity
{
    private AlertDialog success;
    private AlertDialog failure;
    private AlertDialog confirm;

    @Override
    public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        // Set the activity to be full screen
        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        setContentView(R.layout.activity_reset_password);

        // Initialize all dialog alerts
        initializeSuccessDialog();
        initializeFailureDialog();
        initializeConfirmDialog();

        String oldPassword = findViewById(R.id.old_password).toString();
        String newPassword = findViewById(R.id.new_password).toString();

        // Set up code for back button
        ImageButton backButton = findViewById(R.id.back_button);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                if (!isEmptyFields(oldPassword, newPassword))
                {
                    Intent intent = new Intent(ResetPasswordActivity.this, ProfileActivity.class);
                    startActivity(intent);
                }
                else
                {
                    confirm.show();
                }
            }
        });

        // Set up code for reset button
        Button resetButton = findViewById(R.id.reset_button);
        resetButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                invokeUpdatePass(oldPassword, newPassword);
                // TODO figure out a way to show appropriate dialog
                // theres something weird with the main thread.
                // apparently you can't show the dialog alert in the amplify api call
                // TODO also configure user access to let them update their own password
            }
        });
    }

    private void initializeSuccessDialog()
    {
        // Create failed password alert
        success = new AlertDialog.Builder(this).create();
        success.setTitle("Successfully reset password!");
        success.setButton(AlertDialog.BUTTON_POSITIVE, "Heck yeah", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                success.dismiss();
            }
        });
    }

    private void initializeFailureDialog()
    {
        // Create failed password alert
        failure = new AlertDialog.Builder(this).create();
        failure.setTitle("Failed to reset password...Try again!");
        failure.setMessage("Please ensure you enter your old password correctly "
                           + "AND have a minimum of 8 characters in your new password");
        failure.setButton(AlertDialog.BUTTON_POSITIVE, "Aw man...", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                failure.dismiss();
            }
        });
    }

    private void initializeConfirmDialog()
    {
        // Create failed password alert
        confirm = new AlertDialog.Builder(this).create();
        confirm.setTitle("Woah there!");
        confirm.setMessage("You still have some unsaved changes. Are you sure you want to discard them?");
        confirm.setButton(AlertDialog.BUTTON_POSITIVE, "Yes", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                startActivity(new Intent(ResetPasswordActivity.this, ProfileActivity.class));
            }
        });
        confirm.setButton(AlertDialog.BUTTON_NEGATIVE, "No", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                confirm.dismiss();
            }
        });
    }

    private void invokeUpdatePass(String oldPass, String newPass)
    {
        if (!isEmptyFields(oldPass, newPass))
        {
            Amplify.Auth.updatePassword(oldPass,
                                        newPass, () -> Log.i("Auth", "Successful password update!"),
                                        error -> Log.e("Auth", "Failed password update...", error));
        }
        else if (oldPass.isEmpty())
        {
            // TODO
            Log.i("FormField", "Old password is required!!");
        }
        else
        {
            // TODO
            Log.i("FormField", "New password is required!!");
        }
    }

    private boolean isEmptyFields(String oldPass, String newPass)
    {
        // See if we can safely go back to previous page
        return oldPass.trim().isEmpty() && newPass.trim().isEmpty();
    }
}

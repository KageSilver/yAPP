package com.example.yappmobile.ProfileManagement;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageButton;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.amplifyframework.core.Amplify;
import com.example.yappmobile.NaviBarDestinations.ProfileActivity;
import com.example.yappmobile.R;
import com.google.android.material.textfield.TextInputLayout;

import java.util.concurrent.CompletableFuture;

public class ResetPasswordActivity extends AppCompatActivity
{
    private AlertDialog success;
    private AlertDialog failure;
    private AlertDialog confirm;
    private TextInputLayout oldPass;
    private TextInputLayout newPass;

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

        oldPass = findViewById(R.id.old_password);
        newPass = findViewById(R.id.new_password);
        EditText oldPassEditText = oldPass.getEditText();
        EditText newPassEditText = newPass.getEditText();
        oldPassEditText.addTextChangedListener(new TextWatcher()
        {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after)
            {

            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count)
            {

            }

            @Override
            public void afterTextChanged(Editable s)
            {
                oldPass.setError(null);
            }
        });
        newPassEditText.addTextChangedListener(new TextWatcher()
        {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after)
            {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count)
            {

            }

            @Override
            public void afterTextChanged(Editable s)
            {
                final int MIN_CHAR = 6;
                if(s.length() < MIN_CHAR)
                {
                    newPass.setError("Minimum 8 characters");
                }
                else
                {
                    newPass.setError(null);
                }
            }
        });

        // Set up code for back button
        ImageButton backButton = findViewById(R.id.back_button);
        backButton.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                if (isFilled(oldPassEditText.getText().toString(),
                             newPassEditText.getText().toString()))
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
                invokeUpdatePass(oldPassEditText.getText().toString(),
                                 newPassEditText.getText().toString());
                // TODO: Configure user access to let them update their own password
            }
        });
    }

    private void initializeSuccessDialog()
    {
        // Create failed password alert
        success = new AlertDialog.Builder(this).create();
        success.setTitle("Successfully reset password!");
        success.setButton(AlertDialog.BUTTON_POSITIVE,
                          "Heck yeah", new DialogInterface.OnClickListener()
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
        failure.setMessage("Please ensure you enter your old password correctly");
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
        confirm.setButton(AlertDialog.BUTTON_POSITIVE,
                          "Yes", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                startActivity(new Intent(ResetPasswordActivity.this, ProfileActivity.class));
            }
        });
        confirm.setButton(AlertDialog.BUTTON_NEGATIVE,
                          "No", new DialogInterface.OnClickListener()
        {
            public void onClick(DialogInterface dialog, int id)
            {
                confirm.dismiss();
            }
        });
    }

    private void invokeUpdatePass(String oldPassword, String newPassword)
    {
        if (isFilled(oldPassword, newPassword))
        {
            CompletableFuture<Boolean> future = new CompletableFuture<>();
            Amplify.Auth.updatePassword(oldPassword, newPassword,
                                        () -> future.complete(true),
                                        error -> future.complete(false)
                                       );
            future.thenAccept(successful -> {
                runOnUiThread(() -> {
                    if(successful)
                    {
                        success.show();
                    }
                    else
                    {
                        failure.show();
                    }
                });
            });
        }
        else if (oldPassword.isEmpty())
        {
            Log.d("Fault form field", "Old password is required!!");
            oldPass.setError("Old password is required!");
        }
        else
        {
            Log.d("Faulty form field", "New password is required!!");
            newPass.setError("New password is required!");
        }
    }

    private boolean isFilled(String oldPass, String newPass)
    {
        // See if we can safely go back to previous page
        return !oldPass.equals("") && !newPass.equals("");
    }
}

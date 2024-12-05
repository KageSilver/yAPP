package com.example.yappmobile.Posts;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.example.yappmobile.R;
import com.google.android.material.materialswitch.MaterialSwitch;
import com.google.android.material.textfield.TextInputLayout;

import org.json.JSONObject;

public abstract class BasePostActivity extends AppCompatActivity {
    protected TextInputLayout titleText;
    protected TextInputLayout contentText;
    protected MaterialSwitch diaryEntry;
    protected MaterialSwitch anonymous;
    protected String postTitle;
    protected String postBody;
    protected JSONObject post;
    protected AlertDialog successDialog;
    protected AlertDialog failureDialog;
    protected AlertDialog discardDialog;
    protected AlertDialog diaryLimitDialog;

    protected Button actionButton;

    protected Button discardButton;

    protected EditText titleEditText;

    protected EditText contentEditText;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Set the activity to be full screen
        View decorView = getWindow().getDecorView();
        int uiOptions = View.SYSTEM_UI_FLAG_HIDE_NAVIGATION | View.SYSTEM_UI_FLAG_FULLSCREEN;
        decorView.setSystemUiVisibility(uiOptions);

        setContentView(R.layout.activity_create_post);

        post = new JSONObject();
        titleText = findViewById(R.id.post_title);
        contentText = findViewById(R.id.post_content);
        diaryEntry = findViewById(R.id.diaryEntry);
        anonymous = findViewById(R.id.anonymous);
        actionButton = findViewById(R.id.create_button);
        discardButton = findViewById(R.id.discard_button);

        titleEditText = titleText.getEditText();
        contentEditText = contentText.getEditText();
        titleEditText.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
            }

            @Override
            public void afterTextChanged(Editable s) {
                titleText.setError(null);
            }
        });
        contentEditText.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
            }

            @Override
            public void afterTextChanged(Editable s) {
                contentText.setError(null);
            }
        });
        diaryEntry.setOnClickListener(new View.OnClickListener(){
            @Override
            public void onClick(View v){ toggleDiaryEntry(); }
        });
    }

    protected void toggleDiaryEntry() {
        if (diaryEntry.isChecked()) {
            anonymous.setVisibility(View.VISIBLE);
        } else {
            anonymous.setVisibility(View.GONE);
            anonymous.setChecked(true);
        }
    }

    protected boolean hasFilledForms(String postTitle, String postBody) {
        return !postTitle.equals("") && !postBody.equals("");
    }

    public abstract  void initializeSuccessDialog() ;
    public  abstract  void initializeFailureDialog();
    public  abstract  void initializeDiscardDialog();

}




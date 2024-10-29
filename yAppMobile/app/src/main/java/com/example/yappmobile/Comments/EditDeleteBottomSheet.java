package com.example.yappmobile.Comments;

import android.app.Dialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.R;
import com.google.android.material.bottomsheet.BottomSheetDialogFragment;

import org.jetbrains.annotations.Nullable;
import org.json.JSONException;
import org.json.JSONObject;

import java.time.OffsetDateTime;
import java.time.format.DateTimeFormatter;

public class EditDeleteBottomSheet extends BottomSheetDialogFragment {

    private static final String ARG_COMMENT = "comment";

    private String _body;
    private String _cid;

    private String _comment;

    private JSONObject _commentJsonObject;

    private final String LOG_NAME = "EditDelete";
    private static CommentAdapter _adapter;
    public static EditDeleteBottomSheet newInstance(String body, CommentAdapter adapter) {
        _adapter = adapter;
        EditDeleteBottomSheet fragment = new EditDeleteBottomSheet();
        Bundle args = new Bundle();
        args.putString(ARG_COMMENT,  body);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
             _comment = getArguments().getString(ARG_COMMENT);
            try {
                _commentJsonObject = new JSONObject(_comment);
                _body = _commentJsonObject.getString("commentBody");
                _cid = _commentJsonObject.getString("cid");
            } catch (JSONException e) {
                throw new RuntimeException(e);
            }

        }
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.bottom_sheet_edit_delete, container, false);

        if (_body != null && _cid!=null) {
            Button editButton = view.findViewById(R.id.edit_button);
            Button deleteButton = view.findViewById(R.id.delete_button);
            Button cancelButton = view.findViewById(R.id.cancel_button);

            // Handle Edit button click
            editButton.setOnClickListener(v -> {
                showFormDialog();
                dismiss();
            });

            deleteButton.setOnClickListener(v -> {
                showConfirmationDialog();
                dismiss();  // Close this bottom sheet after notifying
            });

            // Handle Cancel button click
            cancelButton.setOnClickListener(v -> dismiss());
        }else {
            // Handle the case where comment is null, to prevent crashes
            Log.e("EditDeleteBottomSheet", "Comment is null");
            dismiss();
        }

        return view;
    }

    private  void  updateComment(String commentBody) throws JSONException {
        OffsetDateTime currentTime = OffsetDateTime.now();
        // Define the desired format
        DateTimeFormatter formatter = DateTimeFormatter.ISO_OFFSET_DATE_TIME;
        // Format the current time into the desired format
        String formattedTime = currentTime.format(formatter);

        _commentJsonObject.put("commentBody",commentBody);
        _commentJsonObject.put("updatedAt",formattedTime);

        String apiUrl = "/api/comments/updateComment";
        RestOptions options = RestOptions.builder()
                .addPath(apiUrl)
                .addBody(_commentJsonObject.toString().getBytes())
                .addHeader("Content-Type", "application/json")
                .build();

        Amplify.API.put(options,
                response -> {  // Corrected "response" spelling
                    Log.i(LOG_NAME, "PUT response: " + response.getData().asString());

                },
                error -> {
                    Log.e(LOG_NAME, "PUT failed: ", error);
                });
    }


    private void showConfirmationDialog() {
        // Create the AlertDialog builder
        AlertDialog.Builder builder = new AlertDialog.Builder(getContext());

        // Set the title and message of the dialog
        builder.setTitle("Woah there")
                .setMessage("Are you sure you want to delete this post?");

        builder.setPositiveButton("Confirm", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                // create delete
                RestOptions options = RestOptions.builder()
                        .addPath("/api/comments/deleteComment?cid="+_cid)
                        .addHeader("Content-Type","application/json")
                        .build();

                Amplify.API.delete(options,
                        response -> {
                            Log.i(LOG_NAME, "DELETE succeeded: " + response);
                            _adapter.removeComment(_cid);
                        },
                        error -> {
                            Log.e(LOG_NAME, "DELETE failed.", error);
                        }
                );
            }
        });

        // Set the negative button (e.g., "No" or "Cancel")
        builder.setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                // Dismiss the dialog when the user cancels
                dialog.dismiss();
            }
        });

        // Create and show the dialog
        AlertDialog dialog = builder.create();
        dialog.show();
    }

    private void showFormDialog() {
        // Create and display the modal form dialog
        final Dialog dialog = new Dialog(getActivity());
        dialog.setContentView(R.layout.dialog_form);

        // Find the EditTexts and the Submit button in the dialog layout
        EditText commentInput = dialog.findViewById(R.id.commentInput);
        commentInput.setText(_body);
        Button submitButton = dialog.findViewById(R.id.submitButton);
        Button cancelButton = dialog.findViewById(R.id.cancelButton);

        cancelButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                dialog.dismiss();
            }
        });

        // Handle form submission
        submitButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String comment = commentInput.getText().toString();
                // Validate inputs and display a Toast message
                if (!comment.isEmpty()) {
                    try {
                        updateComment(comment);
                        _adapter.updateComment(_cid,comment);
                        dialog.dismiss();
                    } catch (JSONException e) {
                      Log.e(LOG_NAME, "Fail to update comment: " +e.getMessage());
                    }
                }
            }
        });

        dialog.show();
    }
}


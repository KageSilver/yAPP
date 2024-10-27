package com.example.yappmobile.Comments;

import android.content.DialogInterface;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;
import com.example.yappmobile.R;
import com.google.android.material.bottomsheet.BottomSheetDialogFragment;

import org.jetbrains.annotations.Nullable;

import java.io.Serializable;

public class EditDeleteBottomSheet extends BottomSheetDialogFragment {

    private static final String ARG_BODY = "body";
    private static final String ARG_CID = "cid";



    private String _body;
    private String _cid;

    private Serializable _comment;

    private final String LOG_NAME = "EditDelete";
    public static EditDeleteBottomSheet newInstance(String body,String cid) {
        EditDeleteBottomSheet fragment = new EditDeleteBottomSheet();
        Bundle args = new Bundle();
        args.putString(ARG_BODY,  body);
        args.putString(ARG_CID,cid);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
             _body = getArguments().getString(ARG_BODY);
             _cid = getArguments().getString(ARG_CID);
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
                // TODO: Implement edit functionality
                Toast.makeText(getContext(), "Edit clicked for comment: " + _body, Toast.LENGTH_SHORT).show();
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


    public void showConfirmationDialog() {
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


                            // Show a Toast message on the main thread
                           getActivity().runOnUiThread(() -> {
                                Toast.makeText(getContext(),"Delete Succesfully", Toast.LENGTH_SHORT).show();
                            });


                        },
                        error -> {
                            Log.e(LOG_NAME, "DELETE failed.", error);

                            // Optionally, show a Toast for failure as well

                            getActivity().runOnUiThread(() -> {
                                Toast.makeText(getContext(), "Delete failed!", Toast.LENGTH_SHORT).show();

                            });


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
}


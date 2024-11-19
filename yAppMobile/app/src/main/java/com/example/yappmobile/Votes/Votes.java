package com.example.yappmobile.Votes;

import android.util.Log;

import com.amplifyframework.api.rest.RestOptions;
import com.amplifyframework.core.Amplify;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.util.concurrent.CompletableFuture;


public class Votes {
    public static CompletableFuture<Boolean> checkVoted(
            boolean type,
            boolean isPost,
            String uuid,
            String pid,
            String log_name
    ) throws UnsupportedEncodingException {
        CompletableFuture<Boolean> future = new CompletableFuture<>();

        // Construct the API path with URL encoding
        String path = "/api/votes/getVote?pid=" + URLEncoder.encode(pid, "UTF-8") +
                "&uid=" + URLEncoder.encode(uuid, "UTF-8") +
                "&type=" + URLEncoder.encode(String.valueOf(type), "UTF-8");

        RestOptions options = RestOptions.builder()
                .addPath(path)
                .addHeader("Content-Type", "application/json")
                .build();

        // Perform the asynchronous API request
        Amplify.API.get(options, response -> {
            try {
                // Convert byte array response to string
                String resultString = new String(response.getData().getRawBytes(), StandardCharsets.UTF_8).trim();
                Log.i(log_name, "Response String: " + resultString);

                // Check if the response is a JSON object
                if (resultString.startsWith("{") && resultString.endsWith("}")) {
                    JSONObject result = new JSONObject(resultString);
                    Log.i(log_name, "Parsed JSON: " + result.toString());

                    Log.i(log_name, "isPost: " + isPost);

                    // Complete with true or false based on the `isPost` value
                    future.complete(true);
                } else if (resultString.trim().equals("Vote does not exist")) {
                    Log.i(log_name, "Vote does not exist");
                    future.complete(false); // Return false for this case
                } else {
                    Log.e(log_name, "Unexpected response: " + resultString);
                    future.complete(false);
                }
            } catch (JSONException e) {
                Log.e(log_name, "Error parsing JSON: " + e.getMessage(), e);
                future.completeExceptionally(e);
            }
        }, error -> {
            Log.e(log_name, "API Error: " + error.getMessage(), error);
            future.completeExceptionally(new RuntimeException("API request failed"));
        });


        return future;
    }


    public static CompletableFuture<Boolean> addVotes(boolean isPost, boolean type, String uuid, String pid, String log_name, boolean up, boolean down) {
        CompletableFuture<Boolean> future = new CompletableFuture<>();

        // Check if the vote action is invalid
        if ((type == true && down) || (type == false && up)) {
            future.complete(false); // Invalid vote action
            return future;
        }

        // Check if we need to remove the existing vote
        if (type == true && up || type == false && down) {
            RestOptions options = RestOptions.builder()
                    .addPath("/api/votes/removeVote?uid=" + uuid + "&pid=" + pid + "&isPost=" + isPost + "&type=" + type)
                    .addHeader("Content-Type", "application/json")
                    .build();

            Amplify.API.delete(options, response -> {
                Log.i(log_name, "Successfully removed vote: " + response.getData().toString());
                future.complete(true); // Successfully removed vote
            }, error -> {
                Log.e(log_name, "Failed to remove vote: " + error.getMessage());
                future.completeExceptionally(new RuntimeException("Failed to remove vote: " + error.getMessage()));
            });
        } else {
            // Add a new vote
            JSONObject votesObj = new JSONObject();
            try {
                votesObj.put("uid", uuid);
                votesObj.put("pid", pid);
                votesObj.put("type", type);
                votesObj.put("isPost", isPost);

                RestOptions options = RestOptions.builder()
                        .addPath("/api/votes/addVote")
                        .addHeader("Content-Type", "application/json")
                        .addBody(votesObj.toString().getBytes())
                        .build();

                Amplify.API.post(options, response -> {
                    Log.i(log_name, "Successfully added vote: " + response.getData().toString());
                    future.complete(true); // Successfully added vote
                }, error -> {
                    Log.e(log_name, "Failed to add vote: " + error.getMessage());
                    future.completeExceptionally(new RuntimeException("Failed to add vote: " + error.getMessage()));
                });
            } catch (JSONException e) {
                Log.e(log_name, "Failed to create vote object: " + e.getMessage());
                future.completeExceptionally(new RuntimeException("Failed to create vote object: " + e.getMessage()));
            }
        }

        return future;
    }

}

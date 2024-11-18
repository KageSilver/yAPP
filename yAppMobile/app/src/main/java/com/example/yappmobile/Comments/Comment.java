package com.example.yappmobile.Comments;

import org.json.JSONException;
import org.json.JSONObject;

public class Comment {
    private final String uid;        // User ID
    private final String cid;        // Comment ID
    private final String pid;        // Post ID
    private String commentBody; // Comment text
    private final String createAt;   // Comment creation timestamp
    private final String updateAt;   // Comment update timestamp

    private final  String downvotes;

    private final String upvotes;


    // Constructor to initialize all fields
    public Comment(String uid, String commentBody, String pid, String cid, String createAt, String updateAt, String downvotes, String upvotes) {
        this.uid = uid;
        this.commentBody = commentBody;
        this.pid = pid;
        this.cid = cid;
        this.createAt = createAt;
        this.updateAt = updateAt;
        this.downvotes = downvotes;
        this.upvotes = upvotes;
    }

    // Getter for uid (User ID)
    public String getUid() {
        return uid;
    }

    // Getter for cid (Comment ID)
    public String getCid() {
        return cid;
    }

    // Getter for pid (Post ID)
    public String getPid() {
        return pid;
    }

    // Getter for commentBody (Comment content)
    public String getCommentBody() {
        return commentBody;
    }

    public void setCommentBody(String commentBody){
        this.commentBody = commentBody;
    }

    // Getter for createAt (Creation timestamp)
    public String getCreateAt() {
        return createAt;
    }

    // Getter for updateAt (Update timestamp)
    public String getUpdateAt() {
        return updateAt;
    }

    public String getUpvotes() {return upvotes;}

    public  String getDownvotes() {return downvotes;}


    @Override
    public String toString() {
        return "Comment{" +
                "uid='" + uid + '\'' +
                ", cid='" + cid + '\'' +
                ", pid='" + pid + '\'' +
                ", commentBody='" + commentBody + '\'' +
                ", Upvotes='" + upvotes + '\'' +
                ", Downvotes='" + downvotes + '\'' +
                ", createAt='" + createAt + '\'' +
                ", updateAt='" + updateAt + '\'' +
                '}';
    }

    public JSONObject getJsonObject() {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("uid", uid);
            jsonObject.put("cid", cid);
            jsonObject.put("pid", pid);
            jsonObject.put("commentBody", commentBody);
            jsonObject.put("upvotes",upvotes);
            jsonObject.put("downvotes",downvotes);
            jsonObject.put("createAt", createAt);
            jsonObject.put("updateAt", updateAt);
        } catch (JSONException e) {
            e.printStackTrace(); // Handle JSON exception
        }
        return jsonObject;
    }
}


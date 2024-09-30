namespace yAppLambda.Models;

/// <summary>
/// Represents a post created by a user
/// </summary>
public class Post
{
    public string PID { get; set; } // Partition key

    public DateTime CreatedAt { get; set; } // Sort key

    public string UserName { get; set; } // The username of the user who created the post

    public string PostTitle { get; set; } // The title of the post

    public string PostBody {get; set; } // The contents of the post

    public int Upvotes { get; set; } // The number of upvotes the post has

    public int Downvotes { get; set; } // The number of downvotes the post has

    public bool DiaryEntry { get; set; } // Is the post a diary entry

    public bool Anonymous { get; set; } // If the post is a diary entry, is it posted anonymously
}
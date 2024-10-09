using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public interface IPostActions
{
    /// <summary>
    /// Creates a new post
    /// </summary>
    /// <param name="post">The post object to be created.</param>
    /// <returns>An ActionResult containing the created Post object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Post>> CreatePost(Post post);

    /// <summary>
    /// Gets a post by the post ID
    /// </summary>
    /// <param name="pid">The id to find a post.</param>
    /// <returns>An ActionResult containing the Post object if found, or a NotFound result otherwise.</returns>
    Task<Post> GetPostById(string pid);

    /// <summary>
    /// Gets all public posts from a user
    /// </summary>
    /// <param name="userName">The username used to find all posts created by a user.</param>
    /// <param name="diaryEntry">If the query is for public posts or diary entries.</param>
    /// <returns>A list of posts created by a user, either public posts or diary entries.</returns>
    Task<List<Post>> GetPostsByUser(string userName, bool diaryEntry);
    
    /// <summary>
    /// Deletes a post from the database by a post id
    /// </summary>
    /// <param name="pid">The id of the post to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    Task<bool> DeletePost(string pid);

    /// <summary>
    /// Edits an already existing post
    /// </summary>
    /// <param name="updatedPost">The new version of the post after editing.</param>
    /// <returns>An ActionResult containing the edited Post object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Post>> UpdatePost(Post updatedPost);

    /// <summary>
    /// Gets all recent posts
    /// </summary>
    /// <param name="since">Returns posts made after this time.</param>
    /// <param name="maxResults">The maximum number of results to retrieve.</param>
    /// <returns>A list of recent posts.</returns>
    Task<List<Post>> GetRecentPosts(DateTime since, int maxResults);
}
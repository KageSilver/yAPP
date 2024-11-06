using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;
using yAppLambda.Common;

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
    /// <param name="uid">The author of the posts to be fetched.</param>
    /// <returns>A list of public posts created by a user.</returns>
    Task<List<Post>> GetPostsByUser(string uid);

    /// <summary>
    /// Gets the diary entries made by a user within a specific day
    /// </summary>
    /// <param name="uid">The author of the diary entry.</param>
    /// <param name="current">The current day to query.</param>
    /// <returns>The diary entry made by a user on the specified day.</returns>
    Task<List<Post>> GetDiariesByUser(string uid, DateTime current);

    /// <summary>
    /// Gets the diary entries made by the user's friends within a specific day
    /// </summary>
    /// <param name="_cognitoActions">An instance of CognitoActions to query user information.</param>
    /// <param name="uid">The user whose friends will be searched for.</param>
    /// <param name="current">The current day to query.</param>
    /// <returns>A list of diary entries made by the user's friends on the specified day</returns>
    Task<List<Post>> GetDiariesByFriends(ICognitoActions _cognitoActions, string uid, DateTime current);
    
    /// <summary>
    /// Gets a specified number of recent posts
    /// </summary>
    /// <param name="since">Returns posts made before this time.</param>
    /// <param name="maxResults">The maximum number of results to retrieve.</param>
    /// <returns>A list of recent posts.</returns>
    Task<List<Post>> GetRecentPosts(DateTime since, int maxResults);

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
}
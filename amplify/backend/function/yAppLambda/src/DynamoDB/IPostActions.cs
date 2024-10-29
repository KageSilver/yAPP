using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;
using yAppLambda.Common;

namespace yAppLambda.DynamoDB;

public interface IPostActions
{
    /// <summary>
    /// Creates a post
    /// </summary>
    /// <param name="post">The post object that contains information on the post.</param>
    /// <returns>An ActionResult containing the created Post object or an error status.</returns>
    Task<ActionResult<Post>> CreatePost(Post post);

    /// <summary>
    /// Gets a post by the post ID
    /// </summary>
    /// <param name="pid">The id to find a post.</param>
    /// <returns>An ActionResult containing the Post object if found, or a NotFound result otherwise.</returns>
    Task<Post> GetPostById(string pid);

    /// <summary>
    /// Gets the user's public posts/diary entries
    /// </summary>
    /// <param name="uid">The author of the public posts/diary entries to be fetched.</param>
    /// <param name="diaryEntry">If the query is for public posts or diary entries.</param>
    /// <returns>A list of posts created by a user, either public posts or diary entries.</returns>
    Task<List<Post>> GetPostsByUser(string uid, bool diaryEntry);

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
    /// Gets all recent posts
    /// </summary>
    /// <param name="since">Returns posts made since this time.</param>
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
    /// Updates an already existing post
    /// </summary>
    /// <param name="updatedPost">The new version of the post after editing.</param>
    /// <returns>An ActionResult containing the edited Post object or an error status.</returns>
    Task<ActionResult<Post>> UpdatePost(Post updatedPost);
}
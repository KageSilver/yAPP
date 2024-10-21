using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public interface ICommentActions
{
    /// <summary>
    /// Creates a new comment
    /// </summary>
    /// <param name="comment">The comment object to be created.</param>
    /// <returns>An ActionResult containing the created comment object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Comment>> CreateComment(Comment comment);

    /// <summary>
    /// Gets the comment given comment ID
    /// </summary>
    /// <param name="cid">The id to find a comment given a comment id.</param>
    /// <returns>The comment associated to the cid.</returns>
    Task<Comment> GetCommentById(string cid);

    /// <summary>
    /// Gets all comments with given post ID
    /// </summary>
    /// <param name="pid">The id to find a post and comment thread.</param>
    /// <returns>A list of comments made under a post.</returns>
    Task<List<Comment>> GetCommentsByPid(string pid);

    /// <summary>
    /// Gets all comments from a user
    /// </summary>
    /// <param name="userName">The username used to find all comments created by a user.</param>
    /// <returns>A list of comments created by a user.</returns>
    Task<List<Comment>> GetCommentsByUser(string userName);
    
}
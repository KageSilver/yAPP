using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

public interface IPostActions
{
    /// <summary>
    /// Creates a new post
    /// </summary>
    /// <param name="post">The post object to be created.</param>
    /// <returns>An ActionResult containing the created Post object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Post>> CreatePost(Post post);
}
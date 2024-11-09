using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public interface IAwardActions
{
    /// <summary>
    /// Creates a new award
    /// </summary>
    /// <param name="award">The award object to be created.</param>
    /// <returns>An ActionResult containing the created award object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Award>> CreateAward(Award award);

    /// <summary>
    /// Gets an award by the award ID
    /// </summary>
    /// <param name="aid">The id to find an award.</param>
    /// <returns>An ActionResult containing the Award object if found, or a NotFound result otherwise.</returns>
    Task<Award> GetAwardById(string aid);

    /// <summary>
    /// Gets all awards from a user
    /// </summary>
    /// <param name="uid">The user who earned the awards being fetched.</param>
    /// <returns>A list of awards earned by the user.</returns>
    Task<List<Award>> GetAwardsByUser(string uid);
    
    /// <summary>
    /// Gets all awards from a post
    /// </summary>
    /// <param name="pid">The post on which the awards were earned.</param>
    /// <returns>A list of awards earned on the post.</returns>
    Task<List<Award>> GetAwardsByPost(string pid);
    
    /// <summary>
    /// Deletes all awards earned on a specific post
    /// </summary>
    /// <param name="pid">The post on which the awards were earned to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    Task<bool> DeleteAwardsByPost(string pid);
    
    /// <summary>
    /// Deletes an award from the database by an award id
    /// </summary>
    /// <param name="aid">The id of the award to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    Task<bool> DeleteAward(string aid);

    /// <summary>
    /// Checks a list of posts for awards
    /// </summary>
    /// <param name="posts">The list of posts to check for awards.</param>
    /// <returns>A list of awards earned on the list of posts.</returns>
    Task<List<Award>> CheckForPostAwards(List<Post> posts);
}
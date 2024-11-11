using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public interface IVoteActions
{
    /// <summary>
    /// Get the given vote status by uid, isPost, id
    /// </summary>
    /// <param name="uid">The uid of the current user.</param>
    /// <param name="id">The id of the post or comment.</param>
    /// <param name="type">Whether it's checking for an upvote/downvote.</param>
    /// <returns>A boolean result showing if the vote exists.</returns>
    Task<bool> GetVoteStatus(string uid, string id, bool type);

    /// <summary>
    /// Gets all votes with given ID
    /// </summary>
    /// <param name="id">The id to find a vote under.</param>
    /// <returns>A list of votes made under a post/comment.</returns>
    Task<List<Vote>> GetVotesById(string id);
    
    /// <summary>
    /// Creates a new vote
    /// </summary>
    /// <param name="vote">The vote object to be created.</param>
    /// <returns>An ActionResult containing the created vote object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Vote>> AddVote(Vote vote);

    /// <summary>
    /// Remove the corresponding vote by id and uid
    /// </summary>
    /// <param name="uid">The uid of the current user.</param>
    /// <param name="id">The id of the post or comment.</param>
    /// <param name="type">Whether it's removing an upvote/downvote.</param>
    /// <returns>A boolean result determining if the deletion failed.</returns>
    Task<bool> RemoveVote(string uid, string id, bool type);
    
    /// <summary>
    /// Deletes all votes under one post/comment from the database by id
    /// </summary>
    /// <param name="id">The id of the parent post/comment to be deleted.</param>
    /// <returns>A boolean indicating whether the deletions were successful.</returns>
    Task<bool> DeleteVotes(string id);

}
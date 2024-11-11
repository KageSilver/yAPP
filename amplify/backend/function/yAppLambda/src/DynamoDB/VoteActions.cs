using System.Numerics;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class VoteActions : IVoteActions
{
    // Default table name for vote table
    private const string VoteTableName = "Vote-test";
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly string _voteTable;
    private readonly DynamoDBOperationConfig _config;

    public VoteActions(IAppSettings appSettings, IDynamoDBContext dynamoDbContext)
    {
        _appSettings = appSettings;
        _dynamoDbContext = dynamoDbContext;

        _voteTable = string.IsNullOrEmpty(_appSettings.VoteTableName)
            ? VoteTableName
            : _appSettings.VoteTableName;
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _voteTable
        };
    }

    /// <summary>
    /// Get the given vote status by uid, isPost, id
    /// </summary>
    /// <param name="uid">The uid of the current user.</param>
    /// <param name="id">The id of the post or comment.</param>
    /// <param name="type">Whether it's checking for an upvote/downvote.</param>
    /// <returns>A boolean result showing if the vote exists.</returns>
    public async Task<bool> GetVoteStatus(string uid, string id, bool type)
    {
        bool result = true;
        try
        {
            List<ScanCondition> scanConditions = new List<ScanCondition>
            {
                new ScanCondition("UID", ScanOperator.Equal, uid),
                new ScanCondition("ID", ScanOperator.Equal, id),
                new ScanCondition("Type", ScanOperator.Equal, type)
            };

            // Query votes where the user's uid is 'uid' and is equal to the given id and whether it's an upvote/downvote
            var vote = await _dynamoDbContext.ScanAsync<Vote>(scanConditions, _config).GetRemainingAsync();

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get vote status: " + e.Message);
            result = false;
        }
        return result;
    }

    /// <summary>
    /// Gets all votes with given ID
    /// </summary>
    /// <param name="id">The id to find a vote under.</param>
    /// <returns>A list of votes made under a post/comment.</returns>
    public async Task<List<Vote>> GetVotesById(string id)
    {
        try
        {
            List<ScanCondition> scanConditions = new List<ScanCondition>
            {
                new ScanCondition("ID", ScanOperator.Equal, id)
            };

            // Query votes where the id is 'id'
            var votes = await _dynamoDbContext.ScanAsync<Vote>(scanConditions, _config).GetRemainingAsync();

            return votes;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get votes: " + e.Message);
            return new List<Vote>();
        }
    }

    /// <summary>
    /// Creates a new vote
    /// </summary>
    /// <param name="vote">The vote object to be created.</param>
    /// <returns>An ActionResult containing the created vote object if successful, or an error message if it fails.</returns>
    public async Task<ActionResult<Vote>> AddVote(Vote vote)
    {
        try
        {
            await _dynamoDbContext.SaveAsync(vote, _config);

            return new OkObjectResult(vote);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create vote: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
    
    /// <summary>
    /// Remove the corresponding vote by id and uid
    /// </summary>
    /// <param name="uid">The uid of the current user.</param>
    /// <param name="id">The id of the post or comment.</param>
    /// <param name="type">Whether it's removing an upvote/downvote.</param>
    /// <returns>A boolean result determining if the deletion failed.</returns>
    public async Task<bool> RemoveVote(string uid, string id, bool type)
    {
        bool result = true;
        try
        {
            // Load the vote record to check if it exists
            var vote = GetVoteStatus(uid, id, type);

            if (vote.Result == null)
            {
                Console.WriteLine("Failed to retrieve vote");
                result = false;
            }
            else
            {
                // Delete the vote from the database
                await _dynamoDbContext.DeleteAsync(vote.Result, _config);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to delete vote: " + e.Message);
            result = false;
        }
        return result;
    }

    /// <summary>
    /// Deletes all votes under one post/comment from the database by id
    /// </summary>
    /// <param name="id">The id of the parent post/comment to be deleted.</param>
    /// <returns>A boolean indicating whether the deletions were successful.</returns>
    public async Task<bool> DeleteVotes(string id)
    {
        var result = true;
        try
        {
            // Load the votes to check if the id exists
            var votes = await GetVotesById(id);

            if (votes.Count == 0)
            {
                Console.WriteLine("Failed to retrieve votes");
                result = false;
            }
            else
            {
                // Delete all votes under the post/comment from the database
                foreach ( var vote in votes )
                {
                    if ( ! await RemoveVote(vote.UID, vote.ID, vote.Type) )
                    {
                        result = false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to delete votes: " + e.Message);
            result = false;
        }
        return result;
    }
    
}
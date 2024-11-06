using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.Enum;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class AwardActions : IAwardActions
{
    // This is the default table name for the award table
    private const string AwardTableName = "Award-test"; 
    private readonly string _awardTable;
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DynamoDBOperationConfig _config;

    public AwardActions(IAppSettings appSettings, IDynamoDBContext dynamoDbContext)
    {
        _appSettings = appSettings;
        _dynamoDbContext = dynamoDbContext;
        
        _awardTable = string.IsNullOrEmpty(_appSettings.AwardTableName)
            ? AwardTableName
            : _appSettings.AwardTableName;
        
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _awardTable
        };
    }

    /// <summary>
    /// Creates a new award
    /// </summary>
    /// <param name="award">The award object to be created.</param>
    /// <returns>An ActionResult containing the created award object if successful, or an error message if it fails.</returns>
    public async Task<ActionResult<Award>> CreateAward(Award award)
    {
        try
        {
            award.CreatedAt = DateTime.Now;
            
            // Gets a unique ID for the award
            award.AID = Guid.NewGuid().ToString();

            await _dynamoDbContext.SaveAsync(award, _config);
            return new OkObjectResult(award);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create award: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
    
    /// <summary>
    /// Gets an award by the award ID
    /// </summary>
    /// <param name="aid">The id to find an award.</param>
    /// <returns>An ActionResult containing the Award object if found, or a NotFound result otherwise.</returns>
    public async Task<Award> GetAwardById(string aid)
    {
        try
        {
            var award = await _dynamoDbContext.LoadAsync<Award>(aid, _config);
            return award;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get award: " + e.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Gets all awards from a user
    /// </summary>
    /// <param name="uid">The user who earned the awards being fetched.</param>
    /// <returns>A list of awards earned by the user.</returns>
    public async Task<List<Award>> GetAwardsByUser(string uid)
    {
        try
        {
            // Scan for awards where the award earner's uid is 'uid'
            List<ScanCondition> scanConditions = new List<ScanCondition>
            {
                new ScanCondition("UID", ScanOperator.Equal, uid)
            };
            
            var awards = await _dynamoDbContext.ScanAsync<Award>(scanConditions, _config).GetRemainingAsync();
            return awards;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get awards: " + e.Message);
            return new List<Award>();
        }
    }
    
    /// <summary>
    /// Gets all awards from a post
    /// </summary>
    /// <param name="pid">The post on which the awards were earned.</param>
    /// <returns>A list of awards earned on the post.</returns>
    public async Task<List<Award>> GetAwardsByPost(string pid)
    {
        try
        {
            var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
            expressionAttributeValues.Add(":pid", false);

            var query = new QueryOperationConfig()
            {
                IndexName = "PIDIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "PID = :pid",
                    ExpressionAttributeValues = expressionAttributeValues
                },
                Limit = maxResults,
                AttributesToGet = new List<string>
                {
                    "CID"
                },
                Select = SelectValues.SpecificAttributes,
                BackwardSearch = true
            };

            var result = await _dynamoDbContext.FromQueryAsync<Award>(query, _config).GetNextSetAsync();
            
            var awards = new List<Award>();
            
            foreach(Award award in result)
            {
                var thisAward = GetAwardById(award.AID).Result;
                awards.Add(thisAward);
            }

            return awards;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get awards: " + e.Message);
            return new List<Award>();
        }
    }
        
    /// <summary>
    /// Deletes an award from the database by an award id
    /// </summary>
    /// <param name="aid">The id of the award to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteAward(string aid)
    {
        try
        {
            // Load the award record to check if it exists
            var award = GetAwardById(aid);

            if (award.Result == null)
            {
                Console.WriteLine("Failed to retrieve award to be deleted");
                return false;
            }

            // Delete the award from the database
            await _dynamoDbContext.DeleteAsync(award.Result, _config);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to delete award: " + e.Message);
            return false;
        }
    }
}
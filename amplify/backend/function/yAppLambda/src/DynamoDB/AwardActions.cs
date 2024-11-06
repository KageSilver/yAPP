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
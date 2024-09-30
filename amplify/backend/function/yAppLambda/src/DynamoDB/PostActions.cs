using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public static class PostActions
{
    //This is the default table name for the post table
    private const string PostTableName = "Post-test";
    
    /// <summary>
    /// Creates a post
    /// </summary>
    /// <param name="post">The post object that contains information on the post.</param>
    /// <param name="dynamoDbContext">The DynamoDB context to interact with the database.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>An ActionResult containing the created Post object or an error status.</returns>
   public static async Task<ActionResult<Post>> CreatePost(Post post,
        IDynamoDBContext dynamoDbContext, IAppSettings appSettings)
    {
        try
        {
            var postTable = string.IsNullOrEmpty(appSettings.PostTableName)
                ? PostTableName
                : appSettings.PostTableName;
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = postTable
            };

            // update the current time
            post.CreatedAt = DateTime.Now;
            // gets a unique ID for the post
            post.PID = Guid.NewGuid().ToString();

            await dynamoDbContext.SaveAsync(post, config);

            return new OkObjectResult(post);

        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create post: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
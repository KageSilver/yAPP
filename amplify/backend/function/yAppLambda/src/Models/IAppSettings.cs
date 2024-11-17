using Amazon;

namespace yAppLambda.Models;

public interface IAppSettings
{
    /// <summary>
    /// Gets or sets the name of the friendship table.
    /// Cloud environment
    /// </summary>
    string Environment { get; set; }

    /// <summary>
    /// AWS region 
    /// </summary>
    string AwsRegion { get; set; }

    /// <summary>
    /// Cognito user pool id
    /// </summary>
    string UserPoolId { get; set; }
    
    /// <summary>
    /// Convert the region to the endpoint
    /// </summary>
    RegionEndpoint AwsRegionEndpoint { get; }

    /// <summary>
    /// Friendship table name
    /// </summary>
    string FriendshipTableName { get; set; }

    /// <summary>
    /// Post table name
    /// </summary>
    string PostTableName { get; set; }

    /// <summary>
    /// Comment table name
    /// </summary>
    string CommentTableName { get; set; }
    
    /// <summary>
    /// award table name
    /// </summary>
    string AwardTableName { get; set; }
    
    /// <summary>
    /// Vote table name
    /// </summary>
    string VoteTableName { get; set; }
}
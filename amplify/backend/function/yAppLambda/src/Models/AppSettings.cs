using Amazon;

namespace yAppLambda.Models;

public class AppSettings: IAppSettings
{
    /// <summary>
    /// Cloud environment, it should be either test, dev or main
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// The region of the web application
    /// </summary>
    public string AwsRegion { get; set; }

    /// <summary>
    /// Cognito user pool id
    /// </summary>
    public string UserPoolId { get; set; }
    
    /// <summary>
    /// Convert the region to the endpoint
    /// </summary>
    public RegionEndpoint AwsRegionEndpoint => RegionEndpoint.GetBySystemName(AwsRegion);

    /// <summary>
    /// friendship table name
    /// </summary>
    public string FriendshipTableName { get; set; }

    /// <summary>
    /// post table name
    /// </summary>
    public string PostTableName { get; set; }

    /// <summary>
    /// comment table name
    /// </summary>
    public string CommentTableName { get; set; }
}
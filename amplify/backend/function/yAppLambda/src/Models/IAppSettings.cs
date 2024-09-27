using Amazon;

namespace yAppLambda.Models;

public interface IAppSettings
{
    /// <summary>
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
}
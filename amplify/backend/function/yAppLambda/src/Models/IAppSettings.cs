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
    /// Cognito user pool url 
    /// </summary>
    string Cognito { get; set; }
}
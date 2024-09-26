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
    /// Cognito user pool url 
    /// </summary>
    public string Cognito { get; set; }
}
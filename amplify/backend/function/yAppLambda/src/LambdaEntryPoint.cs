using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace yAppLambda;

/// <summary>
/// This class extends from APIGatewayProxyFunction which contains the methods FunctionHandlerAsync which is the
/// actual lambda function entry point. The lambda handler field should be set to
///
/// yAppLambda::yAppLambda.LambdaEntryPoint::FunctionHandlerAsync
/// </summary>
public class LambdaEntryPoint :
    APIGatewayProxyFunction
{
    /// <summary>
    /// The builder has configuration, logging and Amazon API Gateway already configured. The Startup class
    /// needs to be configured in this method using the UseStartup<>() method
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IWebHostBuilder builder)
    {
        builder.UseStartup<Startup>();
    }
    
    /// <summary>
    /// Use this override to customize the services registered with the IHostBuilder
    ///
    /// It is recommended not to call ConfigurationWebHostDefaults to configure the IWebHostBuilder inside this method.
    /// Instead customized the IWebHostBuilder in the Init(IWebHostBuilder) overload.
    /// </summary>
    /// <param name="builder"></param>
    protected override void Init(IHostBuilder builder)
    {
       
    }
}

using Amazon.Lambda.AspNetCoreServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace yAppLambda;

public class LambdaEntryPoint : APIGatewayProxyFunction
{
    protected override void Init(IHostBuilder builder)
    {
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>(); 
        });
    }
}

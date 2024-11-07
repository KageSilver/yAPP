using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace yAppLambda;

/// <summary>
/// The Main Program can be used to run the ASP.NET Core application using Kestrel webserver.
/// </summary>
public class LocalEntryPoint
{
    public static void Main (string[] arg){
        CreateHostBuilder(arg).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            webBuilder => { webBuilder.UseStartup<Startup>(); });
}
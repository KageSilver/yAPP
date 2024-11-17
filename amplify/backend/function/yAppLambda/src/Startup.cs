using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;

namespace yAppLambda;

public class Startup
{
    public IConfiguration Configuration { get; set; }
    private bool IsLocal { get; set; }

    private IAppSettings _appSettings; // the settings file for current lambda

    /// <summary>
    /// Startup constructor
    /// `xmlFileName` is optional only to use it in the cloud. However, to use it locally (debug) it is required
    /// or an error will be thrown.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="xmlFilename">use $"{Assembly.GetExecutingAssembly().GetName().Name}.xml" as parameter</param>
    public Startup(IConfiguration configuration, string? xmlFilename = default)
    {
        var functionName = Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME");
        IsLocal = string.IsNullOrEmpty(functionName);
        Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        Console.WriteLine("Starting ConfigurationServices");
        
        try
        {
            services.AddEndpointsApiExplorer();
            services.AddHttpLogging(_ => { });
            if (IsLocal)
            {
                services.AddSwaggerGen(opts =>
                {
                    opts.AddEnumsWithValuesFixFilters();
                });
            }

            // convert the settings to the class object
            _appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(@"appSettings.json"), new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });
            Console.WriteLine($"Environment: {_appSettings.Environment}" );
            Console.WriteLine($"Cognito: {_appSettings.UserPoolId}");
            Console.WriteLine($"Region: {_appSettings.AwsRegionEndpoint}");
            
            // Manually configure AWS options
            var awsOptions = new AWSOptions
            {
                Region = _appSettings.AwsRegionEndpoint
            };
            
            services.AddDefaultAWSOptions(awsOptions);// Register AmazonDynamoDBClient as a singleton
            services.AddCognitoIdentity();
            //TODO: Add authentication & authorization service with cognito

            services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
                options.SerializerOptions.PropertyNamingPolicy = null;
                options.SerializerOptions.WriteIndented = true;
            });
            
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.WriteIndented = true;
            });
            
            
            services.AddControllers().AddJsonOptions(options=> {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.WriteIndented = true;
            });
            
            //Inject shared connection wrappers if they don't exist
            services.AddSingleton<IAppSettings>(_appSettings);
            services.AddSingleton<ICognitoActions,CognitoActions>();
            services.AddSingleton<IFriendshipStatusActions, FriendshipStatusActions>();  
            
           
            // Register AmazonDynamoDBClient as a singleton with the specified region
            services.AddSingleton<IAmazonDynamoDB>(sp =>
            {
                var config = new AmazonDynamoDBConfig
                {
                    RegionEndpoint = _appSettings.AwsRegionEndpoint
                };
                return new AmazonDynamoDBClient(config);
            });

            // Register DynamoDBContext as a scoped service
            services.AddScoped<IDynamoDBContext, DynamoDBContext>();
            services.AddScoped<IFriendshipActions, FriendshipActions>();
            services.AddScoped<IPostActions, PostActions>();
            services.AddScoped<ICommentActions, CommentActions>();
            services.AddScoped<IVoteActions, VoteActions>();

        }
        catch (Exception e)
        {
            Console.WriteLine($"Fatal error occurred in host builder: ${e.Message}");
        }
        Console.WriteLine("Finishing ConfigureServices.");
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        try
        {
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.Use((context, next) =>
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                context.Response.Headers["Access-Control-Allow-Headers"] = "GET, OPTIONS, POST, PUT, DELETE";
                context.Response.Headers["Access-Control-Allow-Headers"] =
                    "Origin, X-Requested-With, Content-Type, Accept, Authorization";
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                context.Response.Headers["Content-Type"] = "application/json";

                return next.Invoke();
            });

            if (IsLocal)
            {
                app.UseSwagger();
            }

            app.UseSwaggerUI();

            //redirect to the ErrorHandler endpoint to handle exception
            app.UseExceptionHandler("/error");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization(); TODO: Add authentication & authorization with cognito

            app.UseHttpLogging();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
        catch (Exception e)
        {
            Console.WriteLine($"Fatal error occurred in host builder: {e.Message}");
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit.Extensions.Ordering;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Enum;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using yAppLambda;
using Newtonsoft.Json;
using System.Net;
using Moq;
using yAppLambda.Models;

namespace Tests.IntegrationTests;

public class PostControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail1 = "bounce4@simulator.amazonses.com";

    private ICognitoActions _cognitoActions;
    private IPostActions _postActions;

    public PostControllerIntegrationTests()
    {
        var webHostBuilder = new WebHostBuilder().UseStartup<Startup>();

        var server = new TestServer(webHostBuilder);

        _client = server.CreateClient();

        _appSettings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(@"appSettings.json"),
            new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });

        IAmazonCognitoIdentityProvider cognitoClient =
            new AmazonCognitoIdentityProviderClient(_appSettings.AwsRegionEndpoint);

        _cognitoActions = new CognitoActions(cognitoClient, _appSettings);
        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = _appSettings.AwsRegionEndpoint
        };
        var client = new AmazonDynamoDBClient(config);
        IDynamoDBContext dynamoDbContext = new DynamoDBContext(client);

        _postActions = new PostActions(_appSettings, dynamoDbContext);
    }

}
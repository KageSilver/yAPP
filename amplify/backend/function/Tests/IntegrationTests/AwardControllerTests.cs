using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit.Extensions.Ordering;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using yAppLambda;
using System.Net;
using Newtonsoft.Json;
using yAppLambda.Models;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Controllers;

namespace Tests.IntegrationTests;

public class AwardControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail = "bounce4@simulator.amazonses.com";
    private static string _testUserId = ""; // this will be updated in the first test when the test user is created
    
    private ICognitoActions _cognitoActions;
    private IAwardActions _awardActions;
    public AwardControllerIntegrationTests()
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

        _awardActions = new AwardActions(_appSettings, dynamoDbContext);
    }
    
    #region GetAwardById Tests

    [Fact, Order(1)]
    public async Task GetAwardById_ShouldReturnAward_WhenSuccessful()
    {
        //setup the user for testing
        await _cognitoActions.CreateUser(TestUserEmail);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created

        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);
        _testUserId = user.Id;
        
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = _testUserId,
            CreatedAt = DateTime.Now,
            Name = "GetAwardById_ShouldReturnAward_WhenSuccessful()"
        };

        var createAward = await _awardActions.CreateAward(request);
        var actionResult = Assert.IsType<ActionResult<Award>>(createAward);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedAward = Assert.IsType<Award>(okResult.Value);
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardById?aid={returnedAward.AID}");
        var responseString = await response.Content.ReadAsStringAsync();
        var award = JsonConvert.DeserializeObject<Award>(responseString);
        
        // Assert
        Assert.NotNull(award);
        Assert.Equal(award.PID, request.PID);
        Assert.Equal(award.UID, request.UID);
        Assert.Equal(award.AID, request.AID);
        Assert.Equal(award.Name, request.Name);

        // Clean up
        await _awardActions.DeleteAward(returnedAward.AID);
        // Test user is deleted in GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()
    }
    
    [Fact, Order(2)]
    public async Task GetAwardById_ShouldReturnNotFound_WhenAwardNotFound()
    {
        // Act
        var aid = "1";
        var response = await _client.GetAsync($"/api/awards/getAwardById?aid={aid}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact, Order(3)]
    public async Task GetAwardById_ShouldReturnBadRequest_WithInvalidAID()
    {
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardById?aid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion
    
    #region GetAwardsByUser Tests

    [Fact, Order(4)]
    public async Task GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = _testUserId,
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()"
        };

        await _awardActions.CreateAward(request);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByUser?uid={_testUserId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var award = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(award);
        Assert.Equal(1, award.Count);
        Assert.Equal(award.First().PID, request.PID);
        Assert.Equal(award.First().UID, request.UID);
        Assert.Equal(award.First().AID, request.AID);
        Assert.Equal(award.First().Name, request.Name);

        // Clean up
        await _awardActions.DeleteAward(request.AID);
        // Test user is deleted in GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()
    }

    [Fact, Order(5)]
    public async Task GetAwardsByUser_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByUser?uid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion

    #region GetAwardsByPost Tests

    [Fact, Order(6)]
    public async Task GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = _testUserId,
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()"
        };

        await _awardActions.CreateAward(request);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByPost?pid={request.PID}");
        var responseString = await response.Content.ReadAsStringAsync();
        var award = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(award);
        Assert.Equal(1, award.Count);
        Assert.Equal(award.First().PID, request.PID);
        Assert.Equal(award.First().UID, request.UID);
        Assert.Equal(award.First().AID, request.AID);
        Assert.Equal(award.First().Name, request.Name);

        // Clean up
        await _awardActions.DeleteAward(request.AID);
        await _cognitoActions.DeleteUser(_testUserId);
    }
    
    [Fact, Order(7)]
    public async Task GetAwardsByPosts_ShouldReturnBadRequest_WithInvalidPID()
    {
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByPost?pid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion
}
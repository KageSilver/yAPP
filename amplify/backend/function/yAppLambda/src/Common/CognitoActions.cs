using System.Net;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.Common;

public static class CognitoActions
{
    /// <summary>
    /// Updates the user attributes in the Cognito user pool.
    /// </summary>
    /// <param name="updateUser">The user object containing updated attributes.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>An APIGatewayProxyResponse indicating the result of the update operation.</returns>
    public static async Task<ActionResult<User>> UpdateUser(User updateUser, IAppSettings appSettings)

    {
        var updateUserAttributesRequest = new AdminUpdateUserAttributesRequest
        {
            UserPoolId = appSettings.UserPoolId,
            Username = updateUser.UserName,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType
                {
                    Name = "name",
                    Value = updateUser.Name
                }
            }
        };


        var awsCognitoIdentityProviderConfig = new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = appSettings.AwsRegionEndpoint
        };

        var cognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(awsCognitoIdentityProviderConfig);

        var response = await cognitoIdentityProvider.AdminUpdateUserAttributesAsync(updateUserAttributesRequest);
        if (response.HttpStatusCode == (HttpStatusCode)StatusCodes.Status200OK)
        {
            return await GetUser(updateUser.UserName, appSettings);
        }


        return null;
    }

    /// <summary>
    /// Get users from the user pool  
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="appSettings"></param>
    /// <returns>APIGatewayProxyResponse</returns>
    public static async Task<User> GetUser(string userName, IAppSettings appSettings)
    {
        Console.WriteLine("GetUser Started");
        try
        {
            var result = await GetUserDetails(userName, appSettings);
            var user = new User()
            {
                UserName = result.Username,
                Email = result.UserAttributes.FirstOrDefault(attr => attr.Name == "email")?.Value,
                Name = result.UserAttributes.FirstOrDefault(attr => attr.Name == "name")?.Value,
                Attributes = result.UserAttributes.ToDictionary(attr => attr.Name, attr => attr.Value)
            };
            return user;
        }
        catch (AmazonServiceException exAws)
        {
            Console.WriteLine($"Failed: Status: {exAws.StatusCode} - Message: {exAws.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
            return null;
        }
    }

    private static async Task<AdminGetUserResponse> GetUserDetails(string userName, IAppSettings appSettings)
    {
        //Get User Params
        var adminGetUserRequest = new AdminGetUserRequest()
        {
            Username = userName,
            UserPoolId = appSettings.UserPoolId
        };

        var adminListGroupsForUserRequest = new AdminListGroupsForUserRequest()
        {
            Username = userName,
            UserPoolId = appSettings.UserPoolId
        };

        Console.WriteLine($"Attempting to Get User: {userName}  with those params: {adminGetUserRequest}");
        var awsCognitoIdentityProviderConfig = new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = appSettings.AwsRegionEndpoint
        };

        var cognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(awsCognitoIdentityProviderConfig);

        var adminGetUserResponse = await cognitoIdentityProvider.AdminGetUserAsync(adminGetUserRequest);
        var result = new AdminGetUserResponse()
        {
            Username = adminGetUserResponse.Username,
            UserStatus = adminGetUserResponse.UserStatus,
            Enabled = adminGetUserResponse.Enabled,
            UserAttributes = adminGetUserResponse.UserAttributes,
        };

        Console.WriteLine($"Get user successfully");
        return result;
    }
}
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
                },
                new AttributeType
                {
                    Name = "nickname",
                    Value = updateUser.NickName
                },
            }
        };

        var awsCognitoIdentityProviderConfig = new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = appSettings.AwsRegionEndpoint
        };

        var cognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(awsCognitoIdentityProviderConfig);

        try
        {
            var response = await cognitoIdentityProvider.AdminUpdateUserAttributesAsync(updateUserAttributesRequest);
            if (response.HttpStatusCode == (HttpStatusCode)StatusCodes.Status200OK)
            {
                return await GetUser(updateUser.UserName, appSettings);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed: {e.Message}");
            return null;
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
                NickName = result.UserAttributes.FirstOrDefault(attr => attr.Name == "nickname")?.Value,
                Id = result.UserAttributes.FirstOrDefault(attr => attr.Name == "sub")?.Value,
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

    /// <summary>
    /// Retrieves a user from the Cognito user pool by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>A `User` object representing the user details, or `null` if the user is not found.</returns>
    public static async Task<User> GetUserById(string userId, IAppSettings appSettings)
    {
        var request = new ListUsersRequest
        {
            UserPoolId = appSettings.UserPoolId,
            Filter = $"sub = \"{userId}\""
        };

        var awsCognitoIdentityProviderConfig = new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = appSettings.AwsRegionEndpoint
        };

        var cognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(awsCognitoIdentityProviderConfig);
        
        var response = await cognitoIdentityProvider.ListUsersAsync(request);

        // Get the first (and likely only) user returned by the filter
        // If the user is found, return the user details
        var cognitoUser = response.Users.FirstOrDefault();

        if (cognitoUser != null)
        {
            var user = new User
            {
                UserName = cognitoUser.Username,
                Email = cognitoUser.Attributes.FirstOrDefault(attr => attr.Name == "email")?.Value,
                Name = cognitoUser.Attributes.FirstOrDefault(attr => attr.Name == "name")?.Value,
                Id = cognitoUser.Attributes.FirstOrDefault(attr => attr.Name == "sub")?.Value,
                Attributes = cognitoUser.Attributes.ToDictionary(attr => attr.Name, attr => attr.Value),
                NickName = cognitoUser.Attributes.FirstOrDefault(attr => attr.Name == "nickname")?.Value
            };
            return user;
        }

        Console.WriteLine("User not found");
        return null;
    }

    /// <summary>
    /// Retrieves detailed information about a user from the Cognito user pool.
    /// </summary>
    /// <param name="userName">The username of the user to retrieve.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>An `AdminGetUserResponse` object containing the user details.</returns>
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
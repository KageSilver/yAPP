using System.Net;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.Common;

public class CognitoActions:ICognitoActions
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly IAppSettings _appSettings;

    public CognitoActions(IAmazonCognitoIdentityProvider cognitoClient, IAppSettings appSettings)
    {
        _cognitoClient = cognitoClient;
        _appSettings = appSettings;
    }

    public async Task<ActionResult<User>> UpdateUser(User updateUser)
    {
        var updateUserAttributesRequest = new AdminUpdateUserAttributesRequest
        {
            UserPoolId = _appSettings.UserPoolId,
            Username = updateUser.UserName,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "name", Value = updateUser.Name },
                new AttributeType { Name = "nickname", Value = updateUser.NickName }
            }
        };

        try
        {
            var response = await _cognitoClient.AdminUpdateUserAttributesAsync(updateUserAttributesRequest);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return await GetUser(updateUser.UserName);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed: {e.Message}");
            return null;
        }

        return null;
    }

    public async Task<User> GetUser(string userName)
    {
        try
        {
            var result = await GetUserDetails(userName);
            var user = new User
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

    public async Task<User> GetUserById(string userId)
    {
        var request = new ListUsersRequest
        {
            UserPoolId = _appSettings.UserPoolId,
            Filter = $"sub = \"{userId}\""
        };

        var response = await _cognitoClient.ListUsersAsync(request);

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

        return null;
    }

    // This method is private in your class, so you'd mock it in your tests
    private async Task<AdminGetUserResponse> GetUserDetails(string userName)
    {
        var adminGetUserRequest = new AdminGetUserRequest
        {
            Username = userName,
            UserPoolId = _appSettings.UserPoolId
        };

        var adminGetUserResponse = await _cognitoClient.AdminGetUserAsync(adminGetUserRequest);

        return adminGetUserResponse;
    }
}

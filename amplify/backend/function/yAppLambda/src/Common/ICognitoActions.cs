using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.Common;

/// <summary>
/// Interface for Cognito actions, including updating user attributes and retrieving user details.
/// </summary>
public interface ICognitoActions
{
    /// <summary>
    /// Updates the user attributes in the Cognito user pool and retrieves the updated user details.
    /// </summary>
    /// <param name="updateUser">The user object containing updated attributes.</param>
    /// <returns>An ActionResult containing the updated User object or null if the update fails.</returns>
    Task<ActionResult<User>> UpdateUser(User updateUser);

    /// <summary>
    /// Retrieves the user details from the Cognito user pool by username.
    /// </summary>
    /// <param name="userName">The username of the user to retrieve.</param>
    /// <returns>A Task containing the User object or null if the user is not found or an error occurs.</returns>
    Task<User> GetUser(string userName);


    /// <summary>
    /// Retrieves the user details from the Cognito user pool by user ID.
    /// </summary>
    /// <param name="sub">The ID of the user to retrieve.</param>
    /// <returns>A Task containing the User object or null if the user is not found or an error occurs.</returns>
    Task<User> GetUserById(string sub);
}
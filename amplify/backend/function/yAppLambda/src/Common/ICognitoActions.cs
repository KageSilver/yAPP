using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.Common;

public interface ICognitoActions
{
    Task<ActionResult<User>> UpdateUser(User updateUser);
    Task<User> GetUser(string userName);
    Task<User> GetUserById(string sub);
}
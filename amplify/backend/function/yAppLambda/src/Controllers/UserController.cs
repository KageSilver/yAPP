using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.Models;

namespace yAppLambda.Controllers;

/// <summary>
/// The `UserController` class is an API controller in the `yAppLambda` project. It is responsible for handling HTTP requests related to user operations.
/// The api gateway's entry point is /api, therefore we need to ensure "api" in the route
/// </summary>
[ApiController]
[Route("api/users")]
public class UserController:ControllerBase
{
    private readonly IAppSettings _appSettings;
    
    public UserController(IAppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    // POST: api/users/updateUser
    //Example: body: {"userName":"testuser","name":"Test User"}
    [HttpPost("updateUser")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> UpdateUser([FromBody] User request)
    {
        if (request == null || request.UserName == null || request.Name == null)
        {
            return BadRequest("request body is required and must contain username and name");
        }       
        
        return await CognitoActions.UpdateUser(request, _appSettings);
    }

    // GET: api/cognito/getUser?username={username}
    [HttpGet("getUser")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUser(string username)
    {
        if (username== null)
        {
            return BadRequest("username is required");
        }
        return await CognitoActions.GetUser(username, _appSettings);
    }
}
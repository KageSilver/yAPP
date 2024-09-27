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
public class UserController : ControllerBase
{
    private readonly IAppSettings _appSettings;

    public UserController(IAppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    /// POST: api/users/updateUser 
    /// <summary>
    /// Updates a user's details.
    /// </summary>
    /// <param name="request">The `User` object containing the updated user details.</param>
    /// <returns>An `ActionResult` containing the updated `User` object if successful, otherwise a `NotFound` result.</returns>
    [HttpPost("updateUser")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> UpdateUser([FromBody] User request)
    {
        if (request == null || request.UserName == null || request.Name == null)
        {
            return BadRequest("request body is required and must contain username and name");
        }

        var user = await CognitoActions.UpdateUser(request, _appSettings);

        if (user == null)
        {
            return NotFound("User not found");
        }

        return user;
    }

    // GET: api/cognito/getUserByName?username={username}
    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <returns>An `ActionResult` containing the `User` object if found, otherwise a `NotFound` result.</returns>
    [HttpGet("getUserByName")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUser(string username)
    {
        if (username == null)
        {
            return BadRequest("username is required");
        }

        var user = await CognitoActions.GetUser(username, _appSettings);

        if (user == null)
        {
            return NotFound("User not found");
        }

        return user;
    }

    // GET: api/cognito/getUserById?id={id}
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>An `ActionResult` containing the `User` object if found, otherwise a `NotFound` result.</returns>
    [HttpGet("getUserById")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserById(string id)
    {
        if (id == null)
        {
            return BadRequest("id is required");
        }

        var user = await CognitoActions.GetUserById(id, _appSettings);

        if (user == null)
        {
            return NotFound("User not found");
        }

        return user;
    }
}
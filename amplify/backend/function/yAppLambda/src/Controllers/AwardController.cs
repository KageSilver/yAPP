using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;

namespace yAppLambda.Controllers;

/// <summary>
/// The 'AwardController" class is an API controller in the 'yAppLamba project. 
/// It is responsible for handling HTTP requests related to award operations.
/// </summary>
[ApiController]
[Route("api/awards")]
public class AwardController : ControllerBase 
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;
    private readonly ICognitoActions _cognitoActions;
    private readonly IAwardActions _awardActions;

    public CommentController(IAppSettings appSettings, ICognitoActions cognitoActions, IDynamoDBContext dbContext, IAwardActions awardActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _awardActions = awardActions;
    }
}
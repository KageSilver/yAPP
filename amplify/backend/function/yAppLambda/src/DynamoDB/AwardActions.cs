using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.Enum;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class AwardActions : IAwardActions
{
    // This is the default table name for the award table
    private const string AwardTableName = "Award-test"; 
    private readonly string _awardTable;
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DynamoDBOperationConfig _config;

    public AwardActions(IAppSettings appSettings, IDynamoDBContext dynamoDbContext)
    {
        _appSettings = appSettings;
        _dynamoDbContext = dynamoDbContext;
        
        _awardTable = string.IsNullOrEmpty(_appSettings.PostTableName)
            ? AwardTableName
            : _appSettings.AwardTableName;
        
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _awardTable
        };
    }
}
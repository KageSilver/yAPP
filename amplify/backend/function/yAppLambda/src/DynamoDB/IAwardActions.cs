using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;
using yAppLambda.Common;

namespace yAppLambda.DynamoDB;

public interface IAwardActions
{
    /// <summary>
    /// Creates a new award
    /// </summary>
    /// <param name="award">The award object to be created.</param>
    /// <returns>An ActionResult containing the created award object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Award>> CreateAward(Award award);
    
    /// <summary>
    /// Deletes an award from the database by an award id
    /// </summary>
    /// <param name="aid">The id of the award to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    Task<bool> DeleteAward(string aid);
}
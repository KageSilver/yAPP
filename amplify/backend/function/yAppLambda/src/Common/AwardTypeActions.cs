using yAppLambda.Enum;

namespace yAppLambda.Common;

public class AwardTypeActions : IAwardTypeActions
{
    /// <summary>
    /// Converts an integer type code to the corresponding AwardType enum value.
    /// </summary>
    /// <param name="type">The integer type code (0: Upvotes, 1: Downvotes, 2: Comments).</param>
    /// <returns>The corresponding AwardType enum value.</returns>
    public AwardType GetAwardType(int type)
    {
        return type switch
        {
            0 => AwardType.Upvotes,
            1 => AwardType.Downvotes,
            2 => AwardType.Comments,
            _ => AwardType.All
        };
    }
}
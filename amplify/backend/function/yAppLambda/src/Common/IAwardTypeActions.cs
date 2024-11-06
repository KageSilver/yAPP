using yAppLambda.Enum;

namespace yAppLambda.Common;

public interface IAwardTypeActions
{
    /// <summary>
    /// Retrieves the award type based on the provided type code.
    /// </summary>
    /// <param name="type">The integer type code (0: Upvotes, 1: Downvotes, 2: Comments).</param>
    /// <returns>The corresponding <see cref="AwardType"/>.</returns>
    AwardType GetAwardType(int type);
}
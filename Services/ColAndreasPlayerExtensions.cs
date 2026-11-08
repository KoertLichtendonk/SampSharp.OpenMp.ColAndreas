using System.Numerics;
using SampSharp.Entities.SAMP;

namespace SampSharp.ColAndreas.Entities.Services;

/// <summary>
/// Player helpers ported from the legacy ColAndreas Pawn include (colandreas.inc). They are stocks built on the
/// ray casts, not exports of the component, so they live outside <see cref="IColAndreasService"/>.
/// </summary>
public static class ColAndreasPlayerExtensions
{
    /// <summary>
    /// True when a ray straight down from the player's height, <paramref name="dist"/> units in front of the
    /// player and <paramref name="height"/> units long, hits water first. Port of <c>CA_IsPlayerFacingWater</c>.
    /// </summary>
    public static bool IsPlayerFacingWater(this IColAndreasService colAndreas, Player player, float dist = 3.0f, float height = 3.0f)
    {
        var inFront = GetPointInFront(player, dist);
        var start = new Vector3(inFront, player.Position.Z);
        var end = new Vector3(inFront, player.Position.Z - height);
        return colAndreas.RayCastLineModel(start, end, out _) == ColAndreasConstants.WaterObject;
    }

    private static Vector2 GetPointInFront(Player player, float distance)
    {
        var angleInRadians = -player.Angle * MathF.PI / 180.0f;
        return new Vector2(
            player.Position.X + distance * MathF.Sin(angleInRadians),
            player.Position.Y + distance * MathF.Cos(angleInRadians));
    }
}

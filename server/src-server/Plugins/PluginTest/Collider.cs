using System;
using System.Numerics;

using MasterData;

namespace PluginTest
{
    internal static class Collider
    {
        public const float PlayerRadius = 0.5f;

        public static bool IsHit(Skill skill, Vector2 pos_xz, float angle_y, Vector2 target_pos)
        {
            var radius = PlayerRadius + skill.Radius;
            var radiusSquared = radius * radius;

            var pos = new Vector2(skill.PositionX, skill.PositionZ);
            var distance = pos.Length();
            var rad = angle_y * Math.PI / 180;
            rad += Math.Acos(skill.PositionZ / distance);
            pos_xz.X += distance * (float)Math.Sin(rad);
            pos_xz.Y += distance * (float)Math.Cos(rad);

            return Vector2.DistanceSquared(pos_xz, target_pos) <= radiusSquared;
        }

    }
}

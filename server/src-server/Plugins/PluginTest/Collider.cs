using System;
using System.Numerics;

namespace PluginTest
{
    static class Collider
    {
        public static float PlayerRadius = 0.5f;
        public static float BeamRadius = 1.5f;

        public static float Radius = PlayerRadius + BeamRadius;
        public static float RadiusSquared = Radius * Radius;

        public static bool IsHit(byte skillId, Vector2 pos_xz, float angle_y, Vector2 target_pos)
        {
            switch (skillId) {
                case 1:
                    var rad = angle_y * Math.PI / 180;
                    pos_xz.X += 2 * (float)Math.Sin(rad);
                    pos_xz.Y += 2 * (float)Math.Cos(rad);
                    return Vector2.DistanceSquared(pos_xz, target_pos) <= RadiusSquared;

                case 2:
                    return Vector2.DistanceSquared(pos_xz, target_pos) <= RadiusSquared;

                default:
                    return true;
            }
        }

    }
}

﻿using Rage;

namespace LtFlash.Common
{
    public struct SpawnPoint
    {
        public float Heading { get; set; }
        public Vector3 Position { get; set; }
        public static SpawnPoint Zero => new SpawnPoint(0.0f, Vector3.Zero);

        public SpawnPoint(float heading, Vector3 position)
        {
            Heading = heading;
            Position = position;
        }

        public SpawnPoint(float heading, float x, float y, float z)
            : this(heading, new Vector3(x, y, z))
        {
        }

        public static bool operator==(SpawnPoint s1, SpawnPoint s2)
        {
            return s1.Heading == s2.Heading && s1.Position == s2.Position;
        }

        public static bool operator !=(SpawnPoint s1, SpawnPoint s2)
        {
            return s1.Heading != s2.Heading && s1.Position != s2.Position;
        }

        public float Distance(SpawnPoint spawn)
            => Vector3.Distance(Position, spawn.Position);

        public float Distance2D(SpawnPoint spawn)
            => Vector3.Distance2D(Position, spawn.Position);   
    };
}

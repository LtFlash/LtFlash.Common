using Rage;

namespace LtFlash.Common
{
    public struct SpawnPoint
    {
        public float Heading { get; set; }
        public Vector3 Position { get; set; }

        public SpawnPoint(float heading, Vector3 position)
        {
            Heading = heading;
            Position = position;
        }

        public SpawnPoint(float heading, float x, float y, float z)
            : this(heading, new Vector3(x, y, z))
        {

        }

        public static SpawnPoint Zero
            { get { return new SpawnPoint(0.0f, Vector3.Zero); } }

        public float Distance(SpawnPoint spawn)
            => Vector3.Distance(Position, spawn.Position);
        
    };
}

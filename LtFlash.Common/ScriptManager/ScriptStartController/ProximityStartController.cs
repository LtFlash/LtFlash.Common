using Rage;

namespace ScriptManager.ScriptStartController
{
    public class ProximityStartController : IScriptStartController
    {
        private Vector3 _pos;
        private float _minDist;

        public ProximityStartController(Vector3 position, float minDistance)
        {
            _pos = position;
            _minDist = minDistance;
        }

        public bool CanBeStarted()
        {
            return Vector3.Distance(_pos, Game.LocalPlayer.Character.Position)
                <= _minDist;
        }
    }
}

using Rage;
using Rage.Native;

namespace LtFlash.Common.EvidenceLibrary.BaseClasses
{
    public abstract class EvidenceObject : EvidenceBase
    {
        //PUBLIC
        public override Vector3 Position 
            => _object ? _object.Position : Vector3.Zero;

        public override PoolHandle Handle
            => _object ? _object.Handle : new PoolHandle();

        protected override Entity EvidenceEntity => _object;

        //PROTECTED
        protected Object _object;

        public EvidenceObject(
            string id, string description, Model model, Vector3 position) 
            : base(id, description)
        {
            _object = new Object(model, position);

            PlaceOnGround(_object);

            ActivateStage(Process_PlaceOnGround); 
            //PlaceObjectOnGround(_object);

            //NativeFunction.Natives.SetEntityHasGravity(_object, true);
            //GameFiber.Sleep(3000);
            //_object.IsPositionFrozen = true;
        }

        private void Process_PlaceOnGround()
        {
            if(Vector3.Distance(PlayerPos, Position) < DistanceEvidenceClose)
            {
                PlaceOnGround(_object);
                DeactivateStage(Process_PlaceOnGround);
            }
        }

        private void PlaceObjectOnGround(Object obj)
            => NativeFunction.Natives.PlaceObjectOnGroundProperly(obj);

        private void PlaceOnGround(Object obj)
        {
            NativeFunction.Natives.RequestCollisionAtCoord(obj.Position.X, obj.Position.Y, obj.Position.Z);
            NativeFunction.Natives.SetEntityDynamic(obj, true);
            float? po = World.GetGroundZ(obj.Position, false, true);
            if (po.HasValue) obj.SetPositionZ(po.Value);
        }

        public override void Dismiss()
        {
            if(_object) _object.Dismiss();
            base.Dismiss();
        }

        public override bool IsValid()
        {
            return _object;
        }
    }
}

using Rage;
using Rage.Native;

namespace LtFlash.Common.EvidenceLibrary.BaseClasses
{
    public abstract class EvidenceObject : EvidenceBase
    {
        //PUBLIC
        public override Vector3 Position 
            => @object ? @object.Position : Vector3.Zero;

        public override PoolHandle Handle
            => @object ? @object.Handle : new PoolHandle();

        protected override Entity EvidenceEntity => @object;

        //PROTECTED
        public Object @object;

        public EvidenceObject(
            string id, string description, Model model, Vector3 position) 
            : base(id, description)
        {
            @object = new Object(model, position);

            PlaceOnGround(@object);

            ActivateStage(Process_PlaceOnGround); 
        }

        private void Process_PlaceOnGround()
        {
            if(Vector3.Distance(PlayerPos, Position) < DistanceEvidenceClose)
            {
                PlaceOnGround(@object);
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
            if(@object) @object.Dismiss();
            base.Dismiss();
        }

        public override bool IsValid() => @object;
    }
}

using Rage;
using Rage.Native;

namespace LtFlash.Common.EvidenceLibrary.BaseClasses
{
    public abstract class EvidenceObject : EvidenceBase
    {
        //PUBLIC
        public override Vector3 Position
            { get { return _object ? _object.Position : Vector3.Zero; } }

        public override PoolHandle Handle
            { get { return _object ? _object.Handle : new PoolHandle(); } }

        protected override Entity EvidenceEntity
            { get { return _object; } }

        //PROTECTED
        protected Object _object;

        public EvidenceObject(
            string id, string description, Model model, Vector3 position) 
            : base(id, description)
        {
            _object = new Object(model, position);

            PlaceObjectOnGround(_object);

            NativeFunction.Natives.SetEntityHasGravity(_object, true);
            GameFiber.Sleep(3000);
            _object.IsPositionFrozen = true;
        }

        private void PlaceObjectOnGround(Object obj)
            => NativeFunction.Natives.PlaceObjectOnGroundProperly(obj);

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

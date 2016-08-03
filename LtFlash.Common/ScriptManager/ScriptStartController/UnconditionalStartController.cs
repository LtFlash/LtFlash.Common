namespace ScriptManager.ScriptStartController
{
    public class UnconditionalStartController : IScriptStartController
    {
        public bool CanBeStarted() => true;
    }
}

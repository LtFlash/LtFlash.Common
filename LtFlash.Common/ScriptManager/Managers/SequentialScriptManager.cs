namespace ScriptManager.Managers
{
    public class SequentialScriptManager : ScriptManagerBase
    {
        public SequentialScriptManager() : base()
        {

        }

        protected override void Process()
        {
            for (int i = 0; i < _scripts.Count; i++)
            {
                if (_scripts[i].Processed) continue;

                if(_scripts[i].HasFinishedSuccessfully)
                {
                    //TODO: refactor!!!
                    StartScript(_scripts[i].NextScriptToRunIds[0], true);

                    _scripts[i].Processed = true;
                }
                else if(_scripts[i].HasFinishedUnsuccessfully)
                {
                    //restart the same script
                    _scripts[i].Start();
                }
            }
        }
    }
}

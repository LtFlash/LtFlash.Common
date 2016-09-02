namespace LtFlash.Common.ScriptManager.Scripts
{
    /// <summary>
    /// Common interface for different types of scripts.
    /// </summary>
    public interface IScript
    {
        bool IsRunning { get; }
        bool HasFinished { get; }
        bool Completed { get; }
        bool HasFinishedSuccessfully { get;}
        bool HasFinishedUnsuccessfully { get; }
        IScriptStatus Status { get; set; }

        bool CanBeStarted();
        void Start();
        void SetScriptFinished(bool completed);
    }
}

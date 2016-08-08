
using LtFlash.Common.Processes;

public class ProcessHostExample
{
    private ProcessHost _procHost = new ProcessHost();

    public ProcessHostExample()
    {
        //use names of functions as params to add processes
        //they'll be run inside a loop
        _procHost.AddProcess(CheckIfPlayerIsClose);
        _procHost.AddProcess(InitEntities);
        _procHost.AddProcess(StartMayhem);

        //turn on any REGISTERED function you like
        _procHost.ActivateProcess(CheckIfPlayerIsClose);
        
        //start processing your functions
        _procHost.Start();
    }

    private void CheckIfPlayerIsClose()
    {
        if(IsPlayerClose()) 
        {
            //use this function to change one active function to another
            _procHost.SwapProcesses(CheckIfPlayerIsClose, InitEntities);
        }
    }

    private void InitEntities()
    {
        //init your entities

        _procHost.SwapProcesses(InitEntities, StartMayhem);
    }


    private void StartMayhem()
    {
        //turn off a function from being called
        _procHost.DeactivateProcess(StartMayhem);
    }

    public void End()
    {
        //stop the internal loop of ProcessHost
        _procHost.Stop();
    }
}
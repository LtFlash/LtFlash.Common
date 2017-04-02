using LtFlash.Common.Processes;

public class ProcessHostExample
{
    private ProcessHost procHost = new ProcessHost();

    public ProcessHostExample()
    {
        //turn on any REGISTERED function you like
        procHost.ActivateProcess(CheckIfPlayerIsClose);
        //OR
        procHost[CheckIfPlayerIsClose] = true;
        
        //start processing your functions
        procHost.Start();
    }

    private void CheckIfPlayerIsClose()
    {
        if(IsPlayerClose()) 
        {
            //use this function to change one active function to another
            procHost.SwapProcesses(CheckIfPlayerIsClose, InitEntities);
        }
    }

    private void InitEntities()
    {
        //init your entities

        procHost.SwapProcesses(InitEntities, StartMayhem);
    }


    private void StartMayhem()
    {
        //turn off a function from being called
        procHost.DeactivateProcess(StartMayhem);
        //OR:
        procHost[StartMayhem] = false;
    }

    public void End()
    {
        //stop the internal loop of ProcessHost
        procHost.Stop();
    }
}
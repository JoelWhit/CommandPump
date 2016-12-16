namespace CommandPump.Contract
{
    public interface ICommandDispatch
    {
        void Dispatch<T>(T command) where T : ICommand;
    }
}

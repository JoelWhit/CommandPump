namespace CommandPump.Contract
{
    public interface ICommandDispatch
    {
        void RegisterHandler(ICommandHandler handler);

        void Dispatch<T>(T command) where T : ICommand;
    }
}

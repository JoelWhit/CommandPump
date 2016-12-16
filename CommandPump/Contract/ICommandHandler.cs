namespace CommandPump.Contract
{
    public interface ICommandHandler<T> : ICommandHandler where T : ICommand
    {
        void Handle(T Command);
    }

    public interface ICommandHandler
    {

    }
}

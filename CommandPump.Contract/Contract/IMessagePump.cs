namespace CommandPump.Contract
{
    public interface IMessagePump
    {
        /// <summary>
        /// Starts the message pump
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the message pump
        /// </summary>
        void Stop();
    }
}

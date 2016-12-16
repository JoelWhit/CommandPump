using System;
using System.Threading.Tasks;
using CommandPump.Contract;
using System.IO;

namespace CommandPump
{
    /// <summary>
    /// Processes received commands
    /// </summary>
    public class CommandPumpReceiver : MessagePump, ICommandPumpReceiver
    {

        public ITextSerializer Serializer { get; set; }
        public ICommandDispatch Dispatch { get; set; }

        public CommandPumpReceiver(IMessageReceiver receiver, ITextSerializer serilizer, ICommandDispatch commandDispatch, int maxDegreeOfParalism) : base(receiver, maxDegreeOfParalism)
        {
            Serializer = serilizer;
            Dispatch = commandDispatch;
        }

        public override void ProcessReceivedMessage(object Payload, string MessageId, string CorrelationId)
        {
            ICommand command = CreateCommand((Stream)Payload);
            Dispatch.Dispatch(command);
        }


        private ICommand CreateCommand(Stream messageStream)
        {
            object command;
            using (var stream = messageStream)
            using (var reader = new StreamReader(stream))
            {
                command = Serializer.Deserialize(reader);
            }

            return (ICommand)command;
        }
    }
}

using CommandPump.Contract;
using CommandPump.Extension;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommandPump
{
    /// <summary>
    /// Sends commands to the messaging queue
    /// </summary>
    public class CommandPumpSender : ICommandPumpSender
    {        
        public IMessageSender Sender { get; set; }

        public ITextSerializer Serializer { get; set; }

        public CommandPumpSender(IMessageSender sender, ITextSerializer serializer)
        {
            Sender = sender;
            Serializer = serializer;
        }

        public void Send(ICommand command)
        {
            Sender.Send(ConstructCommandStream(command));
        }

        public void Send(IEnumerable<ICommand> commands)
        {
            Sender.SendBatch(commands.Select(x => ConstructCommandStream(x)));
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            Sender.SendBatch(commands.Select(x => ConstructEnvelope(x, ConstructCommandStream(x.Body))));
        }

        public void Send(Envelope<ICommand> command)
        {
            Sender.Send(ConstructEnvelope(command, ConstructCommandStream(command.Body)));
        }


        private Stream ConstructCommandStream(ICommand command)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            Serializer.Serialize(writer, command);
            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Creates and sends a new envelope so that you are able to further interrogate the object after sending
        /// for whatever reason
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="commandStream"></param>
        /// <returns></returns>
        private Envelope<Stream> ConstructEnvelope<T>(Envelope<T> command, Stream commandStream)
        {
            var env = Envelope.Create(commandStream);
            env.CorrelationId = command.CorrelationId;
            env.Delay = command.Delay;
            env.MessageId = command.MessageId;
            env.TimeToLive = command.TimeToLive;

            return env;
        }
    }
}

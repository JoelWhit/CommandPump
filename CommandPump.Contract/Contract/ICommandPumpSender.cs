using CommandPump.Common;
using System.Collections.Generic;

namespace CommandPump.Contract
{
    public interface ICommandPumpSender
    {
        ITextSerializer Serializer { get; set; }

        IMessageSender Sender { get; set; }

        void Send(IEnumerable<ICommand> commands);

        void Send(ICommand command);

        void Send(IEnumerable<Envelope<ICommand>> commands);

        void Send(Envelope<ICommand> command);
    }
}

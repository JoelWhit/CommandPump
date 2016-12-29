using CommandPump.Contract;
using CommandPump.Common;
using System.IO;
using System.Messaging;

namespace CommandPump.Msmq
{
    public class MsmqMessageConverter : IMessageConverter
    {
        public Envelope<Stream> ConstructEnvelope(object message)
        {
            Message messageCasted = (Message)message;


            Envelope<Stream> msg = Envelope.Create(messageCasted.BodyStream);

            if (!string.IsNullOrWhiteSpace(messageCasted.Id))
            {
                msg.MessageId = messageCasted.Id;
            }

            try
            {
                msg.CorrelationId = messageCasted.CorrelationId;
            }
            catch //will throw an exception if correlationId is null
            {

            }

            return msg;
        }

        public object ConstructMessage(Envelope<Stream> envelope)
        {
            var message = new Message();
            message.BodyStream = envelope.Body;

            if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
            {
                message.CorrelationId = envelope.CorrelationId;
            }

            return message;
        }
    }
}

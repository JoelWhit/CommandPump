using CommandPump.Contract;
using CommandPump.Common;
using System.IO;
using System.Messaging;

namespace CommandPump.Msmq
{
    public static class MsmqMessageConverter 
    {
        public static Envelope<Stream> ConstructEnvelope(Message message)
        {
            Envelope<Stream> msg = Envelope.Create(message.BodyStream);

            if (!string.IsNullOrWhiteSpace(message.Id))
            {
                msg.MessageId = message.Id;
            }

            try
            {
                msg.CorrelationId = message.CorrelationId;
            }
            catch //will throw an exception if correlationId is null
            {

            }

            return msg;
        }

        public static Message ConstructMessage(Envelope<Stream> envelope)
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

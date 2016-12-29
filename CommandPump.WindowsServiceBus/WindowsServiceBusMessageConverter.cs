using CommandPump.Contract;
using CommandPump.Common;
using System;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace CommandPump.WindowsServiceBus
{
    public class WindowsServiceBusMessageConverter : IMessageConverter
    {
        public Envelope<Stream> ConstructEnvelope(object message)
        {
            BrokeredMessage messageCast = (BrokeredMessage)message;

            Envelope<Stream> msg = Envelope.Create(messageCast.GetBody<Stream>());

            if (!string.IsNullOrWhiteSpace(messageCast.MessageId))
            {
                msg.MessageId = messageCast.MessageId;
            }

            if (!string.IsNullOrWhiteSpace(messageCast.CorrelationId))
            {
                msg.CorrelationId = messageCast.CorrelationId;
            }

            if (messageCast.TimeToLive > TimeSpan.Zero)
            {
                msg.TimeToLive = messageCast.TimeToLive;
            }

            return msg;
        }

        public object ConstructMessage(Envelope<Stream> envelope)
        {
            BrokeredMessage message = new BrokeredMessage(envelope.Body, true);

            if (!string.IsNullOrWhiteSpace(envelope.MessageId))
            {
                message.MessageId = envelope.MessageId;
            }

            if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
            {
                message.CorrelationId = envelope.CorrelationId;
            }

            if (envelope.Delay > TimeSpan.Zero)
            {
                message.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(envelope.Delay);
            }

            if (envelope.TimeToLive > TimeSpan.Zero)
            {
                message.TimeToLive = envelope.TimeToLive;
            }

            return message;
        }
    }
}

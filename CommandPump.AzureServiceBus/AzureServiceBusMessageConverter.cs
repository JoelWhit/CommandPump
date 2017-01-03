using CommandPump.Contract;
using CommandPump.Common;
using System;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace CommandPump.AzureServiceBus
{
    public static class AzureServiceBusMessageConverter
    {
        public static Envelope<Stream> ConstructEnvelope(BrokeredMessage message)
        {
            Envelope<Stream> msg = Envelope.Create(message.GetBody<Stream>());

            if (!string.IsNullOrWhiteSpace(message.MessageId))
            {
                msg.MessageId = message.MessageId;
            }

            if (!string.IsNullOrWhiteSpace(message.CorrelationId))
            {
                msg.CorrelationId = message.CorrelationId;
            }

            if (message.TimeToLive > TimeSpan.Zero)
            {
                msg.TimeToLive = message.TimeToLive;
            }

            return msg;
        }

        public static BrokeredMessage ConstructMessage(Envelope<Stream> envelope)
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

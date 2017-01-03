using Amazon.SQS.Model;
using CommandPump.Common;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CommandPump.AmazonSQS
{
    public static class AmazonSQSMessageConverter
    {
        public static Envelope<Stream> ConstructEnvelope(Message message)
        {

            Envelope<Stream> envelope = Envelope.Create(GetStreamFromText(message.Body));

            if (!string.IsNullOrWhiteSpace(message.MessageId))
            {
                envelope.MessageId = message.MessageId;
            }

            //if (!string.IsNullOrWhiteSpace(message.CorrelationId))
            //{
            //    envelope.CorrelationId = message.CorrelationId;
            //}

            return envelope;
        }

        public static SendMessageBatchRequest ConstructMessage(IEnumerable<Envelope<Stream>> messages)
        {
            SendMessageBatchRequest request = new SendMessageBatchRequest();

            foreach (var message in messages)
            {
                request.Entries.Add(new SendMessageBatchRequestEntry((request.Entries.Count + 1).ToString(), GetTextFromStream(message.Body)));
            }

            return request;
        }

        public static SendMessageRequest ConstructMessage(Envelope<Stream> envelope)
        {
            SendMessageRequest message = new SendMessageRequest();

            message.MessageBody = GetTextFromStream(envelope.Body);

            //if (!string.IsNullOrWhiteSpace(envelope.MessageId))
            //{
            //    message.MessageId = envelope.MessageId;
            //}

            //if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
            //{
            //    message.CorrelationId = envelope.CorrelationId;
            //}

            if (envelope.Delay > TimeSpan.Zero)
            {
                message.DelaySeconds = DateTime.UtcNow.Add(envelope.Delay).Subtract(DateTime.UtcNow).Seconds;
            }

            //if (envelope.TimeToLive > TimeSpan.Zero)
            //{
            //    message.TimeToLive = envelope.TimeToLive;
            //}

            return message;
        }

        public static Stream GetStreamFromText(string body)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(body));
        }

        public static string GetTextFromStream(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream, true))
            {
                return sr.ReadToEnd();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandPump.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandPump.RabbitMQ
{
    public static class RabbitMQMessageConverter
    {
        public static Envelope<Stream> ConstructEnvelope(BasicGetResult message)
        {
            string bodyString = Encoding.Default.GetString(message.Body);
            byte[] bodyBytes = Encoding.Default.GetBytes(bodyString);
            Stream bodyStream = new MemoryStream(bodyBytes);

            Envelope<Stream> envelope = Envelope.Create(bodyStream);

            envelope.MessageId = message.BasicProperties.MessageId;
            envelope.CorrelationId = message.BasicProperties.CorrelationId;

            return envelope;
        }

        public static byte[] GetBytesFromStream(Stream body)
        {
            byte[] bytes;
            using (BinaryReader rdr = new BinaryReader(body))
            {
                bytes = rdr.ReadBytes((int)body.Length);
            }
            //byte[] test;
            //body.pos
            //using (StreamReader sr = new StreamReader(body, true))
            //{
            //    test = Encoding.Default.GetBytes(sr.ReadToEnd());
            //}

            return bytes;    
        }

        public static Envelope<Stream> ConstructEnvelope(BasicDeliverEventArgs e)
        {
            string bodyString = Encoding.Default.GetString(e.Body);
            byte[] bodyBytes = Encoding.Default.GetBytes(bodyString);
            Stream bodyStream = new MemoryStream(bodyBytes);

            Envelope<Stream> envelope = Envelope.Create(bodyStream);

            envelope.MessageId = e.BasicProperties.MessageId;
            envelope.CorrelationId = e.BasicProperties.CorrelationId;

            return envelope;
        }
    }
}

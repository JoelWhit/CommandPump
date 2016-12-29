using CommandPump.Common;
using System.IO;

namespace CommandPump.Contract
{
    /// <summary>
    /// Used to convert between the business layer and messaging implementation
    /// </summary>
    public interface IMessageConverter
    {
        Envelope<Stream> ConstructEnvelope(object message);

        object ConstructMessage(Envelope<Stream> envelope);
    }
}

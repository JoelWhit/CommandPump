using System.Collections.Generic;

namespace CommandPump.Contract
{
    public interface ICommandPumpReceiver
    {
        ITextSerializer Serializer { get; set; }

        ICommandDispatch Dispatch { get; set; }
    }
}

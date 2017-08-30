using System;

namespace Provausio.EventStore
{
    public class DirtyStreamException : Exception
    {
        public DirtyStreamException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

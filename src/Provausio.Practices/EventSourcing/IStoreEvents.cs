using System;
using System.Threading.Tasks;

namespace Provausio.Practices.EventSourcing
{
    public interface IStoreEvents : IDisposable
    {
        Task AppendEventsAsync(string streamName, params EventInfo[] infos);

        Task<EventStreamResult> GetEvents(string streamname, int startingPosition);

        Task<EventInfo> GetSnapshot(string streamName);

        Task Connect();

        void Disconnect();
    }
}
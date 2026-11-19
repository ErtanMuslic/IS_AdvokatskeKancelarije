using System.Threading.Channels;

namespace IS_AdvokatskeKancelarije.Services
{
    /// <summary>Red dokumenata koje treba indeksirati u pozadini.</summary>
    public class IndexingQueue
    {
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

        public void Enqueue(Guid documentId) => _channel.Writer.TryWrite(documentId);

        public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct) =>
            _channel.Reader.ReadAllAsync(ct);
    }
}

namespace IS_AdvokatskeKancelarije.Services
{
   
    public class IndexingWorker : BackgroundService
    {
        private readonly IndexingQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IndexingWorker> _logger;

        public IndexingWorker(
            IndexingQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<IndexingWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var documentId in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var indexer = scope.ServiceProvider.GetRequiredService<DocumentIndexer>();
                    await indexer.IndexAsync(documentId, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Indexing failed for document {DocumentId}", documentId);
                }
            }
        }
    }
}

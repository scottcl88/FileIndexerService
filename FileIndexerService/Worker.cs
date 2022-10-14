namespace FileIndexerService;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Indexer _indexer;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _indexer = new Indexer(logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FileIndexer ExecuteAsync started");
        await _indexer.Setup();

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Indexing running at: {time}", DateTimeOffset.Now);
            var result = await _indexer.IndexFiles();
            if (result)
            {
                var now = DateTime.Now;
                DateTime later;
                if(now.Hour >= 12)
                {
                    var nextDay = now.AddDays(1);
                    later = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
                }
                else
                {
                    later = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
                }
                var difference = later.Subtract(now);
                _logger.LogInformation("Indexing successful. Will rerun at: {time}", later);
                await Task.Delay(difference, stoppingToken);
            }
            else
            {
                _logger.LogError("Indexing failed, stopping. Time: {time}", DateTimeOffset.Now);
                await StopAsync(stoppingToken);
            }
          
        }
    }
}
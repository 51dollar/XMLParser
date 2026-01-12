using System.Collections.Concurrent;
using Shared.Models.Parser.XML;

namespace FileParserService.Processing;

public class ProcessedModelHandler
{
    private readonly ConcurrentQueue<InstrumentStatus> _queue = new();
    
    public void AddInEnqueue(InstrumentStatus model) => _queue.Enqueue(model);
    public bool TryGetNextModel(out InstrumentStatus? model) => _queue.TryDequeue(out model); 
}
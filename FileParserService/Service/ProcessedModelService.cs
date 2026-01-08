using System.Collections.Concurrent;
using Shared.Models.Parser.XML;

namespace FileParserService.Service;

public class ProcessedModelService
{
    private readonly ConcurrentQueue<InstrumentStatus> _queue = new();
    
    public void AddInEnqueue(InstrumentStatus model) => _queue.Enqueue(model);
    public IEnumerable<InstrumentStatus> GetAllModels() => _queue.ToArray();
    public bool TryGetNextModel(out InstrumentStatus? model) => _queue.TryDequeue(out model); 
}
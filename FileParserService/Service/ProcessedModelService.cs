using System.Collections.Concurrent;
using Shared.Models;

namespace FileParserService.Service;

public class ProcessedModelService
{
    private readonly ConcurrentQueue<ModelXmlParse> _queue = new();
    
    public void AddInEnqueue(ModelXmlParse model) => _queue.Enqueue(model);
    public IEnumerable<ModelXmlParse> GetAllModels() => _queue.ToArray();
    public bool TryGetNextModel(out ModelXmlParse? model) => _queue.TryDequeue(out model); 
}
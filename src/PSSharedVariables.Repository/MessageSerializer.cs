using Newtonsoft.Json;

namespace PSSharedVariables.Repository;

public sealed class MessageSerializer(JsonSerializerSettings serializationSettings)// : IMessageSerializer
{
    public string? Serialize(object? value) => value == null ? null : JsonConvert.SerializeObject(value, serializationSettings);
    public Task<string?> SerializeAsync(object? value, CancellationToken cancellationToken = default) => Task.Run(() => Serialize(value), cancellationToken);

    //public TResult? Deserialize<TResult>(string? value) => value == null ? default : JsonConvert.DeserializeObject<TResult>(value, serializationSettings);
    public object? Deserialize(string? value) => value == null ? default : JsonConvert.DeserializeObject(value, serializationSettings);
    //public Task<TResult?> DeserializeAsync<TResult>(string? value, CancellationToken cancellationToken = default) => Task.Run(() => Deserialize<TResult>(value), cancellationToken);
    public Task<object?> DeserializeAsync(string? value, CancellationToken cancellationToken = default) => Task.Run(() => Deserialize(value), cancellationToken);
}

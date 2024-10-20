//using MediatR;
//using Microsoft.PowerShell.Commands;
//using Newtonsoft.Json;
//using System.Collections.Concurrent;
//using System.IO.Pipes;
//using System.Management.Automation;

//namespace PSSharedVariables.PsModule;

//public sealed class VariableEventDispatcher(IObjectConverter converter) //(IMessageSerializer messageSerializer)
//{
//    private readonly ConcurrentQueue<INotification> events = new();

//    public void Send(INotification notification) => events.Enqueue(notification);

//    private void Send(string pipeName, INotification request)
//    {
//        var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
//        client.Connect(100);
//        using var reader = new StreamReader(client);
//        using var writer = new StreamWriter(client);

//        writer.WriteLine(SerializeMessage(request));
//        writer.Flush();
//        reader.ReadLine();
//    }

//    //private T? Send<T>(string pipeName, IVariableRequest request)
//    //{
//    //    var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
//    //    client.Connect(100);
//    //    using var reader = new StreamReader(client);
//    //    using var writer = new StreamWriter(client);

//    //    writer.WriteLine(SerializeMessage(request));
//    //    writer.Flush();
//    //    var result = reader.ReadLine();
//    //    return (T?)DeserializeMessage(result);
//    //}

//    private object DeserializePsObject(string value) => JsonObject.ConvertFromJson(value, out ErrorRecord error);
//    private object? DeserializeMessage(string value) => JsonConvert.DeserializeObject(value, new JsonSerializerSettings
//    {
//        TypeNameHandling = TypeNameHandling.All
//    });
//    private string SerializePsObject(object value) => JsonObject.ConvertToJson(value, new JsonObject.ConvertToJsonContext(2, false, true));
//    private string SerializeMessage(object value) => JsonConvert.SerializeObject(value, new JsonSerializerSettings
//    {
//        TypeNameHandling = TypeNameHandling.All
//    });
//}

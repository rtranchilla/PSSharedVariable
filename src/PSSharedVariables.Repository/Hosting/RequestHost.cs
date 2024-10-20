using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Pipes;
using System.Runtime.Versioning;

namespace PSSharedVariables.Repository.Hosting;

public sealed class RequestHost : IDisposable
{
    private bool isDisposed = false;
    private readonly NamedPipeServerStream stream;
    private readonly CancellationToken cancellationToken = default;
    private Task? taskCommunication;
    private readonly RequestHostPool hostPool;
    private readonly IServiceProvider serviceProvider;
    private readonly MessageSerializer messageSerializer;
    private readonly VariableRepository repository;

    public RequestHost(RequestHostPool hostPool, IServiceProvider serviceProvider,
        VariableRepository repository, string pipeName = "TestPipe", int maxNumberofHostInstances = 10)
    {
        this.hostPool = hostPool ?? throw new ArgumentNullException(nameof(hostPool));
        this.serviceProvider = serviceProvider;
        messageSerializer = messageSerializer;
        this.repository = repository;
        stream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, maxNumberofHostInstances, PipeTransmissionMode.Byte);
        var asyncResult = stream.BeginWaitForConnection(OnConnected, null);
    }

    internal TaskStatus? Status => taskCommunication?.Status;

    public void Dispose()
    {
        isDisposed = true;
        stream.Dispose();
    }

    private void OnConnected(IAsyncResult result)
    {
        if (!isDisposed)
        {
            stream.EndWaitForConnection(result);

            hostPool.Create();

            taskCommunication = Communication(stream, cancellationToken);
        }
    }

    public async Task Communication(NamedPipeServerStream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream);
        while (!reader.EndOfStream)
        {
            var serializedRequest = reader.ReadLine();

            if (!string.IsNullOrEmpty(serializedRequest))
            {
                var request = await messageSerializer.DeserializeAsync(serializedRequest, cancellationToken);

                ArgumentNullException.ThrowIfNull(request);

                var mediator = serviceProvider.GetRequiredService<IMediator>();

                if (request is INotification notification)
                    await mediator.Publish(notification, cancellationToken);
                else if (request is IRequest requestMessage)
                    await mediator.Send(requestMessage, cancellationToken);
                else if (request is VariableUpdateRequest updateRequest)
                {
                    var notificationResponse = await mediator.Send(updateRequest, cancellationToken);
                    var serializedResponse = await messageSerializer.SerializeAsync(notificationResponse, cancellationToken);
                    await writer.WriteLineAsync(serializedResponse?.ToArray(), cancellationToken);
                }
                //else if (request is IRequest<> requestMessage)

                //if (request is SharedVariableGetRequest get)
                //{
                //    string? result;
                //    if (get.Name == null)
                //        result = await messageSerializer.SerializeAsync(repository.Get().Select(i => new VariableValue(i.Key, i.Value)).ToArray(), cancellationToken);
                //    else
                //        result = await messageSerializer.SerializeAsync(new VariableValue(get.Name, repository.Get(get.Name)), cancellationToken);

                //    await writer.WriteLineAsync(result?.ToArray(), cancellationToken);
                //}
                //else if (request is SharedVariableExistsRequest exists)
                //{
                //    var result = await messageSerializer.SerializeAsync(repository.Exists(exists.Name), cancellationToken);
                //    await writer.WriteLineAsync(result?.ToArray(), cancellationToken);
                //}
                //else if (request is SharedVariableRemoveEvent remove)
                //{
                //    repository.Remove(remove.Name);
                //    await writer.WriteLineAsync(Array.Empty<char>(), cancellationToken);
                //}
                //else if (request is SharedVariableSetEvent set)
                //{
                //    //repository.Set(set.Name, set.Value);
                //    await writer.WriteLineAsync(Array.Empty<char>(), cancellationToken);
                //}
                //else
                //    throw new NotSupportedException();
                await writer.FlushAsync(cancellationToken);
            }
        }
    }
}

public sealed class RequestHostPool(IServiceProvider serviceProvider, int maxNumberofHostInstances = 10) : IDisposable
{
    private readonly List<RequestHost> streams = [];

    public void Dispose() => CleanStreams(true);

    internal void Create()
    {
        if (streams.Count < maxNumberofHostInstances)
            streams.Add(ActivatorUtilities.CreateInstance<RequestHost>(serviceProvider));

        CleanStreams(false);
    }


    /// <summary>
    /// A routine to clean NamedPipeServerInstances. When disposeAll is true,
    /// it will dispose all server instances. Otherwise, it will only dispose
    /// the instances that are completed, canceled, or faulted.
    /// PS: disposeAll is true only for this.Dispose()
    /// </summary>
    /// <param name="disposeAll"></param>
    private void CleanStreams(bool disposeAll)
    {
        if (disposeAll)
        {
            foreach (var server in streams)
                server.Dispose();
        }
        else
        {
            for (int i = streams.Count - 1; i >= 0; i--)
            {
                if (streams[i] == null)
                {
                    streams.RemoveAt(i);
                }
                else if (streams[i].Status != null &&
                    (streams[i].Status == TaskStatus.RanToCompletion ||
                    streams[i].Status == TaskStatus.Canceled ||
                    streams[i].Status == TaskStatus.Faulted))
                {
                    streams[i].Dispose();
                    streams.RemoveAt(i);
                }
            }
        }
    }
}
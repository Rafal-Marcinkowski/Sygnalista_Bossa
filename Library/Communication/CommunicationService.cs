using Library.Events;
using Library.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Library.Communication;

public class CommunicationService(IEventAggregator eventAggregator) : ICommunicationService
{
    private readonly IEventAggregator eventAggregator = eventAggregator;
    private bool isListening = true;
    private readonly int sourcePort = 12348;

    public async Task ListenAsync()
    {
        TcpListener listener = new(IPAddress.Any, sourcePort);
        listener.Start();

        try
        {
            while (isListening)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                NetworkStream stream = client.GetStream();
                byte[] data = new byte[1024];
                int bytesRead = await stream.ReadAsync(data);
                string jsonString = Encoding.UTF8.GetString(data, 0, bytesRead);
                var payload = JsonSerializer.Deserialize<CommunicationPayload>(jsonString);
                client.Close();
                eventAggregator.GetEvent<CommunicationEvent>().Publish(payload);
            }
        }

        finally
        {
            listener.Stop();
        }
    }

    public async Task SendMessageAsync(string message, int port)
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        using TcpClient client = new(ipAddress.ToString(), port);
        NetworkStream stream = client.GetStream();

        var payload = new CommunicationPayload
        {
            Message = message,
            Port = sourcePort
        };

        string serializedPayload = JsonSerializer.Serialize(payload);
        byte[] data = Encoding.UTF8.GetBytes(serializedPayload);
        await stream.WriteAsync(data);
    }

    public void StopListening()
    {
        isListening = false;
    }
}

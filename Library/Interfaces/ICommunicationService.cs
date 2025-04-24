namespace Library.Interfaces;

public interface ICommunicationService
{
    Task ListenAsync();
    Task SendMessageAsync(string message, int port);
    void StopListening();
}
using Azure.Storage.Queues;
using System.Text;

public class QueueService
{
    private readonly QueueClient _queueClient;

    public QueueService(string connectionString, string queueName)
    {
        _queueClient = new QueueClient(connectionString, queueName);
        _queueClient.CreateIfNotExists(); // ensures queue exists
    }

    public async Task SendMessageAsync(string message)
    {
        if (_queueClient == null)
            throw new ArgumentNullException(nameof(_queueClient));

        await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
    }
}

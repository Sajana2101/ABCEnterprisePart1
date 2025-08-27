namespace ABCEnterprisePart1.Services;

using Azure.Storage.Queues;
using System.Text;

public class QueueService
{
    private readonly QueueClient _queueClient;
    public QueueService(string connectionString, string queueName)
    {
        _queueClient = new QueueClient(connectionString, queueName);
        _queueClient.CreateIfNotExists();
    }

    public async Task SendMessageAsync(string message)
    {
        await _queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
    }
}
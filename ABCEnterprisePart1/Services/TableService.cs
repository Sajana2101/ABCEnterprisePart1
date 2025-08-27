namespace ABCEnterprisePart1.Services;

using Azure.Data.Tables;

public class TableService
{
    private readonly TableServiceClient _serviceClient;
    public TableService(string connectionString)
    {
        _serviceClient = new TableServiceClient(connectionString);
    }

    public TableClient GetTableClient(string tableName)
    {
        return _serviceClient.GetTableClient(tableName);
    }
}
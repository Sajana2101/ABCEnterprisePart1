using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCEnterprisePart1.Models
{
    public class Customer: ITableEntity
    {
        public string? PartitionKey { get; set; } = "Customers";
        public string? RowKey { get; set; } 
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }


    }
}

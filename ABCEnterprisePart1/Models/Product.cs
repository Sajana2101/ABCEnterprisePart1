using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCEnterprisePart1.Models
{
    public class Product: ITableEntity
    {


        public string? PartitionKey{ get; set; } = "Products"; 
        public string? RowKey { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string ? ImageURL { get; set; }  
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }


    }
}

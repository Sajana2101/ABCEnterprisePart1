using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCEnterprisePart1.Models
{
    public class Product: ITableEntity
    {


        public string? PartitionKey{ get; set; } = "Products"; // Logical group
        public string? RowKey { get; set; } // Unique Product ID (string)
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        //public int ? Quantity { get; set; }
        public string ? ImageURL { get; set; }  

        // Required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }


    }
}

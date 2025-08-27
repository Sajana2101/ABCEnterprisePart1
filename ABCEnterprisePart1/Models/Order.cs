using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABCEnterprisePart1.Models
{
    public class Order : ITableEntity
    {
        public string? PartitionKey { get; set; } = "Orders";
        public string? RowKey { get; set; } // Unique Order ID
       
     //   public DateTime OrderDate { get; set; }

        //public string? CustomerID { get; set; } // Store as string to match RowKey from CustomerEntity
        public string? CustomerName { get; set; } // Optional for easy display
        //public string? ProductID { get; set; } // Store as string to match RowKey from ProductEntity
        public string? ProductName { get; set; } // Optional for easy display

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }



    }
}

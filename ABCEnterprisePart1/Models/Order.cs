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
        public string? RowKey { get; set; } 

        public string? CustomerName { get; set; } 
       
        public string? ProductName { get; set; } 

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }



    }
}

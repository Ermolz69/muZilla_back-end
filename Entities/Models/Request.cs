using Azure;
using Azure.Data.Tables;

namespace muZilla.Entities.Models
{
    public class Request : ITableEntity
    {
        public int RequesterId { get; set; }
        public int ReceiverId { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public Request()
        {
            PartitionKey = "Request";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
            ETag = new ETag();
        }
    }
}

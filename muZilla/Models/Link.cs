using Azure;
using Azure.Data.Tables;

namespace muZilla.Models
{
    public class Link : ITableEntity
    {
        public string LinkValue { get; set; }
        public int UserId { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public Link()
        {
            PartitionKey = "Link";
            RowKey = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
            ETag = new ETag();
        }
    }
}

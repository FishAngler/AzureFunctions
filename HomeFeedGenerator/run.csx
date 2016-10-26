#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;

public async static Task Run(
    PostSummary post,
    IAsyncCollector<DynamicTableEntity> homeFeedTable, 
    TraceWriter log)
{
    log.Info($"C# ServiceBus queue trigger function processed message: {post}");
    log.Info($"C# ServiceBus queue trigger function processed message: {post.PostId} {post.BodyOfWater}");
}

public class PostSummary
{
	public string PostId { get; set; }
	public PostType PostType { get; set; } // [C|R|L]
	public long CreationDate { get; set; }
	public string UserId { get; set; }
	public string BodyOfWater { get; set; }
	public string FishSpecie { get; set; }
}

public enum PostType
{
    Catch = 4,
    Report = 6,
    Location = 5
}
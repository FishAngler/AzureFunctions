#r "Microsoft.WindowsAzure.Storage"
#r "System.Runtime"

using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;

public async static Task Run(
    PostSummary post,
    IAsyncCollector<DynamicTableEntity> homeFeedTable, 
    TraceWriter log)
{
    log.Info($"C# ServiceBus queue trigger function processed message: {post.PostId} {post.BodyOfWater}");

    string _dbId = ConfigurationManager.AppSettings["DocDBId"];
    string _collName = "Follow";

    log.Info($"dbId = {_dbId}");
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
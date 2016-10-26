#r "Microsoft.WindowsAzure.Storage"
#r "System.Runtime"

using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Documents.Client;

public async static Task Run(
    PostSummary post,
    IAsyncCollector<DynamicTableEntity> homeFeedTable, 
    TraceWriter log)
{
    log.Info($"C# ServiceBus queue trigger function processed message: {post.PostId} {post.BodyOfWater}");

    string _dbId = ConfigurationManager.AppSettings["DocDBId"];
    string _collName = "Follow";

    log.Info($"dbId = {_dbId}");
    
    var collLink = UriFactory.CreateDocumentCollectionUri(_dbId, _collName).ToString();

    log.Info($"collLink = {collLink}");
    
    var followables = new List<string>();
    followables.Add(post.UserId);
    if (!string.IsNullOrWhiteSpace(post.BodyOfWater)) followables.Add(post.BodyOfWater);
    if (!string.IsNullOrWhiteSpace(post.FishSpecie)) followables.Add(post.FishSpecie);
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
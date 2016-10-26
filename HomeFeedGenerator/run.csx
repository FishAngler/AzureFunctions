#r "Microsoft.WindowsAzure.Storage"
#r "System.Runtime"
#r "Newtonsoft.Json"

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

private static readonly long _maxTicks = DateTime.MaxValue.Ticks;

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

    var usersIds = await GetFollowersAsync(followables, collLink).ConfigureAwait(false);
    usersIds.Add(post.UserId); //TO include the post in the HomeFeed of the own user
    
    foreach (var userId in usersIds) {
        log.Info($"userId = {userId}");
    }

    if (usersIds.Count == 1)
    {
        var dict = new Dictionary<string, string>() { { "userIds", string.Join(",", usersIds) } };
        log.Info($"PostFeedUpdater.ProcessAsync userIds.Count: {usersIds.Count} for Post: {post.PostId}");
    }

    await AddPostToHomeAndUserFeedsAsync(
        post, 
        usersIds, 
        //userFeedsTable, 
        homeFeedTable)
            .ConfigureAwait(false);

    log.Info("All done");
}

public static async Task<IList<string>> GetFollowersAsync(IList<string> followables, string collLink)
{
    var documentEndpoint = ConfigurationManager.AppSettings["DocDBEndpoint"];
    var authorizationKey = ConfigurationManager.AppSettings["DocDBKey"];

    FeedOptions docDbFeedOptions = new FeedOptions { EnableLowPrecisionOrderBy = true, MaxItemCount = 1000, EnableCrossPartitionQuery = true };
    
    var client = new DocumentClient(
                        new Uri(documentEndpoint), 
                        authorizationKey,
                        new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp },
                        ConsistencyLevel.Eventual);

    //Microsoft.Azure.Document.Client
    var qq = $"select value c.{nameof(Follow.UserId)} from c where c.{nameof(Follow.FollowableId)} IN ({string.Join(",", followables.Select(f => $"'{f}'"))})";
    var q = client.CreateDocumentQuery<string>(collLink, qq, docDbFeedOptions).AsDocumentQuery();

    var r = await QueryCollectionAsync(q).ConfigureAwait(false);

    var rt = r.Distinct().ToList();

    return rt;
}

public static async Task<IList<T>> QueryCollectionAsync<T>(IDocumentQuery<T> query)
{
    var result = new List<T>();

    while (query.HasMoreResults)
    {
        var qr = await SafeExecute(() => query.ExecuteNextAsync<T>());
        if (qr.Any()) result.AddRange(qr.ToList());
    }

    return result;
}

public static async Task<T> SafeExecute<T>(Func<Task<T>> command)
{
    while (true)
    {
        try
        {
            return await command.Invoke().ConfigureAwait(false);
        }
        catch (DocumentClientException ex)
            when ((int)ex.StatusCode == 429 || (int)ex.StatusCode == 449 ||
                    ex.StatusCode == HttpStatusCode.RequestTimeout ||
                    ex.StatusCode == HttpStatusCode.PreconditionFailed ||
                    ex.Error.Message.StartsWith("One of the specified pre-condition is not met") ||
                    ex.Error.Message.Contains("Conflicting request to resource has been attempted. Retry to avoid conflicts."))
        {
            if (ex.RetryAfter != null) await Task.Delay(ex.RetryAfter).ConfigureAwait(false);
        }
    }
}

public static Task AddPostToHomeAndUserFeedsAsync(
    PostSummary post, 
    IEnumerable<string> usersIds,
    //IAsyncCollector<DynamicTableEntity> userFeedsTable,
    IAsyncCollector<DynamicTableEntity> homeFeedTable)
{
    var feedProps = new Dictionary<string, EntityProperty> {
        { nameof(PostSummary.PostId), new EntityProperty(post.PostId) },
        { nameof(PostSummary.PostType), new EntityProperty((int)post.PostType) }};

    var rk = string.Format("{0:D19}", _maxTicks - post.CreationDate) + "_" + post.PostId + "_" + post.PostType;

    var tasks = new List<Task>();

    //var item = new DynamicTableEntity(partitionKey: post.UserId, rowKey: rk, properties: feedProps, etag: null);
    //tasks.Add(userFeedsTable.AddAsync(item));

    foreach (var userId in usersIds.Distinct())
    {
        var item = new DynamicTableEntity(partitionKey: userId, rowKey: rk, properties: feedProps, etag: null);
        tasks.Add(homeFeedTable.AddAsync(item));
    }

    return Task.WhenAll(tasks);
}

public class Follow
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [Required]
    public string UserId { get; set; }
    [Required]
    public string FollowableId { get; set; } //Id of followable Entity: Angler, FishSpecie, BodyOfWater
    [Required]
    public EntityType FollowableType { get; set; }

    public bool IsAutomatic { get; set; }
    [Required]
    public long CreationDate { get; set; }
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
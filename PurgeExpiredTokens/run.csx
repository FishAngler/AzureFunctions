using System;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");        
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetEnvironmentVariable("AzureStorageConnectionString"));
    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
    CloudTable table = tableClient.GetTableReference(GetEnvironmentVariable("AuthTokensTableName")); 

    TableQuery<LoginRefreshToken> query = 
        new TableQuery<LoginRefreshToken>()
        .Where(TableQuery.GenerateFilterCondition("ExpiresUtc", QueryComparisons.LessThan, DateTime.UtcNow.ToShortDateString()));

    IEnumerable<LoginRefreshToken> expiredTokens = table.ExecuteQuery(query);

    TableBatchOperation batchOperation = new TableBatchOperation();

    foreach (LoginRefreshToken entity in expiredTokens)
    {
        batchOperation.Delete(entity);
    }

    table.ExecuteBatch(batchOperation);

}

public class LoginRefreshToken : TableEntity
{
    public string PartitionKey {get;set;}
    public string RowKey {get;set;}

	public string Id { get; set; }
	public string UserId { get; set; }
	public string Subject { get; set; }
	public string ClientId { get; set; }
	public DateTime IssuedUtc { get; set; }
	public DateTime ExpiresUtc { get; set; }
	public string ProtectedTicket { get; set; }
	public bool Enabled { get; set; } = true;
}

public static string GetEnvironmentVariable(string name)
{
    return name + ": " + 
        System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}
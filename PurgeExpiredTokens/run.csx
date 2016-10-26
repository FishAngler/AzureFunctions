#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;

public static void Run(string myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");        

    log.Info($"** Connection String: {ConfigurationManager.AppSettings["AzureStorageConnectionString"]}");
    log.Info($"** Table Name: {ConfigurationManager.AppSettings["AuthTokensTableName"]}");

    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
    CloudTable table = tableClient.GetTableReference(ConfigurationManager.AppSettings["AuthTokensTableName"]); 

    TableQuery<LoginRefreshToken> query = 
        new TableQuery<LoginRefreshToken>()
        .Where(TableQuery.GenerateFilterConditionForDate("ExpiresUtc", QueryComparisons.LessThan, DateTimeOffset.UtcNow));

    IEnumerable<LoginRefreshToken> expiredTokens = table.ExecuteQuery(query);
    log.Info($"*** Table Length: {expiredTokens.Count()} ***");  

    int deleteCount = 0;
    foreach (LoginRefreshToken entity in expiredTokens)
    {        
        TableOperation deleteOperation = TableOperation.Delete(entity);
        table.Execute(deleteOperation);
        deleteCount++;
        log.Info($"*** Deleting Entity: {entity.ExpiresUtc} ***");  
    }

    log.Info($"*** A total of {deleteCount} records deleted. ***");

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


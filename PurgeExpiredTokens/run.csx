using System;
using Microsoft.Azure;
#r Microsoft.WindowsAzure.Storage

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");        
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(GetEnvironmentVariable("AzureStorageConnectionString"));
    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
    CloudTable table = tableClient.GetTableReference(GetEnvironmentVariable("AuthTokensTableName")); 

    TableQuery<LoginRefreshToken> query = new TableQuery<LoginRefreshToken>().Where(TableQuery.GenerateFilterCondition("ExpiresUtc", QueryComparisons.LessThan, DateTime.Now));

    List<LoginRefreshToken> expiredTokens = table.ExecuteQuery(query);

    TableBatchOperation batchOperation = new TableBatchOperation();

    foreach (LoginRefreshToken entity in expiredTokens)
    {
        batchOperation.delete(entiry)
    }

    table.ExecuteBatch(batchOperation);

}

public static string GetEnvironmentVariable(string name)
{
    return name + ": " + 
        System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}
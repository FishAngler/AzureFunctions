#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

public static async Task Run(HttpRequestMessage req, TraceWriter log)
{
    //Get data from req
    string myData = await req.Content.ReadAsStringAsync();
    Catch receivedCatchRecord = JsonConvert.DeserializeObject<Catch>(myData);
    string catchId = receivedCatchRecord.id;
    List<string> mediaUris = receivedCatchRecord.Media.Select(media => media.MediaUri).ToList();
    //Get Data from temp DB
    //*********** DOC DB ********************/

    string EndpointUri = ConfigurationManager.AppSettings["DocDBEndpoint"];
    string PrimaryKey = ConfigurationManager.AppSettings["DocDBKey"];
    string databaseName = "fishangler";
    string collectionName = "HackFestFaces";
    DocumentClient client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

    // Set some common query options
    FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

    var myQuery = client.CreateDocumentQuery<MediaMetaInfo>(
            UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
            .Where(mediaMetaInfo => mediaMetaInfo.CatchId == catchId)
            .Select(e => e).AsDocumentQuery();

    // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
    log.Info("Running LINQ query...");

    var mediaMetaInfoRecords = (await myQuery.ExecuteNextAsync<MediaMetaInfo>()).SingleOrDefault();
    
    if(mediaMetaInfoRecords != null){
        log.Info("Documents Found...");
    }
    //Update the record in catch DB

}

public class Catch
{
    public string UserId { get; set; }
    public long CreationDate { get; set; }
    public Fishspecies FishSpecies { get; set; }
    public string Description { get; set; }
    public string id { get; set; }
    public IList<MediaBlob> Media { get; set; }
}

public class Fishspecies
{
    public string Id { get; set; }
    public string Description { get; set; }
}

public class MediaBlob
{
    public string MediaUri { get; set; }
    public IdDescription<int> MediaType { get; set; }
    public string PreviewUri { get; set; }
    public MediaSize Size { get; set; }

    public List<Face> Faces { get; set; }
}

public class IdDescription<T>
{
    public T Id { get; set; }
    public string Description { get; set; }
}

public class MediaSize
{
    public int Width { get; set; }
    public int Height { get; set; }
}

public class MediaMetaInfo {
    public string CatchId { get; set; }
    public string MediaUri { get; set; }
    public List<Face> Faces {get;set;}
}

public class Face
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Loc_x { get; set; }
    public int Loc_y { get; set; }
    public string UserId { get; set; }
}


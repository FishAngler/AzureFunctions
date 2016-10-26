#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    //Retrieve Object and Convert
    string myData = await req.Content.ReadAsStringAsync();
    FaceResult fr = new FaceResult();
    fr = JsonConvert.DeserializeObject<FaceResult>(myData);

    //Debugging 
    log.Info($"Retrieving Document: {fr.DocumentId}");
    foreach(Face f in fr.Faces){
        log.Info($"Width: {f.Width}");
    }

    //Doc DB
    string EndpointUri = ConfigurationManager.AppSettings["DocDBEndpoint"];
    string PrimaryKey = ConfigurationManager.AppSettings["DocDBKey"];
    DocumentClient client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

    await this.ExecuteSimpleQuery("fishangler", "HackFestFaces", fr);

    return req.CreateResponse(HttpStatusCode.Created);
}

private async Task ExecuteSimpleQuery(string databaseName, string collectionName, FaceResult faceData)
{
    // Set some common query options
    FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

    // Here we find the Andersen family via its LastName
    var myQuery = this.client.CreateDocumentQuery<Catch>(
            UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
            .Where(c => c.id == faceData.DocumentId)
            .Select(e => e).AsDocumentQuery();

    // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
    log.Info("Running LINQ query...");

    var myCatch = (await myQuery.ExecuteNextAsync<Catch>()).SingleOrDefault();
    
    if(myCatch != null)
        log.Info("Document Found...");

    if (myCatch.Media != null)
    {
        foreach (var media in myCatch.Media)
        {
            media.Faces = new List<Face>();
            foreach (Face f in faceData.Faces)
            {
                media.Faces.Add(f);
            }                    
        }

        await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, myCatch.id), myCatch);
    }    

    log.Info("Update Completed");
}

public class FaceResult {
    public string DocumentId { get; set; }
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
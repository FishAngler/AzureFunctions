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

    //*********** DOC DB ********************/

    string EndpointUri = ConfigurationManager.AppSettings["DocDBEndpoint"];
    string PrimaryKey = ConfigurationManager.AppSettings["DocDBKey"];
    string databaseName = "fishangler";
    string collectionName = "Catch";
    DocumentClient client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

    // Set some common query options
    FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

    // Here we find the Andersen family via its LastName
    var myQuery = client.CreateDocumentQuery<Catch>(
            UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
            .Where(c => c.id == fr.CatchId)
            .Select(e => e).AsDocumentQuery();

    // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
    log.Info("Running LINQ query...");

    var myCatch = (await myQuery.ExecuteNextAsync<Catch>()).SingleOrDefault();
    
    if(myCatch != null)
        log.Info("Document Found...");

    if (myCatch.Media != null)
    {

        var myMediaToUpdate = myCatch.Media
            .Where(m => m.MediaUri == fr.MediaUri).SingleOrDefault();
        
        if (myMediaToUpdate != null)
        {
            MediaBlob myMediaUpdated = new MediaBlob();
            myMediaUpdated = myMediaToUpdate;

            myMediaUpdated.Faces = new List<Face>();
            foreach (Face f in faces)
            {
                myMediaUpdated.Faces.Add(f);
            }
            myCatch.Media.Remove(myMediaToUpdate);
            myCatch.Media.Add(myMediaUpdated);
        }

        await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, myCatch.id), myCatch);
    }    

    log.Info("Update Completed");

    //*********** END DB ********************/

    return req.CreateResponse(HttpStatusCode.Created);
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

public class FaceResult {
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
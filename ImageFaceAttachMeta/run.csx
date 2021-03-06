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
    log.Info($"CatchId: {fr.CatchId}");
    log.Info($"MediaUri: {fr.MediaUri}");
    foreach(Face f in fr.Faces){
        log.Info($"Width: {f.Width}");
        log.Info($"Height: {f.Height}");
    }

    /*********** DocumentDb Settings ********************/
    string EndpointUri = ConfigurationManager.AppSettings["DocDBEndpoint"];
    string PrimaryKey = ConfigurationManager.AppSettings["DocDBKey"];
    string databaseName = ConfigurationManager.AppSettings["DocDBId"];
    string collectionName = ConfigurationManager.AppSettings["DocDBCollection"];

    // Create Client
    DocumentClient client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

    // Set some common query options
    FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

    // Search for Catch by supplied CatchId
    var myQuery = client.CreateDocumentQuery<Catch>(
            UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
            .Where(c => c.id == fr.CatchId)
            .Select(e => e).AsDocumentQuery();

    log.Info("Running LINQ query...");

    // Grab the Catch
    var myCatch = (await myQuery.ExecuteNextAsync<Catch>()).SingleOrDefault();
    
    if(myCatch != null)
        log.Info("Document Found...");

    // TODO: Fail Gracefully

    // Attach Face Meta Data if Media is not Null
    if (myCatch.Media != null)
    {

        var myMediaToUpdate = myCatch.Media
            .Where(m => m.MediaUri == fr.MediaUri).SingleOrDefault();
        
        if (myMediaToUpdate != null)
        {
            MediaBlob myMediaUpdated = myMediaToUpdate;

            myMediaUpdated.Faces = new List<Face>();
            foreach (Face f in fr.Faces)
            {
                myMediaUpdated.Faces.Add(f);
                log.Info("Adding Face...");
            }
            myCatch.Media.Remove(myMediaToUpdate);
            log.Info("Removing Record...");
            myCatch.Media.Add(myMediaUpdated);
            log.Info("Updating Media Record...");
        }

        await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, myCatch.id), myCatch);
    }    

    log.Info("Update Completed");

    /*********** END DocumentDb ********************/

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
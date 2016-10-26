#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    string myData = await req.Content.ReadAsStringAsync();
    FaceResult fr = new FaceResult();
    fr = JsonConvert.DeserializeObject<FaceResult>(myData);

    log.Info($"Retrieving Document: {fr.DocumentId}");

    foreach(Face f in fr.Faces){
        log.Info($"Width: {f.Width}");
    }

    // Doc DB
    string EndpointUri = ConfigurationManager.AppSettings["DocDBEndpoint"];
    string PrimaryKey = ConfigurationManager.AppSettings["DocDBKey"];
    DocumentClient client;

    return req.CreateResponse(HttpStatusCode.Created);
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
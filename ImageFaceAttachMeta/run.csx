#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    FaceResult fr = new FaceResult();
    fr = await JsonConvert.DeserializeObjectAsync<FaceResult>(req);

    //dynamic data = await req.Content.ReadAsAsync<FaceResult>();
    //string name = data?.name;

    // if (name == null)
    // {
    //     return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name in the request body");
    // }

    foreach(Face f in FaceResult.Faces){
        log.Info($"{f.Width}");
    }

    return req.CreateResponse(HttpStatusCode.Created);
}

public class FaceResult {
    public List<Face> Faces {get;set;}
}

public class Face
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Loc_x { get; set; }
    public int Loc_y { get; set; }
}
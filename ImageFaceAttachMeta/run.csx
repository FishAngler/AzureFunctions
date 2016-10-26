#r "Newtonsoft.Json"
using System.Net;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    //List<Face> = new List<Face>();

    dynamic data = await req.Content.ReadAsAsync<Faces>();
    //string name = data?.name;

    // if (name == null)
    // {
    //     return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name in the request body");
    // }

    foreach(Face f in data){
        log.Info($"{f.Width}");
    }

    return req.CreateResponse(HttpStatusCode.Created);
}

public class Faces {
    public List<Face> Faces {get;set;}
}

public class Face
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Loc_x { get; set; }
    public int Loc_y { get; set; }
}
using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    dynamic data = await req.Content.ReadAsAsync<object>();
    string name = data?.name;

    if (name == null)
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name in the request body");
    }

    return req.CreateResponse(HttpStatusCode.Created);
}

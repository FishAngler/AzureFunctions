#r "System.Runtime"
#r "System.Threading.Tasks"
#r "System.IO"
#r "System.Linq"
#r "Newtonsoft.Json"

using System;
using System.Configuration;

using System.Runtime;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face; 
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using System.Net;
using System.Text;

public static void Run(Stream inputBlob, string blobname, string catchId, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{blobname} \n Size: {inputBlob.Length} Bytes");

    IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["CognitiveServiceAPIKey"]);

    var recognizedFaces = faceServiceClient.DetectAsync(inputBlob).Result;

    var faceRects = recognizedFaces.Select(face => face.FaceRectangle);

    log.Info($"Faces: {faceRects.Count()}");

    Face[] faces = faceRects.Select(faceRect => new Face(
        faceRect.Width, 
        faceRect.Height, 
        faceRect.Left, 
        faceRect.Top)).ToArray();

    FacesContainer fc = new FacesContainer(catchId, $"blob/catch/{catchId}/{blobname}", faces);

    log.Info($"{fc.MediaUri}");

    WebRequest request = WebRequest.Create("https://dev-af-eus-fa-001.azurewebsites.net/api/ImageFaceAttachMeta?code=udm1to2ia8fwpccxyzivhd7vi4shyjo4zf02v93aw4tajpgmn29qn43vmdu32717doonrs2x1or");
    
    request.Method = "POST";

    string postData = JsonConvert.SerializeObject(fc);
    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
    request.ContentType = "application/json";
    request.ContentLength = byteArray.Length;

    Stream dataStream = request.GetRequestStream();
    dataStream.Write(byteArray, 0, byteArray.Length);
    dataStream.Close();

    WebResponse response = request.GetResponse();
    //log.info(((HttpWebResponse)response).StatusDescription);
    dataStream = response.GetResponseStream();
    StreamReader reader = new StreamReader(dataStream);
    string responseFromServer = reader.ReadToEnd();
    //log.info(responseFromServer);

    reader.Close();
    dataStream.Close();
    response.Close();
}

public class FacesContainer
{
    public Face[] Faces { get; set; }
    public string CatchId { get; set; }
    public string MediaUri { get; set; }

    public FacesContainer(string CatchId, string MediaUri, Face[] Faces){
        this.CatchId = CatchId;
        this.Faces = Faces;
        this.MediaUri = MediaUri;
    }
}

public class Face
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Loc_x { get; set; }
    public int Loc_y { get; set; }

    public Face(int Width, int Height, int Loc_x, int Loc_y){
        this.Width = Width;
        this.Height = Height;
        this.Loc_x = Loc_x;
        this.Loc_y = Loc_y;
    }
}
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

    document = new FacesContainer(catchId, $"blob/catch/{catchId}/{blobname}", faces);
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
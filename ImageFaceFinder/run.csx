#r "System.Runtime"
#r "System.Threading.Tasks"
#r "System.IO"
#r "System.Linq"

using System;
using System.Configuration;

using System.Runtime;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face; 
using Microsoft.ProjectOxford.Face.Contract;

public static void Run(Stream inputBlob, string blobname, out Faces document, TraceWriter log)
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

    document = new Faces(faces);
}

public class Faces{
    public Face[] Faces { get; set; }

    public Faces(Face[] Faces){
        this.Faces = Faces;
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
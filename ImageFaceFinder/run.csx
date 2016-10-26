#r "System.Runtime"
#r "System.Threading.Tasks"
#r "System.IO"

using System;
using System.Configuration;

using System.Runtime;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face; 
using Microsoft.ProjectOxford.Face.Contract;

public async static Task Run(Stream inputBlob, string blobname, out object document, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{blobname} \n Size: {inputBlob.Length} Bytes");

    IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["CognitiveServiceAPIKey"]);


    var faces = await faceServiceClient.DetectAsync(inputBlob);

    var faceRects = faces.Select(face => face.FaceRectangle);
    var result = faceRects.Select(faceRect => new Face(
        faceRect.Width, 
        faceRect.Height, 
        faceRect.Left, 
        faceRect.Top));

    document = result;

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
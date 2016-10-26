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

public async static Task Run(Stream inputBlob, string blobname, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{blobname} \n Size: {inputBlob.Length} Bytes");

    IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["CognitiveServiceAPIKey"]);


    var faces = await faceServiceClient.DetectAsync(inputBlob);
         log.Info($"Faces {faces}");
    var faceRects = faces.Select(face => face.FaceRectangle);

     foreach (int faceRect in faceRects)
    {
        log.Info($"ImageFile {faceRect.ImageFile}");
        log.Info($"Left {faceRect.Left}");
        log.Info($"Top {faceRect.Top}");
        log.Info($"Width {faceRect.Width}");
        log.Info($"Height {faceRect.Height}");

    }


    
}
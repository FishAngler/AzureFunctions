#r "System.Runtime"
#r "SSystem.Threading.Tasks"

using System;
using System.Configuration;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face; 
using Microsoft.ProjectOxford.Face.Contract;

public static void Run(Stream inputBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");

    IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["CognitiveServiceAPIKey"]);


    var faces = await faceServiceClient.DetectAsync(inputBlob);
         log.Info($"Faces {faces}");
    var faceRects = faces.Select(face => face.FaceRectangle);
    var temp = faceRects.ToArray();


    log.Info($"Result {temp}");

    
}
using System;
using System.Configuration;
using Microsoft.ProjectOxford.Face; 
using Microsoft.ProjectOxford.Face.Contract;

public static void Run(Stream inputBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");

    IFaceServiceClient faceServiceClient = new FaceServiceClient(ConfigurationManager.AppSettings["CognitiveServiceAPIKey"]);

    var faces = faceServiceClient.Detect(inputBlob);
     log.Info($"Face {faces}");
    var faceRects = faces.Select(face => face.FaceRectangle);
    var temp = faceRects.ToArray();

    log.Info($"Result {temp}");

    
}
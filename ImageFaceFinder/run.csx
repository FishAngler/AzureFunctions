using System;
using Microsoft.ProjectOxford.Face; 
using Microsoft.ProjectOxford.Face.Contract;

public static void Run(Stream inputBlob, string name, TraceWriter log)
{
    log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");

    IFaceServiceClient faceServiceClient = new FaceServiceClient("0a10db5a4d4448e38b8229d05b39e432");

    var face = faceServiceClient.Detect(inputBlob);
     log.Info($"Face {face}");
    var faceRects = faces.Select(face => face.FaceRectangle);
    var temp = faceRects.ToArray();

    log.Info($"Result {temp}");

    
}
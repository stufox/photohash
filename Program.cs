using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Amazon.SQS;
using Amazon.SQS.Model;


namespace photohash
{
    
    class FileObject
    {
        public string ShortName { get; set; }
        public string OriginalPath {get;set;}
        public string Hash {get;set;}


    }
    class Program
    {
        
        static void Main(string[] args)
        {
            string dirPath = "/mnt/Photo Shoots for clients/2016";
            string filePattern = "*.cr2";
            List<string> directories = new List<string>(Directory.EnumerateDirectories(dirPath));
            foreach (var dir in directories)
            {
                
                List<string> files = new List<string>(Directory.EnumerateFiles(dir,filePattern));
                foreach (var file in files)
                {
                    string hash = GetHash(file);
                    string shortName = file.Substring(file.LastIndexOf('/')+1);
                
                    var thisThing = new FileObject {ShortName=shortName,OriginalPath=file,Hash=hash};
                    var json = JsonConvert.SerializeObject(thisThing);
                    System.Console.WriteLine($"{json}");
                }


            }
            AmazonSQSClient mySQSClient = new AmazonSQSClient(Amazon.RegionEndpoint.APSoutheast2);
            

            
            
        }
    
    static string GetHash(string filename)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-","").ToLowerInvariant();
            }
        }
    }
}
}
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.S3;
using Microsoft.EntityFrameworkCore.Sqlite;
using System.Text;


namespace photohash
{

    class Program
    {
        
        static void Main(string[] args)
        {
            const ushort takenDateTagId = 306;
            string dirPath = "/Users/stufox/Documents/test";
            string filePattern = "*.CR2";
            //AmazonSQSClient mySQSClient = new AmazonSQSClient(Amazon.RegionEndpoint.APSoutheast2);
            
            using (var context = new photohashContext()) 
            {
                context.Database.EnsureCreated();
                List<string> directories = new List<string>(Directory.EnumerateDirectories(dirPath));
                foreach (var dir in directories)
                {
                
                    List<string> files = new List<string>(Directory.EnumerateFiles(dir,filePattern));
                    foreach (var file in files)
                    {
                        string hash = GetHash(file);
                        string shortName = file.Substring(file.LastIndexOf('/')+1);
                        
                        var thisThing = new photoObject {ShortName=shortName,OriginalPath=file,Hash=hash};
                        var json = JsonConvert.SerializeObject(thisThing);
                        //System.Console.WriteLine($"{json}");

                        //System.Console.WriteLine($"{context.photoObjects.Find(hash).ToString()}");
                        if ((context.photoObjects.Find(hash))==null)
                        {
                            System.Console.WriteLine($"Writing object {shortName} to database");
                            context.photoObjects.Add(thisThing);
                            context.SaveChanges();
                            using (BinaryReader reader = new BinaryReader(File.Open(file,FileMode.Open,FileAccess.Read)))
                            {
                            var header = new CR2Header();
                            header.byteOrder = reader.ReadBytes(2);
                            header.tiffMagicWord = reader.ReadUInt16();
                            header.tiffHeaderOffset = reader.ReadUInt32();
                            header.cr2MagicWord = reader.ReadBytes(2);
                            header.cr2Version = reader.ReadBytes(2);
                            header.rawIFDOffset = reader.ReadUInt32();  
                            var numEntries = reader.ReadUInt16();
                            //System.Console.WriteLine($"There are {numEntries} entries");
                            var entries = new List<IFDEntry>();
                            for (int i=1;i<=numEntries;i++)
                            {
                                var entry = new IFDEntry();
                                entry.tagId = reader.ReadUInt16();
                                entry.tagType = reader.ReadUInt16();
                                entry.numberOfValue = reader.ReadUInt32();
                                entry.valuePointer = reader.ReadUInt32();
                                entries.Add(entry);

                            }
                            foreach (var entry in entries)
                            {
                                switch(entry.tagId)
                                {
                                    case takenDateTagId:
                                        reader.BaseStream.Seek(entry.valuePointer,SeekOrigin.Begin);
                                        var bytes = reader.ReadBytes((int)entry.numberOfValue);
                                        System.Console.WriteLine($"Tag id {entry.tagId} has contents of {Encoding.ASCII.GetString(bytes)}");
                                        break;
                                        default:
                                        break;

                                }
                            }
                        }
                    }


                    }
                }       
            }

            
            
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
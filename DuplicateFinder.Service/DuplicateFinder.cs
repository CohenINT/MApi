using System.Collections.Concurrent;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DuplicateFinder.Service;

public class FileData
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public double SizeInMB { get; set; }
    public int DuplicateCount { get; set; }
}

public class DuplicateFinder
{
    public ILogger<DuplicateFinder> log { set; get; }
    public IServiceProvider services { get; set; }
    public static SHA256 sha256 = SHA256.Create();
    public ConcurrentDictionary<string, FileData> fileNames { set; get; }

    public DuplicateFinder(IServiceProvider svc)
    {
        this.services = svc;
        this.log = this.services.GetRequiredService<ILogger<DuplicateFinder>>();
        this.fileNames = new ConcurrentDictionary<string, FileData>();
    }

    public Task<bool> GetFileDetails(string path)
    {
        var files = Directory.GetFileSystemEntries(path,"*", SearchOption.AllDirectories).ToList();
 
        return null;
    }

    public async Task<string> IndexAllFilesAsync(string pathToFolder)
    {
       
        var filesAndDirectories = Directory.GetFileSystemEntries(pathToFolder);
        

        foreach (var fd in filesAndDirectories)
        {
            if (Directory.Exists(fd))
            { // directory
                return await Task.FromResult(fd);
            }
            
            else if (File.Exists(fd))
            { // file
                
            }
        }


        return null;
    }
    
    
    
}
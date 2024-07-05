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
    public long SizeInMB { get; set; }
    public int? DuplicateCount { get; set; }
    public string HashedValue { set; get; }
}

public class DuplicateFinder
{
    public ILogger<DuplicateFinder> log { set; get; }
    public IServiceProvider services { get; set; }
    public static SHA256 sha256 = SHA256.Create();
    public ConcurrentDictionary<string, List<FileData>> fileNames { set; get; }

    private async Task<string> HashFile(string path)
    {
        this.log.LogInformation($"[{path}] : Begin hashing.");
        await using var stream = new FileStream(path, FileMode.Open);
        var hashedValue = await sha256.ComputeHashAsync(stream);
        var resultString = Convert.ToHexString(hashedValue);
        this.log.LogInformation($"[{path}] : Hashing complete.");
        return resultString;
    }
    
   

    private async Task ExtractAndSaveFileData(string path)
    {
        var HashValueTask = HashFile(path);
        var info = new FileInfo(path);
        
        var fd = new FileData()
        {
            FileName = info.Name,
            FileType = info.Extension,
            DuplicateCount = null,//TODO: how do i update this property? each one? does not make sense. find other way.
            SizeInMB = info.Length * 1024,
            HashedValue = await HashValueTask
        };

        this.fileNames.AddOrUpdate(fd.HashedValue, _ => new List<FileData>(), (key, existingList) =>
        {
            existingList.Add(fd);
            return existingList;
        });

    }

    public DuplicateFinder(IServiceProvider svc)
    {
        this.services = svc;
        this.log = this.services.GetRequiredService<ILogger<DuplicateFinder>>();
        this.fileNames = new ConcurrentDictionary<string, List<FileData>>();
    }
    
    public async Task IndexAllFilesAsync(string pathToFolder)
    {
        var filesAndDirectories = Directory.GetFileSystemEntries(pathToFolder);
        var tasks =  filesAndDirectories.Select(fd =>
        {
            if (Directory.Exists(fd))
            {
                // directory
                return IndexAllFilesAsync(fd);
            }

            else if (File.Exists(fd))
            {
                // file
                // Note: can start processing this specific file, no need to wait for an answer.
                return ExtractAndSaveFileData(fd);
            }

            return Task.CompletedTask;
        });
        
        await Task.WhenAll(tasks);
        return;

    }
}
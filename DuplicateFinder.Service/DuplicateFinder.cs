using System.Collections.Concurrent;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DuplicateFinder.Service;

public class FileData
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public decimal SizeInMB { get; set; }
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

        var resultString = "";
        try
        {
            await using var stream = File.OpenRead(path);
            var hashedValue = await sha256.ComputeHashAsync(stream);
            resultString = Convert.ToHexString(hashedValue);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            log.LogError(e,$"Failed to read from path: {path}");
        }
        
        this.log.LogInformation($"[{path}] : Hashing complete.");

        return resultString;
    }


    private async Task ExtractAndSaveFileData(string path)
    {
        var hashstring = "";
        try
        {
            hashstring = await HashFile(path);
        }
        catch (Exception e)
        {
            log.LogError(e, $"failed to compute hash for : {path}");
        }

        var info = new FileInfo(path);
        
        var fd = new FileData()
        {
            FileName = info.Name,
            FileType = info.Extension,
            SizeInMB = Math.Round((decimal.Add( info.Length,0) / 1024) / 1024,4),
            HashedValue = hashstring
        };

        this.fileNames.AddOrUpdate(fd.HashedValue, (key) => new List<FileData>() { fd }, (key, existingList) =>
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

    public string ExportIndexToJSON()
    {
        var strData = "";
        try
        {
            strData = JsonConvert.SerializeObject(this.fileNames, Formatting.Indented);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return strData;
    }

    public async Task IndexAllFilesAsync(string pathToFolder)
    {
        var filesAndDirectories = Directory.GetFileSystemEntries(pathToFolder);
        var tasks = filesAndDirectories.Select(fd =>
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
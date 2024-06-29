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
        var files = Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories).ToList();

        return null;
    }


    public async Task ProcessFileAsync(string path)
    {
        return;
    }

    public async Task<string> IndexAllFilesAsync(string pathToFolder)
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
                return ProcessFileAsync(fd);
            }

            return Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
        return "DONE";
        // foreach (var fd in filesAndDirectories)
        // {
        //     if (Directory.Exists(fd))
        //     { // directory
        //         return await Task.FromResult(fd);
        //     }
        //     
        //     else if (File.Exists(fd))
        //     { // file
        //         // Note: can start processing this specific file, no need to wait for an answer.
        //          ProcessFileAsync(fd);
        //     }
        // }


    }
}
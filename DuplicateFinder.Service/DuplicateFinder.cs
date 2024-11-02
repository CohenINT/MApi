using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace DuplicateFinder.Service;

public enum StateEnum
{
    InProgress,
    Completed,
    Error
}

public class FileData
{
    public string FileName { get; set; }
    public string FileType { get; set; }
    public decimal SizeInMB { get; set; }
    public string FullFilePath { set; get; }
    public string HashedValue { set; get; }
}

public class DuplicateFinder
{
    private ILogger<DuplicateFinder> Log { set; get; }
    private IServiceProvider Services { get; set; }
    private static readonly SHA256 Sha256 = SHA256.Create();
    private ConcurrentQueue<string> PathsQueue { set; get; }
    private ConcurrentDictionary<string, List<FileData>> FileNames { set; get; }

    private async Task<string> HashFile(string path)
    {
        this.Log.LogInformation($"[{path}] : Begin hashing.");
        var resultString = "";
        try
        {
            await using var stream = File.OpenRead(path);
            //var hashedValue = await Sha256.ComputeHashAsync(stream);
            resultString = Convert.ToHexString(await Sha256.ComputeHashAsync(stream));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.LogError(e, $"Failed to read from path: {path}");
        }

        this.Log.LogInformation($"[{path}] : Hashing complete.");
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
            Log.LogError(e, $"failed to compute hash for : {path}");
        }

        var info = new FileInfo(path);
        var fd = new FileData()
        {
            FileName = info.Name,
            FileType = info.Extension,
            SizeInMB = Math.Round((decimal.Add(info.Length, 0) / 1024) / 1024, 4),
            HashedValue = hashstring,
            FullFilePath = path
        };

        hashstring = null;
        info = null;
        this.FileNames.AddOrUpdate(fd.HashedValue, (key) => new List<FileData>() { fd }, (key, existingList) =>
        {
            existingList.Add(fd);
            return existingList;
        });
    }

    public DuplicateFinder(IServiceProvider svc)
    {
        this.Services = svc;
        this.Log = this.Services.GetRequiredService<ILogger<DuplicateFinder>>();
        this.FileNames = new ConcurrentDictionary<string, List<FileData>>();
        this.PathsQueue = new ConcurrentQueue<string>();
    }

    public async Task<string> ExportIndexToJson(string pathToSave = "")
    {
        var strData = "";
        try
        {
            strData = JsonConvert.SerializeObject(this.FileNames, Formatting.Indented);
            if (!string.IsNullOrEmpty(pathToSave))
                await File.WriteAllTextAsync(pathToSave, strData);
        }
        catch (Exception e)
        {
            Log.LogError(e, $"failed to convert the data to json format string");
        }
        Log.LogInformation($"Exported completed to {pathToSave}.  amount of paths: {this.PathsQueue.Count}. total files: {this.FileNames.Count}.");
        return strData;
    }

    
    
    public async Task IndexAllFilesAsyncV2(string pathToFolder)
    {
        _ = await Directory.GetFileSystemEntries(pathToFolder)
            .ToObservable()
            .Select(fd => Observable.FromAsync(async () =>
            {
                if (Directory.Exists(fd))
                {
                    // directory
                    return IndexAllFilesAsyncV2(fd);
                }

                else if (File.Exists(fd))
                {
                    // file
                    this.PathsQueue.Enqueue(fd);
                }

                return Task.CompletedTask;
            }))
            .Merge(10)
            .LastOrDefaultAsync();
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
                this.PathsQueue.Enqueue(fd);
            }

            return Task.CompletedTask;
        });

        await Task.WhenAll(tasks);
    }

    public async Task ProcessFilesAsync()
    {
        _ = await this.PathsQueue
            .ToObservable()
            .Select(p => Observable.FromAsync(async () =>
            {
                 await ExtractAndSaveFileData(p);
            }))
            .Merge(500)
            .LastOrDefaultAsync();
    }

    public async Task Init(string path)
    {
        Log.LogInformation("Init start");
        Stopwatch st = new Stopwatch();
        st.Start();
        await IndexAllFilesAsync(path);
        st.Stop();
        Log.LogInformation($"index completed in {st.ElapsedMilliseconds} ms.");
        st.Reset();
        st.Start();
        await ProcessFilesAsync();
        st.Stop();
        Log.LogInformation($"ProcessFile completed in {st.ElapsedMilliseconds} ms.");
    }
}
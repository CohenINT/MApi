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
    private ConcurrentBag<string> FailedFiles { set; get; }   
    private ConcurrentBag<string> SuccedFiles { set; get; }
    private int TotalTask { set; get; }

    private async Task<string> GetFileHashV1(string path)
    {
        await using var stream = File.OpenRead(path);
        byte[] buff = null;
        await stream.ReadExactlyAsync(buff);
        var result = Convert.ToHexString(await Sha256.ComputeHashAsync(stream));
        return result;
    }

    private async Task<string> GetFileHashV2(string path)
    {
        await using FileStream fileStream = File.OpenRead(path);
        byte[] checksum  =  await Sha256.ComputeHashAsync(fileStream); 
        return BitConverter.ToString(checksum).Replace("-", String.Empty);

    }

    private async Task<string> GetFileHashV3(string path)
    {
        await using FileStream fileStream = File.OpenRead(path);
        var checksum  =   await MD5.HashDataAsync(fileStream);
        return BitConverter.ToString(checksum).Replace("-", String.Empty);
    }
    public async Task<string> HashFile(string path)
    {
        this.Log.LogInformation($"[{path}] : Begin hashing.");
        var resultString = "";
        try
        {
            resultString = await GetFileHashV3(path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.LogError(e, $"Failed to read from path: {path}");
            throw new Exception( $"Failed to read from path: {path}",e);
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
        catch (Exception eס)
        {
            Log.LogError(eס, $"failed to compute hash for : {path}. failed processing this file.");
            this.FailedFiles.Add(path);
            return;
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
        
        this.SuccedFiles.Add(path);
        Log.LogInformation($"File Process Succeeded for : {path}");
    }

    public DuplicateFinder(IServiceProvider svc)
    {
        this.Services = svc;
        this.Log = this.Services.GetRequiredService<ILogger<DuplicateFinder>>();
        this.FileNames = new ConcurrentDictionary<string, List<FileData>>();
        this.PathsQueue = new ConcurrentQueue<string>();
        this.FailedFiles = new ConcurrentBag<string>();
        this.SuccedFiles = new ConcurrentBag<string>();
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
        this.TotalTask = tasks.Count();
        Log.LogInformation($"Total jobs to execute: {this.TotalTask}");
    }

   
        public async Task ProcessFilesAsync()
        {
            _ = await this.PathsQueue
                .ToObservable()
                .Select(p => Observable.FromAsync(async () =>
                {
                     await ExtractAndSaveFileData(p);
                }))
                .Merge(20)
                .LastOrDefaultAsync();
        }

    public async Task Init(string path)
    {
        Log.LogInformation("Init start");
        Stopwatch st = new Stopwatch();
        st.Start();
        await IndexAllFilesAsyncV2(path);
        st.Stop();
        Log.LogInformation($"index completed in {st.ElapsedMilliseconds} ms.");
        st.Reset();
        st.Start();
        await ProcessFilesAsync();
        st.Stop();
        Log.LogInformation($"ProcessFile completed in {st.ElapsedMilliseconds} ms.");
    }
}
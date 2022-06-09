using Newtonsoft.Json;
using PdfPartIndexer.Indexer;
using PdfPartIndexer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfPartIndexer;
public class PdfIndexer
{
    #region -- properties

    public string IndexFileLocation { get; init; } = "";

    public string? BackupDirectory { get; init; }

    public string PartsDirectory { get; init; } = "";

    public string SongDirectory { get; init; } = "";

    #endregion

    public async Task Run()
    {
        // load the index file.
        var indexFile = await ReadIndexFile().ConfigureAwait(false);
        if (indexFile is null)
            throw new FileNotFoundException($"Index file could not be read. Unable to execute.");

        // backup the existing directory.
        await BackupPartsDirectory().ConfigureAwait(false);

        // prepare the output.
        var partDirectories = PreparePartsDirectory(indexFile);

        await ProcessSongs(indexFile, partDirectories).ConfigureAwait(false);
    }

    private async Task<IndexFile?> ReadIndexFile()
    {
        if (!File.Exists(IndexFileLocation))
            throw new FileNotFoundException($"Unable to find index file.");

        var indexFileJson = await File.ReadAllTextAsync(IndexFileLocation);
        var result = default(IndexFile);

        try
        {
            result = JsonConvert.DeserializeObject<IndexFile>(indexFileJson);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Invalid index file. Unable to read index file with message: {ex.Message}", ex);
        }

        return result;
    }

    private async Task BackupPartsDirectory()
    {
        if (string.IsNullOrWhiteSpace(BackupDirectory))
            return;

        if (!Directory.Exists(BackupDirectory))
        {
            LogHelper.LogDebug("Creating backup directory...");
            Directory.CreateDirectory(BackupDirectory);
        }

        LogHelper.LogDebug("Backing up existing output...");

        var backupDir = Path.Combine(BackupDirectory, $"backup-{DateTime.Now.ToString("yyyyMMddhhmmssfff")}");

        DirectoryHelper.CopyDirectory(PartsDirectory, backupDir, true);
    }

    private async Task ProcessSongs(IndexFile indexFile, IDictionary<IndexFilePart, string> partDirectories)
    {
        if (!Directory.Exists(SongDirectory))
            throw new DirectoryNotFoundException($"Unable to find the songs directory. {SongDirectory}");

        LogHelper.LogInfo("Processing songs...");

        // load all songs. 
        foreach (var song in indexFile.Songs)
        {
            LogHelper.LogDebug($"Processing {song.IndexNumber} - {song.Title}...");

            // determine song directory.
            var songDirectory = Path.Combine(SongDirectory, song.Title);

            if (!Directory.Exists(songDirectory))
            {
                LogHelper.LogWarning($"Unable to find directory {songDirectory}. Song could not be processed.");

                continue;
            }

            await ProcessSong(songDirectory, song, partDirectories).ConfigureAwait(false);
        }
    }

    private async Task ProcessSong(string songDirectory, IndexFileSong song, IDictionary<IndexFilePart, string> partDirectories)
    {
        // load all files.
        var files = new DirectoryInfo(songDirectory).GetFiles();

        // get the best file for each part.
        foreach (var (part, dir) in partDirectories)
        {
            // get the part name.
            var partName = GetPartName(part);

            try
            {
                // determine the correct file.
                var file = DetermineFile(part, files);

                if (string.IsNullOrWhiteSpace(file))
                {
                    if (part.Optional)
                        continue;

                    LogHelper.LogException($"Unable to load part {partName} for {song.Title}.");

                    continue;
                }
                
                // determine the new file name.
                var newFileName = $"{song.IndexNumber} - {song.Title} - {partName}.pdf";
                var newFile = Path.Combine(dir, newFileName);

                // copy the file.
                File.Copy(file, newFile, overwrite: true);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, $"Error copying part {partName} from {song.Title}. {ex.Message}");
            }
        }
    }

    private string DetermineFile(IndexFilePart part, IEnumerable<FileInfo> files)
    {
        // execute on mp3 files.
        if (part.IsAudioPart())
            throw new NotSupportedException("Audio is not yet supported.");

        // load all pdf's.
        var pdfFiles = files.Where(f => string.Equals(f.Extension, ".pdf", StringComparison.OrdinalIgnoreCase));

        // load the part name. 
        var partName = GetPartName(part);

        // determine the options.
        var options = new List<string>();
        options.Add(partName);
        options.Add(part.Instrument);

        // load abbreviations.
        var abbr = part.Abbreviations?.Split(',') ?? new string[] { };
        if (abbr.Length > 0)
            options.AddRange(abbr);

        foreach(var option in options)
        {
            var selected = pdfFiles.Where(f => f.Name.Contains(option, StringComparison.OrdinalIgnoreCase));
            if (selected.Count() == 1)
                return selected.First().FullName;
        }

        return "";
    }

    private IDictionary<IndexFilePart, string> PreparePartsDirectory(IndexFile indexFile)
    {
        if (indexFile.Parts?.Count() <= 0)
            throw new InvalidDataException($"No parts were defined in the index file.");

        if(!Directory.Exists(PartsDirectory))
        {
            LogHelper.LogDebug("Creating parts directory...");

            Directory.CreateDirectory(PartsDirectory);
        }

        LogHelper.LogInfo("Initializing output directory...");

        // load all current subdirectories.
        var subDirectories = Directory.EnumerateDirectories(PartsDirectory).ToList();
        LogHelper.LogInfo($"Detected {subDirectories.Count} part output directories.");

        var result = new Dictionary<IndexFilePart, string>();

        // process each directory.
        foreach (var part in indexFile.Parts ?? Enumerable.Empty<IndexFilePart>())
        {
            var partName = GetPartName(part);

            LogHelper.LogDebug($"  Initializing {partName}...");

            var partsDir = Path.Combine(PartsDirectory, partName);
            if (!Directory.Exists(partsDir))
                Directory.CreateDirectory(partsDir);

            var existingDir = subDirectories.FirstOrDefault(d => string.Equals(d, partsDir, StringComparison.OrdinalIgnoreCase));
            if (existingDir is not null)
                subDirectories.Remove(existingDir);

            result.Add(part, partsDir);
        }

        // remove subdirs.
        foreach (var sd in subDirectories)
        {
            LogHelper.LogDebug($"Removing {sd}...");

            Directory.Delete(sd, true);
        }

        return result;
    }

    private string GetPartName(IndexFilePart part)
    {
        var partName = part.Instrument;
        if (part.PartNumber > 0)
            partName = $"{partName} {part.PartNumber}";

        return partName;
    }
}

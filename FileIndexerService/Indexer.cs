using FileIndexerService;
using MongoDB.Driver;

namespace Business;
public class Indexer
{
    private readonly ILogger<Worker> _logger;
    private readonly Database _database;
    public Indexer(ILogger<Worker> logger)
    {
        _logger = logger;
        _database = new Database();
    }

    public async Task Setup()
    {
        await _database.Setup();
    }

    public async Task<bool> IndexFiles()
    {
        string directory = "D:\\Backup Drive";

        var directoryFiles =
            Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

        var mongoFiles = await _database.GetFiles();

        foreach (var file in directoryFiles)
        {
            FileAttributes attr = File.GetAttributes(file);

            if (!attr.HasFlag(FileAttributes.Directory))
            {
                _logger.LogDebug($"Add/Update file: {file}");
                AddUpdateFile(file);
            }
        }
        foreach (var file in mongoFiles)
        {
            if (string.IsNullOrEmpty(file.Source))
            {
                _logger.LogDebug($"Deleting file id: {file.Id}");
                _database.DeleteFile(file);
                continue;
            }
            var foundFile = directoryFiles.FirstOrDefault(x => x == file.Source);
            if (foundFile == null)
            {
                _logger.LogDebug($"Deleting file: {file.Source}");
                _database.DeleteFile(file);
            }
        }
        return true;
    }

    private void AddUpdateFile(string file)
    {
        DateTime createdDateTime = File.GetCreationTimeUtc(file);
        DateTime modifiedDateTime = File.GetLastWriteTimeUtc(file);
        DateTime lastAccessDateTime = File.GetLastAccessTimeUtc(file);
        DateTime lastWriteDateTime = File.GetLastWriteTimeUtc(file);

        var fileName = Path.GetFileNameWithoutExtension(file);
        var extension = Path.GetExtension(file);

        try
        {
            string size = "";
            string mimeType = "";
            int duration = 0;
            int width = 0;
            int height = 0;
            if (extension is not (".txt" or ".zip" or ".lnk"))
            {
                IEnumerable<MetadataExtractor.Directory> directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(file);
                var fileMetadataDirectory = directories.OfType<MetadataExtractor.Formats.FileSystem.FileMetadataDirectory>().FirstOrDefault();
                size = fileMetadataDirectory?.GetDescription(MetadataExtractor.Formats.FileSystem.FileMetadataDirectory.TagFileSize);

                var fileTypeDirectory = directories.OfType<MetadataExtractor.Formats.FileType.FileTypeDirectory>().FirstOrDefault();
                mimeType = fileTypeDirectory?.GetDescription(MetadataExtractor.Formats.FileType.FileTypeDirectory.TagDetectedFileMimeType);

                var quickTimeDirectory = directories.OfType<MetadataExtractor.Formats.QuickTime.QuickTimeTrackHeaderDirectory>().FirstOrDefault();
                var durationStr = quickTimeDirectory?.GetDescription(MetadataExtractor.Formats.QuickTime.QuickTimeTrackHeaderDirectory.TagDuration);
                var durationResult = Int32.TryParse(durationStr, out duration);
                var widthStr = quickTimeDirectory?.GetDescription(MetadataExtractor.Formats.QuickTime.QuickTimeTrackHeaderDirectory.TagWidth);
                var widthResult = Int32.TryParse(widthStr, out width);
                var heightStr = quickTimeDirectory?.GetDescription(MetadataExtractor.Formats.QuickTime.QuickTimeTrackHeaderDirectory.TagHeight);
                var heightResult = Int32.TryParse(heightStr, out height);
            }

            var newFile = new MyFile()
            {
                Title = fileName,
                OriginalTitle = fileName,
                MimeType = mimeType ?? "",
                Size = size,
                CreatedDateTime = createdDateTime,
                ModifiedDateTime = modifiedDateTime,
                LastAccessDateTime = lastAccessDateTime,
                LastWriteDateTime = lastWriteDateTime,
                Source = file,
                OriginalSource = file,
                Extension = extension,
                Duration = duration,
                Width = width,
                Height = height
            };
            if (!_database.FileAlreadyExists(newFile.Source))
            {
                _database.AddFile(newFile);
            }
            else
            {
                _database.UpdateFile(newFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed AddUpdate for file: " + file);
        }
    }

}
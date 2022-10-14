using MongoDB.Driver;

namespace Data;
public class Database
{
    public Database()
    {
    }

    private IMongoCollection<MyFile>? _collection;

    public async Task Setup()
    {
        var client = new MongoClient(
            "mongodb://localhost:27017"
        );
        var database = client.GetDatabase("local");
        var collections = await database.ListCollectionNamesAsync();
        var names = collections.ToList();
        if (names.Where(x => x == "files") == null)
        {
            database.CreateCollection("files");
        }

        _collection = database.GetCollection<MyFile>("files");
    }
    public async Task<List<MyFile>> GetFiles()
    {
        if (_collection == null)
        {
            return new List<MyFile>();
        }
        return await _collection.Find(x => true).ToListAsync();
    }

    public void AddFile(MyFile newFile)
    {
        _collection?.InsertOne(newFile);
    }
    public void UpdateFile(MyFile newFile)
    {
        _collection.FindOneAndReplace(x => x.Id == newFile.Id, newFile);
    }
    public void DeleteFile(MyFile newFile)
    {
        newFile.DeletedDateTime = DateTime.UtcNow;
        _collection.FindOneAndReplace(x => x.Id == newFile.Id, newFile);
    }
    public void HardDeleteFile(MyFile newFile)
    {
        _collection.FindOneAndDelete(x => x.Id == newFile.Id);
    }
    public bool FileAlreadyExists(string fileSource)
    {
        var foundFile = _collection.Find(x => x.Source == fileSource).FirstOrDefault();
        return foundFile != null;
    }
    public MyFile? GetFile(string fileSource)
    {
        var foundFile = _collection.Find(x => x.Source == fileSource).FirstOrDefault();
        return foundFile;
    }
}
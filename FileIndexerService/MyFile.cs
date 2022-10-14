
using MongoDB.Bson;

namespace Models;
public class MyFile
{
    public ObjectId Id { get; set; }
    public string Title { get; set; } = "";
    public string Extension { get; set; } = "";
    public int Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string MimeType { get; set; } = "";
    public string Source { get; set; } = "";
    public string Size { get; set; } = "";
    public string OriginalSource { get; set; } = "";
    public string OriginalTitle { get; set; } = "";
    public List<string> Tags { get; set; } = new List<string>();
    public DateTime CreatedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
    public DateTime LastAccessDateTime { get; set; }
    public DateTime LastWriteDateTime { get; set; }
    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        MyFile file = obj as MyFile;
        if ((System.Object)file == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (Title == file.Title);
    }

    public bool Equals(MyFile file)
    {
        // If parameter is null return false:
        if ((object)file == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (Title == file.Title);
    }

    public override int GetHashCode()
    {
        return Title.GetHashCode();
    }
}
namespace Amlaki.Domain.ValueObjects.Photo;

public class Photo
{
    public string Original { get; private set; }
    public string Small { get; private set; }   // 165x165
    public string Medium { get; private set; }  // 256x256

    public Photo(string original, string small, string medium)
    {
        Original = original;
        Small = small;
        Medium = medium;
    }

    public void Update(string original, string small, string medium)
    {
        Original = original;
        Small = small;
        Medium = medium;
    }
}

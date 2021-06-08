using System;

[Serializable]
public class GameFile 
{
    public string FileLocation { get; }
    public string FileName { get; }

    public GameFile(string l, string n) 
    {
        FileLocation = l;
        FileName = n;
    }
}
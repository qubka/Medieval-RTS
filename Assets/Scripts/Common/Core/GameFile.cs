﻿using System;

[Serializable]
public class GameFile 
{
    public string FileName { get; }

    public GameFile(string name) 
    {
        FileName = name;
    }
}
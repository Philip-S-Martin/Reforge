using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

public static class SaveLoad
{

    //it's static so we can call it from anywhere
    public static void Save(StructureData saveGame, string saveGameName)
    {
        Directory.CreateDirectory(Application.persistentDataPath + "/Savegames/");
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Savegames/" + saveGameName + ".sav"); //you can call it anything you want, including the extension. The directories have to exist though.
        bf.Serialize(file, saveGame);
        file.Close();
        Debug.Log("Saved Game: " + saveGameName + " to " + Application.persistentDataPath + "/Savegames/");
    }

    public static StructureData Load(string gameToLoad)
    {
        if (SaveExists(gameToLoad))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/Savegames/" + gameToLoad + ".sav", FileMode.Open);
            StructureData loadedGame = (StructureData)bf.Deserialize(file);
            file.Close();
            Debug.Log("Loaded Game: " + gameToLoad);
            return loadedGame;
        }
        else
        {
            Debug.Log("File doesn't exist!");
            return null;
        }
    }

    public static bool SaveExists(string saveGameName)
    {
        return File.Exists(Application.persistentDataPath + "/Savegames/" + saveGameName + ".sav");
    }

}


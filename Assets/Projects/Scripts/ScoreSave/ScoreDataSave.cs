using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using UnityEngine;

/* 本来使うクラス
public static class SaveDataSave
{
    // private static void S()
    // {
    //     SaveData.song = new SongData[]
    //     {
    //             new SongData
    //             {
    //                 Id = "Test2",
    //                 Detail = new ScoreDetail[]
    //                 {
    //                     new ScoreDetail { Difficulty = 2, Score = 900000, Rank = 1, Accuracy = 9635 },
    //                     new ScoreDetail { Difficulty = 3, Score = 852300, Rank = 0, Accuracy = 9322 }
    //                 }
    //             },
    //             new SongData
    //             {
    //                 Id = "TwiNote",
    //                 Detail = new ScoreDetail[]
    //                 {
    //                     new ScoreDetail { Difficulty = 1, Score = 900000, Rank = 1, Accuracy = 9635 }
    //                 }
    //             }
    //     };
    // }

    public static void ScoreWrite()
    {
        var serialized = MessagePackSerializer.Serialize(SaveData.song);
        
        SaveText(
            GetSecureDataPath(),
            "Score.dat",
            serialized
            );
    }

    public static void SettingWrite()
    {
        var serialized = MessagePackSerializer.Serialize(SaveData.setting);
        
        SaveText(
            GetSecureDataPath(),
            "Setting.dat",
            serialized
            );
    }

    public static void ScoreRead()
    {
        try
        {
            ReadText(
                GetSecureDataPath(),
                "Score.dat",
                true
            );
        }
        catch (Exception e)
        {
            Debug.Log("Score Read failed.");
            SaveData.song = new List<SongData>();
        }
    }

    public static void SettingRead()
    {
        try
        {
            ReadText(
                GetSecureDataPath(),
                "Setting.dat",
                false
            );
        }
        catch (Exception e)
        {
            Debug.Log("Setting Read failed.");
            SaveData.setting = new Setting();
        }
    }

    private static void SaveText(string filePath, string fileName, byte[] textToSave)
    {
        var combinedPath = Path.Combine(filePath, fileName);
        using var fileStream = new FileStream(combinedPath, FileMode.Create, FileAccess.Write);
        fileStream.Write(textToSave, 0, textToSave.Length);
    }
    
    private static void ReadText(string filePath, string fileName, bool isScore)
    {
        var combinedPath = Path.Combine(filePath, fileName);
        
        using var fileStream = new FileStream(combinedPath, FileMode.Open);
        
        var bs = new byte[fileStream.Length];
        fileStream.Read(bs, 0, bs.Length);

        if (isScore)
            SaveData.song = MessagePackSerializer.Deserialize<List<SongData>>(bs);
        else
            SaveData.setting = MessagePackSerializer.Deserialize<Setting>(bs);
    }

    private static string GetSecureDataPath()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir"))
        {
            string secureDataPathForAndroid = getFilesDir.Call<string>("getCanonicalPath");
            return secureDataPathForAndroid;
        }
#else
        // 本来は各プラットフォームに対応した処理が必要
        return Application.persistentDataPath;
#endif
    }
}

*/


public static class SaveDataSave
{
    public static void S()
    {
        SaveData.song = new ScoreData()
        {
            item = new List<SongData>()
            {
                new SongData("Test2")
                {
                    detail = new SongDetail[4]
                    {
                        new SongDetail(),
                        new SongDetail(),
                        new SongDetail() { score = 900000 },
                        new SongDetail() { score = 852300 }
                    }
                },
                new SongData("TwiNote")
                {
                    detail = new SongDetail[4]
                    {
                        new SongDetail(),
                        new SongDetail() { score = 900000 },
                        new SongDetail(),
                        new SongDetail()
                    }
                }
            }
        };
        
        SaveData.setting = new Setting();
        
        ScoreWrite();
        SettingWrite();
    }
    
    public static void ScoreWrite()
    {
        var serialized = JsonUtility.ToJson(SaveData.song);
        
        SaveText(
            GetSecureDataPath(),
            "Score.data",
            serialized
            );
    }

    public static void SettingWrite()
    {
        var serialized = JsonUtility.ToJson(SaveData.setting);
        
        SaveText(
            GetSecureDataPath(),
            "Setting.data",
            serialized
            );
    }

    public static void ScoreRead()
    {
        try
        {
            ReadText(
                GetSecureDataPath(),
                "Score.data",
                true
            );
        }
        catch (Exception e)
        {
            Debug.Log("Score Read failed.");
            SaveData.song = new ScoreData();
        }
    }

    public static void SettingRead()
    {
        try
        {
            ReadText(
                GetSecureDataPath(),
                "Setting.data",
                false
            );
        }
        catch (Exception e)
        {
            Debug.Log("Setting Read failed.");
            SaveData.setting = new Setting();
        }
    }

    private static void SaveText(string filePath, string fileName, string textToSave)
    {
        var combinedPath = Path.Combine(filePath, fileName);
        using var writer = new StreamWriter(combinedPath, false);
        writer.Write(textToSave);
    }
    
    private static void ReadText(string filePath, string fileName, bool isScore)
    {
        var combinedPath = Path.Combine(filePath, fileName);
        
        using var reader = new StreamReader(combinedPath);
        string str = reader.ReadToEnd();

        if (isScore)
        {
            SaveData.song = JsonUtility.FromJson<ScoreData>(str);
        }
        else
        {
            SaveData.setting = JsonUtility.FromJson<Setting>(str);
        }
    }

    private static string GetSecureDataPath()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir"))
        {
            string secureDataPathForAndroid = getFilesDir.Call<string>("getCanonicalPath");
            return secureDataPathForAndroid;
        }
#else
        // 本来は各プラットフォームに対応した処理が必要
        return Application.persistentDataPath;
#endif
    }
}

// テストクラス
[Serializable]
public class test
{
    public int id;
    public string str;
    
    public test(int id, string str)
    {
        this.id = id;
        this.str = str;
    }
}

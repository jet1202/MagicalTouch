using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using UnityEngine;

public static class ScoreDataSave
{
    private static void S()
    {
        // ScoreData.song = new SongData[]
        // {
        //         new SongData
        //         {
        //             Id = "Test2",
        //             Detail = new ScoreDetail[]
        //             {
        //                 new ScoreDetail { Difficulty = 2, Score = 900000, Rank = 1, Accuracy = 9635 },
        //                 new ScoreDetail { Difficulty = 3, Score = 852300, Rank = 0, Accuracy = 9322 }
        //             }
        //         },
        //         new SongData
        //         {
        //             Id = "TwiNote",
        //             Detail = new ScoreDetail[]
        //             {
        //                 new ScoreDetail { Difficulty = 1, Score = 900000, Rank = 1, Accuracy = 9635 }
        //             }
        //         }
        // };
        
        ScoreRead();
        SettingRead();
    }

    private static void ScoreWrite()
    {
        var serialized = MessagePackSerializer.Serialize(ScoreData.song);
        
        SaveText(
            GetSecureDataPath(),
            "Score.dat",
            serialized
            );
    }

    private static void SettingWrite()
    {
        var serialized = MessagePackSerializer.Serialize(ScoreData.setting);
        
        SaveText(
            GetSecureDataPath(),
            "Setting.dat",
            serialized
            );
    }

    private static void ScoreRead()
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
            ScoreData.song = Array.Empty<SongData>();
        }
    }

    private static void SettingRead()
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
            ScoreData.setting = new Setting();
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
            ScoreData.song = MessagePackSerializer.Deserialize<SongData[]>(bs);
        else
            ScoreData.setting = MessagePackSerializer.Deserialize<Setting>(bs);
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

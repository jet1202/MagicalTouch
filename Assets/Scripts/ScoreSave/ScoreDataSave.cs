using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using UnityEngine;

public static class ScoreDataSave
{
    public static SongData[] scoreData;

    private static void S()
    {
        scoreData = new SongData[]
        {
                new SongData
                {
                    Id = "Test2",
                    Detail = new ScoreDetail[]
                    {
                        new ScoreDetail { Difficulty = 2, Score = 900000, Rank = 1, Accuracy = 9635 },
                        new ScoreDetail { Difficulty = 3, Score = 852300, Rank = 0, Accuracy = 9322 }
                    }
                },
                new SongData
                {
                    Id = "TwiNote",
                    Detail = new ScoreDetail[]
                    {
                        new ScoreDetail { Difficulty = 1, Score = 900000, Rank = 1, Accuracy = 9635 }
                    }
                }
        };
        
        Save();
        Read();
    }

    private static void Save()
    {
        var serialized = MessagePackSerializer.Serialize(scoreData);
        
        SaveText(
            GetSecureDataPath(),
            "Score.dat",
            serialized
        );
    }

    private static void Read()
    {
        ReadText(
            GetSecureDataPath(),
            "Score.dat"
        );
    }

    private static void SaveText(string filePath, string fileName, byte[] textToSave)
    {
        var combinedPath = Path.Combine(filePath, fileName);
        try
        {
            using (var fileStream = new FileStream(combinedPath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(textToSave, 0, textToSave.Length);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Saveできませんでした");
            Debug.Log(e);
        }
    }
    
    private static void ReadText(string filePath, string fileName)
    {
        var combinedPath = Path.Combine(filePath, fileName);
        try
        {
            using (var fileStream = new FileStream(combinedPath, FileMode.Open))
            {
                var bs = new byte[fileStream.Length];
                fileStream.Read(bs, 0, bs.Length);

                scoreData = MessagePackSerializer.Deserialize<SongData[]>(bs);
            
                var json = MessagePackSerializer.ConvertToJson(bs);
                Debug.Log(json);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Readできませんでした");
            scoreData = new SongData[0];
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

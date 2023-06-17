using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ImportScore : MonoBehaviour
{
    private SongList[] list;

    public IEnumerator ImportSongData()
    {
        string url = Application.streamingAssetsPath + "/SongData/SongData.json";
        
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.ConnectionError)
        {
            string jsonStr = req.downloadHandler.text;

            ListSaveData data = JsonUtility.FromJson<ListSaveData>(jsonStr);

            list = data.item;
        
            yield return list;
        }
        else
        {
            Debug.Log("error");
        }
    }

    public IEnumerator ImportJacket(string name)
    {
        string url = Application.streamingAssetsPath + $"/SongData/{name}/jacket.jpg";

        Debug.Log($"url: {url}, Exists: {System.IO.File.Exists(url)}");

        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            var myTexture = ((DownloadHandlerTexture)req.downloadHandler).texture;

            yield return myTexture;
        }
        else
        {
            Debug.Log($"Error: result -> ${req.result}, msg -> ${req.error}");
            yield return null;
        }
    }

    public IEnumerator ImportInfo(string name)
    {
        string url = Application.streamingAssetsPath + $"/SongData/{name}";
        
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.ConnectionError)
        {
            string jsonStr = req.downloadHandler.text;

            SongInfo data = JsonUtility.FromJson<SongInfo>(jsonStr);

            yield return data;
        }
        else
        {
            Debug.Log("error");
            yield return null;
        }
    }
}

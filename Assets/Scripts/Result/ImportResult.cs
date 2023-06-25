using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ImportResult : MonoBehaviour
{
    public IEnumerator ImportJacket(string name)
    {
        string url = Application.streamingAssetsPath + $"/SongData/{name}/jacket.jpg";

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var myTexture = ((DownloadHandlerTexture)req.downloadHandler).texture;

                yield return myTexture;
            }
            else
            {
                yield return null;
            }
        }
    }
}

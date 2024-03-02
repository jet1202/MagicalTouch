using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ImportCategory : MonoBehaviour
{
    private CategoryData data;
    
    public IEnumerator ImportCategoryData()
    {
        string url = Application.streamingAssetsPath + "/SongData/CategoryData.json";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.ConnectionError)
            {
                string jsonStr = req.downloadHandler.text;

                data = JsonUtility.FromJson<CategoryData>(jsonStr);

                yield return data;
            }
            else
            {
                Debug.Log("error");
            }
        }
    }
}

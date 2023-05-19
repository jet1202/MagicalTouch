using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyObjectPool : MonoBehaviour
{
    private List<GameObject> _pool = new List<GameObject>();
    public GameObject prefab;

    public GameObject SetObject()
    {
        GameObject ins;
        
        if (_pool.Count == 0)
        {
            ins = Instantiate(prefab, Vector3.zero, Quaternion.identity, this.transform);
        }
        else
        {
            ins = _pool.Last();
            _pool.RemoveAt(_pool.Count - 1);
        }
        ins.SetActive(true);

        return ins;
    }

    public void RemoveObject(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Add(obj);
    }
}

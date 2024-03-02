using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryWindowController : MonoBehaviour
{
    [SerializeField] private ImportCategory importCategory;
    [SerializeField] private RectTransform window;
    [SerializeField] private RectTransform right;

    [SerializeField] private GameObject tabPrefab;
    [SerializeField] private GameObject itemPrefab;

    private string[] tabsData;
    private Category[] contentsData;
    private int[] progressData;
    
    private List<GameObject> tabs = new List<GameObject>();
    
    IEnumerator Start()
    {
        // CategoryDataのインポート
        IEnumerator corutine = importCategory.ImportCategoryData();
        yield return StartCoroutine(corutine);
        CategoryData data = (CategoryData)corutine.Current;
        tabsData = data.tabs;
        contentsData = data.item;
        progressData = new int[contentsData.Length];
        
        // タブの生成
        for (int i = 0; i < tabsData.Length; i++)
        {
            GameObject tab = Instantiate(tabPrefab, window);
            tabs.Add(tab);
            
            var ribbonImage = tab.transform.GetChild(0).GetChild(0);
            ribbonImage.GetComponent<RectTransform>().localPosition = new Vector3(80f + 160f * i, -2.5f, 0f);
            ribbonImage.GetChild(0).GetComponent<TextMeshProUGUI>().text = tabsData[i];
            
            // クリック時
            ribbonImage.GetComponent<Button>().onClick.AddListener(() => OnClickTab(i));
        }
        
        // カテゴリの生成
        int[] ind = new int[tabsData.Length]; 
        for (int i = 0; i < contentsData.Length; i++)
        {
            int tab = contentsData[i].tab;
            GameObject content = Instantiate(itemPrefab, tabs[tab].transform.GetChild(1).GetChild(2));

            var cPos = content.GetComponent<RectTransform>().localPosition;
            cPos.y = -40f - 80f * ind[tab]++;
            content.GetComponent<RectTransform>().localPosition = cPos;
            
            // スコアデータをもとに表示を変更
            
            // content.transform.GetChild(0).GetComponent<Image>().sprite =
            content.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = contentsData[i].name;
            content.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = contentsData[i].sub;
            content.transform.GetChild(4).gameObject.SetActive(contentsData[i].document != -1);
            
            // クリック時
            content.GetComponent<Button>().onClick.AddListener(() => OnClickContent(i));
        }
    }

    public void OnClickTab(int index)
    {
        
    }
    
    public void OnClickContent(int index)
    {
        
    }
}

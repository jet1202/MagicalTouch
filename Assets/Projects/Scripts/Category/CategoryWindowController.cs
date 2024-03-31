using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CategoryWindowController : MonoBehaviour
{
    [SerializeField] private ImportCategory importCategory;
    [SerializeField] private RectTransform window;
    [SerializeField] private RectTransform right;

    [SerializeField] private List<Sprite> iconSprites;
    
    [SerializeField] private GameObject tabPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject mask;

    private string[] tabsData;
    private Category[] contentsData;
    private int[] progressData;
    
    private List<GameObject> tabs = new List<GameObject>();
    private List<GameObject> contents = new List<GameObject>();

    private int selectTab = 0;
    private int selectContent = -1;
    
    IEnumerator Start()
    {
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        
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
            var i1 = i;
            ribbonImage.GetComponent<Button>().onClick.AddListener(() => OnClickTab(i1));
        }
        
        // カテゴリの生成
        int[] ind = new int[tabsData.Length]; 
        for (int i = 0; i < contentsData.Length; i++)
        {
            int tab = contentsData[i].tab;
            GameObject content = Instantiate(itemPrefab, tabs[tab].transform.GetChild(1).GetChild(0).GetChild(0));
            contents.Add(content);

            var cPos = content.GetComponent<RectTransform>().localPosition;
            cPos.y = -40f - 80f * ind[tab]++;
            content.GetComponent<RectTransform>().localPosition = cPos;
            
            // スコアデータをもとに表示を変更
            
            // content.transform.GetChild(0).GetComponent<Image>().sprite =
            content.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = contentsData[i].name;
            content.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = contentsData[i].sub;
            content.transform.GetChild(4).gameObject.SetActive(contentsData[i].document != -1);
            
            // クリック時
            var i2 = i;
            content.GetComponent<Button>().onClick.AddListener(() => OnClickContent(i2));
        }
        
        for (int i = 0; i < tabsData.Length; i++)
        {
            GameObject tab = tabs[i];
            Transform content = tab.transform.GetChild(1).GetChild(0);
            int height = 80 * ind[i];
            content.GetComponent<CategoryWindowDrag>().h = height;
        }
        
        OnClickTab(0);

        mask.GetComponent<Image>().DOFade(0f, 0.7f)
            .OnComplete(() => mask.SetActive(false));
    }

    public void OnClickTab(int index)
    {
        var bTab = tabs[selectTab].transform;
        Color c = bTab.GetChild(0).GetChild(0).GetComponent<Image>().color;
        c.a = 150f / 255f;
        bTab.GetChild(0).GetChild(0).GetComponent<Image>().color = c;
        bTab.GetChild(1).GetChild(0).gameObject.SetActive(false);
        
        Debug.Log(index);
        var nTab = tabs[index].transform;
        c = nTab.GetChild(0).GetChild(0).GetComponent<Image>().color;
        c.a = 1f;
        nTab.GetChild(0).GetChild(0).GetComponent<Image>().color = c;
        nTab.GetChild(1).GetChild(0).gameObject.SetActive(true);
        
        selectTab = index;
    }
    
    public void OnClickContent(int index)
    {
        if (selectContent != -1)
            contents[selectContent].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        contents[index].GetComponent<Image>().color = new Color(1f, 1f, 1f, 100f / 255f);
        
        selectContent = index;

        var content = contentsData[selectContent];
        right.GetChild(0).GetComponent<Image>().sprite = iconSprites[progressData[selectContent]];
        right.GetChild(1).GetComponent<TextMeshProUGUI>().text = content.name;
        right.GetChild(2).GetComponent<TextMeshProUGUI>().text = content.sub;
        
        // TODO: progressBar, grassの表示を変更
    }

    public void SelectButtonPush()
    {
        if (selectContent == -1) return;
        
        SelectData.division = contentsData[selectContent].division;

        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.SetActive(true);
        mask.GetComponent<Image>().DOFade(1f, 0.7f)
            .OnComplete(() => SceneManager.LoadScene("SelectScene"));
    }
}

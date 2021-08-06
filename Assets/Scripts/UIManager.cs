using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private string[] colorTable = { "#B2DB98", "#BC9D66", "#6953E3", "#FFF14F" };

    public Data data;
    public Text count;
    public GameObject resultPanel;
    public Text resultCount;
    public Text resultLevel;
    
    private void ChangeBackgroundColor(int level)
    {
        string color = colorTable.Where(x => x != "#" + ColorUtility.ToHtmlStringRGB(Camera.main.backgroundColor)).ToArray()[Random.Range(0,3)];

        Color c;

        ColorUtility.TryParseHtmlString(color, out c);
        Debug.Log(color + c.ToString());
        Camera.main.backgroundColor = c;
    }

    public void EventSetUp()
    {
        data.countUpEvent += (x) => count.text = x.ToString();
        data.levelUpEvent += ChangeBackgroundColor;
        GameManager.Instance.gameStartEvent += () => 
        {
            count.text = "0"; 
            resultPanel.SetActive(false); 
        };

        GameManager.Instance.gameEndEvent += () => 
        { 
            resultPanel.SetActive(true);
            resultCount.text = "Count : " + data.count.ToString();
            resultLevel.text = "Level : " + data.level.ToString();
        };
    }

    private void Awake()
    {
        
        EventSetUp();
    }
}

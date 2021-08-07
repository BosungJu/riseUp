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
    public InputField inputField;
    
    private void ChangeBackgroundColor(int level)
    {
        string color = colorTable.Where(x => x != "#" + ColorUtility.ToHtmlStringRGB(Camera.main.backgroundColor)).ToArray()[Random.Range(0,3)];

        Color c;

        ColorUtility.TryParseHtmlString(color, out c);
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
            resultCount.text = "Count : " + GameManager.Instance.playerData.count.ToString();
            resultLevel.text = MatchServer.Instance.resultCode == 1 ? "WIN" : 
                MatchServer.Instance.resultCode == 0 ? "DRAW" : "LOSE"; 
        };
    }

    public void EndEditInputField()
    {
        var coroutine = GameManager.Instance.ConnectServer(inputField.text);
        StartCoroutine(coroutine);
        inputField.gameObject.SetActive(false);
    }

    private void Awake()
    {
        EventSetUp();
    }
}

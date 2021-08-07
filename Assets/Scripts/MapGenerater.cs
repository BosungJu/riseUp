using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerater : MonoBehaviour
{
    public enum PlatType : int
    {
        None = -1,
        blockedAll = 0,
        blockedLeft = 1,
        blockedRight = 2,
        blockedNone = 3
    }

    private Queue<GameObject> blockedAllPool = new Queue<GameObject>();
    private Queue<GameObject> blockedLeftPool = new Queue<GameObject>();
    private Queue<GameObject> blockedRightPool = new Queue<GameObject>();
    private Queue<GameObject> blockedNonePool = new Queue<GameObject>();

    private List<GameObject> blocks = new List<GameObject>();
    public GameObject blockedAll;
    public GameObject blockedLeft;
    public GameObject blockedRight;
    public GameObject blockedNone;
    private List<float> probabilityTable = new List<float>{ 0.3f, 0.315f, 0.315f, 0.07f };

    private float y = 0; 
    public BSLibrary.Tween tween;
    public float jumpTime;
    private bool nowJumping;
    private int _stock;

    public Data data;

    public Player player;
    public OtherPlayer otherPlayer;

    public int stock
    {
        get { return _stock; }
        set
        {
            if (_stock > value)
            {
                PlatType type = GenerateBlock(new Vector3(0, y, 0));
                y += blockedAll.transform.localScale.y;
                _stock = value + 1;
                return;
            }

            _stock = value;
        }
    }

    private PlatType GenerateBlock(Vector3 pos, PlatType type = PlatType.None)
    {
        GameObject obj = null;

        if (type == PlatType.None)
        {
            float num = Random.Range(0f, 1f);
            type = (PlatType)(
                num <= probabilityTable.GetRange(0, 1).Sum() ? 0 :
                num <= probabilityTable.GetRange(0, 1).Sum() + probabilityTable[1] ? 1 :
                num <= probabilityTable.GetRange(0, 2).Sum() + probabilityTable[2] ? 2 :
                num <= probabilityTable.GetRange(0, 3).Sum() + probabilityTable[3] ? 3 : 0);
        }
        //mapData += ((int)type).ToString();
        obj = blocks[(int)type];

        Instantiate(obj, transform).transform.position += pos;

        return type;
    }

    public void Init(int seed) // 장판들 초기 배치
    {
        Random.InitState(seed);

        for (int i = transform.childCount - 1; i >= 1; --i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        transform.position = new Vector3(0, -1, 0);
        y = 0;
        
        GenerateBlock(new Vector3(0, y, 0), PlatType.blockedAll);
        
        y += blockedAll.transform.localScale.y;

        for (int i = 1; i < 15; ++i, y += blockedAll.transform.localScale.y)
        {
            GenerateBlock(new Vector3(0, y, 0));
        }

        stock = 15;
        Debug.Log("initailize complete");
        //StartCoroutine("SendData");
    }

    private void SendUserData()
    {
        MatchServer.Instance.SendUserData(player, otherPlayer);
        //yield return new WaitForSeconds(1.5f);
    }

    private void SendJumpData()
    {
        MatchServer.Instance.SendJumpData(player.transform.position.x, player.transform.eulerAngles.y, data.count);
    }

    private IEnumerator Jump()
    {
        nowJumping = true;
        data.jumpStartEvent();
        tween.StartCoroutine();

        //SendUserData();
        SendJumpData();
        
        Vector3 beforePos = transform.position;

        while (tween.IsPlay)
        {
            transform.position = beforePos - new Vector3(0, blockedAll.transform.localScale.y * tween.ReturnValueToFloat, 0);
            yield return new WaitForFixedUpdate();
        }
        transform.position = beforePos - new Vector3(0, blockedAll.transform.localScale.y, 0);

        yield return new WaitForSeconds(0.05f); // give delay

        if (GameManager.Instance.isPlay) { data.count += 1; }

        if (data.count > GameManager.Instance.otherData.count) { stock--; }
        data.jumpEndEvent();
        nowJumping = false;
    }

    private void Awake()
    {
        // 점프 시간 설정
        tween.RepeatTime = jumpTime;
        nowJumping = false;

        // 장판들 설치할때 용이하도록 리스트에 넣기
        blocks.Add(blockedAll);
        blocks.Add(blockedLeft);
        blocks.Add(blockedRight);
        blocks.Add(blockedNone);

        MatchServer.Instance.StartGame += Init;
        //GameManager.Instance.gameStartEvent += Init;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 점프 (터치, 좌클릭) 시작 버튼 이기도 함.
        {
            Debug.Log("클릭");
            if (!nowJumping && GameManager.Instance.isPlay && MatchServer.Instance.onMatch)
            {
                var coroutine = Jump();
                StartCoroutine(coroutine);
            }
        }
    }

}

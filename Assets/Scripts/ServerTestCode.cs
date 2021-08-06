using BackEnd;
using Battlehub.Dispatcher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerTestCode : MonoBehaviour
{
    private IEnumerator CreateRoom()
    {
        yield return new WaitForSeconds(3f);
        MatchServer.Instance.CreateRoom();
    }

    private IEnumerator JoinMatchServer() 
    {
        yield return new WaitForSeconds(1f);
        MatchServer.Instance.JoinMatchMakingServer();
    }

    private IEnumerator RequestMatchMaking()
    {
        yield return new WaitForSeconds(3f);
        MatchServer.Instance.RequestMatchMaking();
        //MatchServer.Instance.RequestMatchMaking(BackEnd.Tcp.MatchType.Random, BackEnd.Tcp.MatchModeType.OneOnOne, MatchServer.Instance.matchInfos[0].inDate);
    }

    // Start is called before the first frame update
    void Start()
    {
        int timer = 0;
        ServerManager.Instance.GuestLogin();
        StartCoroutine("JoinMatchServer");
        UserDatas.NickName.Value = "a";
        ServerManager.Instance.UpdateNickname((bool result, string error) =>
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                if (!result)
                {
                    //loadingObject.SetActive(false);
                    //errorText.text = "닉네임 생성 오류\n\n" + error;
                    //errorObject.SetActive(true);
                    Debug.Log("닉네임 생성 오류 : " + error);
                    return;
                }
                //ChangeLobbyScene();
            });
        });
        StartCoroutine("CreateRoom");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

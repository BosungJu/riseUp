using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerTestCode : MonoBehaviour
{
    public InputField inputField;

    private IEnumerator CreateRoom()
    {
        yield return new WaitForSeconds(2f);
        MatchServer.Instance.CreateRoom();
        var data = Backend.BMember.GetUserInfo();
        Debug.Log(data.GetInDate());
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

    public void UpdateNickname()
    {
        //while (!ServerManager.Instance.isLogin) { }

        UserDatas.NickName.Value = inputField.text;
        ServerManager.Instance.UpdateNickname((bool result, string error) =>
        {
            //Dispatcher.Current.BeginInvoke(() =>
            //{
            if (!result)
            {
                //loadingObject.SetActive(false);
                //errorText.text = "닉네임 생성 오류\n\n" + error;
                //errorObject.SetActive(true);
                Debug.Log("닉네임 생성 오류 : " + error);
                return;
            }
            //ChangeLobbyScene();
            //});
        });
        inputField.gameObject.SetActive(false);
        
        StartCoroutine("JoinMatchServer");
        StartCoroutine("CreateRoom");

        StartCoroutine("RequestMatchMaking");
    }

    // Start is called before the first frame update
    void Start()
    {
        ServerManager.Instance.GuestLogin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

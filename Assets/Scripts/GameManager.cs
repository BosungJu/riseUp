using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BSLibrary.MonoSingleton<GameManager>
{
    public Data playerData;
    public Data otherData;
    public bool isPlay;
    public Action gameStartEvent;
    public Action gameEndEvent;

    private bool _updateNickname = false;
    private bool updateNickname
    {
        get { return _updateNickname; }
        set
        {
            if (value == true)
            {
                var coroutine = ConnectServer(UserDatas.NickName.Value);
                StartCoroutine(coroutine);
            }

            _updateNickname = value;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ServerManager.Instance.GuestLogin();
        StartGame();
    }

    //public IEnumerator UpdateNickname(string text)
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    UserDatas.NickName.Value = text;
    //    ServerManager.Instance.UpdateNickname((bool result, string error) =>
    //    {
    //        if (!result)
    //        {
    //            Debug.Log("닉네임 생성 오류 : " + error);
    //            return;
    //        }
    //    });
    //    updateNickname = true;
    //}


    public IEnumerator ConnectServer(string text)
    {
        yield return new WaitForSeconds(0.5f);
        UserDatas.NickName.Value = text;
        ServerManager.Instance.UpdateNickname((bool result, string error) =>
        {
            if (!result)
            {
                Debug.Log("닉네임 생성 오류 : " + error);
                return;
            }
        });
        yield return new WaitForSeconds(0.5f);
        MatchServer.Instance.JoinMatchMakingServer();
        yield return new WaitForSeconds(0.5f);
        MatchServer.Instance.CreateRoom();
        yield return new WaitForSeconds(0.5f);
        MatchServer.Instance.RequestMatchMaking();
    }

    public void ReStartGame()
    {
        StartGame();
        if (updateNickname)
        {
            var coroutine = ConnectServer(UserDatas.NickName.Value);
            StartCoroutine(coroutine);
        }
    }

    public void StartGame()
    {
        playerData.level = 1;
        playerData.count = 0;
        otherData.level = 1;
        otherData.count = 0;
        
        isPlay = true;
        gameStartEvent();

        //var coroutine = ConnectServer();
        //StartCoroutine(coroutine);
    }

    public void EndGame()
    {
        isPlay = false;
        gameEndEvent();
    }

    private void OnApplicationQuit()
    {
        ServerManager.Instance.Logout();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

}

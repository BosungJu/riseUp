using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using UnityEngine.UI;
using LitJson;
using System;
using BackEnd.Tcp;

public class MatchServer : Singleton<MatchServer>
{
    private bool isConnectMatchServer = false;
    private bool isConnectInGameServer = false;

    public void JoinMatchServer()
    {
        if (isConnectMatchServer)
        {
            return;
        }

        BackEnd.Tcp.ErrorInfo errorInfo;
        if (!Backend.Match.JoinMatchMakingServer(out errorInfo))
        {
            Debug.Log(errorInfo);
        }
        else
        {
            isConnectMatchServer = true;
            Debug.Log("����");
        }
    }

    public bool CreateRoom()
    {
        if (!isConnectMatchServer)
        {
            Debug.Log("don't Connect Match Server");
            Debug.Log("try Connect...");
            JoinMatchServer();
            return false;
        }

        Backend.Match.CreateMatchRoom();
        return true;
    }

    public void LeaveMatchRoom()
    {
        Backend.Match.LeaveMatchRoom();
    }

    public void InviteUser(string nickName)
    {
        Backend.Match.InviteUser(nickName);
    }

    public void RequestMatchMaking()
    {
        if (!isConnectMatchServer)
        {
            Debug.Log("don't Connect Match Server");
            Debug.Log("try Connect...");
            JoinMatchServer();
            return;
        }

        isConnectMatchServer = false;

        Backend.Match.RequestMatchMaking(BackEnd.Tcp.MatchType.Random, BackEnd.Tcp.MatchModeType.OneOnOne, Backend.Match.GetMatchList().GetInDate());

        if (isConnectInGameServer)
        {
            Backend.Match.LeaveGameServer();
        }
    }

    public void CancelMatchMaking()
    {
        Backend.Match.CancelMatchMaking();
    }

    private void ProcessAccessMatchMakingServer(ErrorInfo errInfo)
    {
        if (errInfo != ErrorInfo.Success)
        {
            // ���� ����
            isConnectMatchServer = false;
        }

        if (!isConnectMatchServer)
        {
            // ���� ����
            Debug.Log(errInfo.Reason);
        }
        else
        {
            //���� ����
            Debug.Log("connect complete");
        }
    }
    /*
    private void ProcessMatchSuccess(MatchMakingResponseEventArgs args)
    {
        ErrorInfo errorInfo;
        if (sessionIdList != null)
        {
            Debug.Log("���� ���� ���� ����");
            sessionIdList.Clear();
        }

        if (!Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address, args.RoomInfo.m_inGameServerEndPoint.m_port, false, out errorInfo))
        {
            var debugLog = string.Format(FAIL_ACCESS_INGAME, errorInfo.ToString(), string.Empty);
            Debug.Log(debugLog);
        }
        // ���ڰ����� �ΰ��� ����ū�� �����صξ�� �Ѵ�.
        // �ΰ��� �������� �뿡 ������ �� �ʿ�
        // 1�� ���� ��� ������ �뿡 �������� ������ �ش� ���� �ı�ȴ�.
        isConnectInGameServer = true;
        isJoinGameRoom = false;
        isReconnectProcess = false;
        inGameRoomToken = args.RoomInfo.m_inGameRoomToken;
        isSandBoxGame = args.RoomInfo.m_enableSandbox;
        var info = GetMatchInfo(args.MatchCardIndate);
        if (info == null)
        {
            Debug.LogError("��ġ ������ �ҷ����� �� �����߽��ϴ�.");
            return;
        }

        nowMatchType = info.matchType;
        nowModeType = info.matchModeType;
        numOfClient = int.Parse(info.headCount);
    }
    */
    
    private void ProcessMatchMakingResponse(MatchMakingResponseEventArgs args)
    {
        
    }
    
    private void MatchMakingHandler()
    {
        Backend.Match.OnJoinMatchMakingServer += (args) =>
        {
            ProcessAccessMatchMakingServer(args.ErrInfo);
        };

        Backend.Match.OnMatchMakingResponse += (args) =>
        {
            ProcessMatchMakingResponse(args);
        };
    }

    private void Start()
    {
        MatchMakingHandler();
    }

    private void Update()
    {
        Backend.Match.Poll();
    }
}

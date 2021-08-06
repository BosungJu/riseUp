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
    public bool isConnectMatchServer = false;
    public bool isConnectInGameServer = false;
    public bool isSuperUser = false;
    public Player player;
    public OtherPlayer otherPlayer;
    public Text text;

    public void JoinMatchMakingServer()
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
            text.text += "��ġ ���� ����\n";
            isConnectMatchServer = true;
            Debug.Log("����");
        }
    }

    public void LeaveMatchMakingServer()
    {
        Backend.Match.LeaveMatchMakingServer();
    }

    public bool CreateRoom()
    {
        if (!isConnectMatchServer)
        {
            Debug.Log("don't Connect Match Server");
            Debug.Log("try Connect...");
            JoinMatchMakingServer();
            return false;
        }
        
        Backend.Match.CreateMatchRoom();
        text.text += "�� ����\n";
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
            JoinMatchMakingServer();
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
    
    private void ProcessMatchMakingResponse(MatchMakingResponseEventArgs args)
    {
        switch (args.ErrInfo)
        {
            case ErrorCode.Success:
                // ��Ī �������� ��
                ProcessMatchSuccess(args);
                break;
            //case ErrorCode.Match_InProgress:
            //    // ��Ī ��û �������� �� or ��Ī ���� �� ��Ī ��û�� �õ����� ��

            //    // ��Ī ��û �������� ��
            //    if (args.Reason == string.Empty)
            //    {
            //        debugLog = SUCCESS_REGIST_MATCHMAKE;

            //        LobbyUI.GetInstance().MatchRequestCallback(true);
            //    }
            //    break;
            case ErrorCode.Match_MatchMakingCanceled:
                RequestMatchMaking();
                break;
        }
    }

    private void ProcessMatchSuccess(MatchMakingResponseEventArgs args)
    {
        ErrorInfo errorInfo;

        if (!Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address, args.RoomInfo.m_inGameServerEndPoint.m_port, false, out errorInfo))
        {
            Debug.Log(errorInfo.Reason);
            return;
        }

        Backend.Match.JoinGameRoom(args.RoomInfo.m_inGameRoomToken);
    }

    public void SendClickData()
    {
        Backend.Match.SendDataToInGameRoom(new byte[]{1});
    }

    public void SendWindowData
        (
        string map, Vector3 userPos, Vector3 otherUserPos, 
        Vector3 userDirection, Vector3 otherDirection,
        string userState, string otherState)
    {
        Backend.Match.SendDataToInGameRoom(
            Convert.FromBase64String(string.Format("{" +
            "map : {{0}}, " +
            "userPos : {1}, " +
            "otherUserPos : {2}}, " +
            "userDirection : {3}, " +
            "otherUserDirection : {4}, " +
            "userState : {5}, " +
            "otherState : {6}}", 
            map, 
            userPos.ToString(),
            otherUserPos.ToString(),
            userDirection.ToString(),
            otherDirection.ToString(),
            userState,
            otherState)));
    }

    private void MatchMakingHandler()
    {
        Backend.Match.OnJoinMatchMakingServer += (args) =>
        {
            ProcessAccessMatchMakingServer(args.ErrInfo);
        };

        Backend.Match.OnLeaveMatchMakingServer += (args) =>
        {
            // TODO leave match making server
        };

        Backend.Match.OnMatchMakingRoomCreate += (args) =>
        {
            // TODO create room
            // TODO �ٷ� ��Ī ����
            RequestMatchMaking();
        };

        Backend.Match.OnMatchMakingResponse += (args) =>
        {
            ProcessMatchMakingResponse(args);
        };

        Backend.Match.OnSessionJoinInServer += (args) => 
        {
            if (args.ErrInfo != ErrorInfo.Success)
            {
                Debug.Log(args.ErrInfo.Reason);
                return;
            }
        };

        Backend.Match.OnSessionListInServer += (args) =>
        {
            
        };

        Backend.Match.OnLeaveInGameServer += (args) =>
        {
            if (args.ErrInfo != ErrorCode.Success)
            {
                Debug.Log(args.Reason);
                return;
            }
        };

        Backend.Match.OnMatchInGameAccess += (args) =>
        {
            if (args.ErrInfo != ErrorCode.Success)
            {
                Debug.Log(args.Reason);
                return;
            }

            text.text += "access complete";
            isSuperUser = args.GameRecord.m_isSuperGamer;
            
        };

        Backend.Match.OnMatchInGameStart += () =>
        {
            Debug.Log("game start");
        };

        Backend.Match.OnMatchRelay += (args) => 
        { 
            // TODO jump
            if (isSuperUser)
            {
                // TODO process jump
                otherPlayer.Jump();
            }
        };

        Backend.Match.OnMatchResult += (args) => // ������ ������ ��������
        {
            if (args.ErrInfo == ErrorCode.Success)
            {

            }
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

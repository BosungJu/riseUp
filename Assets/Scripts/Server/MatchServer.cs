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
    public class MatchInfo
    {
        public string title;                // ��Ī ��
        public string inDate;               // ��Ī inDate (UUID)
        public MatchType matchType;         // ��ġ Ÿ��
        public MatchModeType matchModeType; // ��ġ ��� Ÿ��
        public string headCount;            // ��Ī �ο�
        public bool isSandBoxEnable;        // ����ڽ� ��� (AI��Ī)
    }

    public List<MatchInfo> matchInfos { get; private set; } = new List<MatchInfo>();  // �ֿܼ��� ������ ��Ī ī����� ����Ʈ


    public bool isConnectMatchServer = false;
    public bool isConnectInGameServer = false;
    public bool isSuperUser = false;
    public bool onMatch = false;
    public Player player;
    public OtherPlayer otherPlayer;
    public Text text;

    public void JoinMatchMakingServer()
    {
        if (isConnectMatchServer)
        {
            return;
        }

        isConnectMatchServer = true;

        BackEnd.Tcp.ErrorInfo errorInfo;
        if (!Backend.Match.JoinMatchMakingServer(out errorInfo))
        {
            Debug.Log(errorInfo);
        }
        else
        {
            text.text += "��ġ ���� ����\n";
            Debug.Log(errorInfo.SocketErrorCode);
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
        Debug.Log("RequestMatchMaking");
        if (!isConnectMatchServer)
        {
            Debug.Log("don't Connect Match Server");
            Debug.Log("try Connect...");
            JoinMatchMakingServer();
            return;
        }

        isConnectInGameServer = false;

        Debug.Log("�ΰ��� ����");

        var matchList = Backend.Match.GetMatchList();
        Debug.Log(matchList.GetInDate());
        Backend.Match.RequestMatchMaking(BackEnd.Tcp.MatchType.Random, BackEnd.Tcp.MatchModeType.OneOnOne, matchList.GetInDate());
        if (isConnectInGameServer)
        {
            Debug.Log("����");
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
            Debug.Log(errInfo);
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
                Debug.Log("��Ī ����");
                ProcessMatchSuccess(args);
                break;
            case ErrorCode.Match_InProgress:
                // ��Ī ��û �������� �� or ��Ī ���� �� ��Ī ��û�� �õ����� ��
                Debug.Log("2");
                // ��Ī ��û �������� ��
                if (args.Reason == string.Empty)
                {
                    ProcessMatchSuccess(args);
                }
                break;
            case ErrorCode.InvalidOperation:
                Debug.Log("���� : " + args.Reason);

                break;
            case ErrorCode.Match_MatchMakingCanceled:
                Debug.Log("���� : " + args.Reason);
                RequestMatchMaking();
                break;
        }

        Debug.Log(args.ErrInfo);
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
        onMatch = true;
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
            Debug.Log("OnMatchMakingRoomCreate : " + args.ErrInfo + " : " + args.Reason);

            if (args.ErrInfo.Equals(ErrorCode.Success))
            {
                //RequestMatchMaking();
            }
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

            Debug.Log("game ����");
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

    public void GetMatchList(Action<bool, string> func)
    {
        // ��Ī ī�� ���� �ʱ�ȭ
        matchInfos.Clear();

        Backend.Match.GetMatchList(callback =>
        {
            // ��û �����ϴ� ��� ���û
            if (callback.IsSuccess() == false)
            {
                Debug.Log("��Īī�� ����Ʈ �ҷ����� ����\n" + callback);
                //Dispatcher.Current.BeginInvoke(() =>
                //{
                //GetMatchList(func);
                //});
                return;
            }
            Debug.Log("Get Match List");
            foreach (LitJson.JsonData row in callback.Rows())
            {
                Debug.Log("create ��Ī ī��");

                MatchInfo matchInfo = new MatchInfo();
                matchInfo.title = row["matchTitle"]["S"].ToString();
                matchInfo.inDate = row["inDate"]["S"].ToString();
                matchInfo.headCount = row["matchHeadCount"]["N"].ToString();
                matchInfo.isSandBoxEnable = row["enable_sandbox"]["BOOL"].ToString().Equals("True") ? true : false;

                foreach (MatchType type in Enum.GetValues(typeof(MatchType)))
                {
                    if (type.ToString().ToLower().Equals(row["matchType"]["S"].ToString().ToLower()))
                    {
                        matchInfo.matchType = type;
                    }
                }

                foreach (MatchModeType type in Enum.GetValues(typeof(MatchModeType)))
                {
                    if (type.ToString().ToLower().Equals(row["matchModeType"]["S"].ToString().ToLower()))
                    {
                        matchInfo.matchModeType = type;
                    }
                }

                matchInfos.Add(matchInfo);
            }
            Debug.Log("��Īī�� ����Ʈ �ҷ����� ���� : " + matchInfos.Count);
            func(true, string.Empty);
        });

        
    }
}

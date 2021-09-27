using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using UnityEngine.UI;
using LitJson;
using System;
using BackEnd.Tcp;
using System.Text;

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
    private SessionId mySessionID;
    private SessionId otherSessionID;
    private string nickname = "";
    public bool onMatch = false;
    public Player player;
    public OtherPlayer otherPlayer;
    public Text text;
    public string roomToken = "";
    public MapGenerater mapGenerater;
    public Action<int> StartGame = null;
    public bool isGameStart = false;
    public int resultCode = 0;

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
                Debug.Log("�����");
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

        roomToken = args.RoomInfo.m_inGameRoomToken;

        if (!Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address, args.RoomInfo.m_inGameServerEndPoint.m_port, false, out errorInfo))
        {
            Debug.Log(errorInfo.Reason);
            return;
        }
    }

    public void SendJumpData(float x, float direction,int count)
    {
        var data = DataParser.DataToJsonData<Protocol.JumpMessage>(new Protocol.JumpMessage(x, direction, count));

        Backend.Match.SendDataToInGameRoom(data);
    }

    public void SendCollapse()
    {
        var data = DataParser.DataToJsonData<Protocol.Collapse>(new Protocol.Collapse(mySessionID, otherSessionID, player.data.count, otherPlayer.data.count));
        Debug.Log("SendCollapse : " + isSuperUser);
        Backend.Match.SendDataToInGameRoom(data);
    }

    public void SendUserData(Player player, OtherPlayer otherPlayer)
    {

        Protocol.UserMassage message = new Protocol.UserMassage(
            player.transform.position.x,
            otherPlayer.transform.position.x,
            player.data.count,
            otherPlayer.data.count,
            (int)player.transform.eulerAngles.y,
            (int)otherPlayer.transform.eulerAngles.y);

        var data = DataParser.DataToJsonData<Protocol.UserMassage>(message);

        Backend.Match.SendDataToInGameRoom(data);
    }

    public void SendSeedData()
    {
        System.Random rand = new System.Random();
        
        var data = DataParser.DataToJsonData<Protocol.SeedMessage>(new Protocol.SeedMessage(rand.Next()));
        Backend.Match.SendDataToInGameRoom(data);
        //mapGenerater.Init(DataParser.ReadJsonData<Protocol.SeedMessage>(data).seed);
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

            // Debug.Log("game ����");
            Backend.Match.JoinGameRoom(roomToken);
            onMatch = true;
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

            if (nickname == "")
            {
                nickname = args.GameRecord.m_nickname;
                mySessionID = args.GameRecord.m_sessionId;
                isSuperUser = args.GameRecord.m_isSuperGamer;
            }
            else
            {
                otherSessionID = args.GameRecord.m_sessionId;
            }
            Debug.Log("���� ���� : " + isSuperUser);
            Debug.Log("���� ������ ����");
        };

        Backend.Match.OnMatchInGameStart += () =>
        {
            text.gameObject.SetActive(false);
            isGameStart = true;
            Debug.Log("game start : " + isGameStart);
        };

        Backend.Match.OnMatchRelay += (args) => 
        {
            if (args.BinaryUserData == null)
            {
                return;
            }

            Protocol.Message msg = DataParser.ReadJsonData<Protocol.Message>(args.BinaryUserData);
            

            if (msg == null)
            {
                return;
            }
            if (args.From.SessionId == mySessionID && !isSuperUser)
            {
                return;
            }

            Debug.Log(Encoding.UTF8.GetString(args.BinaryUserData));

            switch (msg.type)
            {
                case Protocol.Type.Jump:
                    // TODO other player jump
                    if (args.From.SessionId == mySessionID) return;
                    Protocol.JumpMessage jumpMessage = DataParser.ReadJsonData<Protocol.JumpMessage>(args.BinaryUserData);

                    otherPlayer.PlayJump(jumpMessage);
                    break;
                case Protocol.Type.Collapse:
                    // TODO End Game
                    Protocol.Collapse collapse = DataParser.ReadJsonData<Protocol.Collapse>(args.BinaryUserData);

                    int myCount = collapse.mySessionID == mySessionID ? collapse.myCount : collapse.myCount + 1;
                    int otherCount = collapse.otherSessionID == otherSessionID ? collapse.otherCount : collapse.otherCount + 1;
                    MatchGameResult matchGameResult = new MatchGameResult();
                    matchGameResult.m_winners = new List<SessionId>();
                    matchGameResult.m_losers = new List<SessionId>();
                    matchGameResult.m_draws = new List<SessionId>();
                    
                    if (myCount > otherCount)
                    {
                        matchGameResult.m_winners.Add(mySessionID);
                        matchGameResult.m_losers.Add(otherSessionID);
                        if (collapse.mySessionID == mySessionID)
                        {
                            resultCode = 1;
                        }
                        else
                        {
                            resultCode = -1;
                        }
                    }
                    else if (myCount < otherCount)
                    {
                        matchGameResult.m_winners.Add(otherSessionID);
                        matchGameResult.m_losers.Add(mySessionID);
                        if (collapse.mySessionID == mySessionID)
                        {
                            resultCode = -1;
                        }
                        else
                        {
                            resultCode = 1;
                        }
                    }
                    else
                    {
                        matchGameResult.m_draws.Add(mySessionID);
                        matchGameResult.m_draws.Add(otherSessionID);
                        resultCode = 0;
                    }

                    if (isSuperUser) Backend.Match.MatchEnd(matchGameResult);


                    break;
                case Protocol.Type.UserData:
                    // TODO map ����ȭ and player position ����ȭ
                    Debug.Log("get map data : " + isSuperUser);
                    if (!isSuperUser)
                    {
                        Protocol.UserMassage mapData = JsonUtility.FromJson<Protocol.UserMassage>(Encoding.UTF8.GetString(args.BinaryUserData));
                        otherPlayer.transform.position = new Vector3(
                            mapData.superUserPos_x,
                            0,
                            0);
                        otherPlayer.pivot.position = new Vector3(
                            0,
                            -1 + mapGenerater.blockedAll.transform.localScale.y * (mapData.userCount - mapData.superUserCount),
                            0);
                        otherPlayer.transform.eulerAngles = new Vector3(0, mapData.superUserDirection, 0);
                        player.transform.position = new Vector3(mapData.userPos_x, -1, 0);
                        player.transform.eulerAngles = new Vector3(0, mapData.userDirection, 0);
                    }
                    break;
                case Protocol.Type.Seed:
                    Debug.Log("get seed");

                    Protocol.SeedMessage seed = DataParser.ReadJsonData<Protocol.SeedMessage>(args.BinaryUserData);
                    StartGame(seed.seed);

                    break;
                case Protocol.Type.GameStart:

                    break;
                case Protocol.Type.GameEnd:
                    // TODO show result window
                    break;
            }

            //// TODO jump
            //if (isSuperUser && args.BinaryUserData.Length == 1)
            //{
            //    // TODO process jump
            //    otherPlayer.Jump();
            //}
        };

        Backend.Match.OnMatchResult += (args) => // ������ ������ ��������
        {
            if (args.ErrInfo == ErrorCode.Success)
            {
                isGameStart = false;
                GameManager.Instance.EndGame();
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

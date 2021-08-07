using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using UnityEngine.UI;
using LitJson;
using System;
using static BackEnd.SendQueue;

public class ServerManager : Singleton<ServerManager>
{
    
    //비동기로 가입, 로그인을 할때에는 Update()에서 처리를 해야합니다. 이 값은 Update에서 구현하기 위한 플래그 값 입니다.
    BackendReturnObject bro = new BackendReturnObject();
    bool isSuccess = false;

    string updatedPW = "c8xtwbeu";
    string email = "초기화된 비밀번호 받을 이메일 주소";
    private Action<bool, string> loginSuccessFunc = null;

    public string myNickName { get; private set; } = string.Empty;
    public string myIndate { get; private set; } = string.Empty;
    public bool isLogin { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        //큐 가동.
            SendQueue.StartSendQueue(true);

        MatchServer.Instance.text.text = "시작\n";
        Backend.Initialize(HandleBackendCallback, true);
        
    }

    private void Update()
    {
        if (SendQueue.IsInitialize == true)
        {
            SendQueue.Poll();
        }
    }

    void HandleBackendCallback()
    {
        if (Backend.IsInitialized)
        {
            // 구글 해시키 획득 
            if (!Backend.Utils.GetGoogleHash().Equals(""))
                Debug.Log(Backend.Utils.GetGoogleHash());

            // 서버시간 획득
            Debug.Log(Backend.Utils.GetServerTime());

      

            
            /*
            if (PlayerPrefs.GetInt("isLogin") == 1)
            {
                PopUpCanvas.Instance.OpenLoadingPopup();
                ACustomLogin(PlayerPrefs.GetString("ID"), PlayerPrefs.GetString("PW"), ()=> PopUpCanvas.Instance.CloseLoadingPopup(), () => PopUpCanvas.Instance.CloseLoadingPopup());
            }
            else*/
            {
                AGuestLogin();
            }
        }
        // 실패
        else
        {
            Debug.LogError("Failed to initialize the backend");
        }
    }
    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        StopSendQueue();
    }

    // 게임 시작, 게임 종료, 백그라운드로 돌아갈 때(홈버튼 누를 때) 호출됨
    // 위의 종료함수와는 달리 무조건 호출됨
    // 비동기 큐 종료, 재시작
    void OnApplicationPause(bool isPause)
    {
        Debug.Log("OnApplicationPause : " + isPause);
        if (isPause == false)
        {
            ResumeSendQueue();
        }
        else
        {
            PauseSendQueue();
        }
    }

    bool InputFieldEmptyCheck(InputField inputField)
    {
        return inputField != null && !string.IsNullOrEmpty(inputField.text);
    }

    // 커스텀 가입
    public void CustomSignUp()
    {
        Debug.Log("-------------CustomSignUp-------------");
        if (UserDatas.ID.Value != "" && UserDatas.PW.Value != "")
        {
            Debug.Log(Backend.BMember.CustomSignUp(UserDatas.ID.Value, UserDatas.PW.Value, "tester").ToString());
        }
        else
        {
            Debug.Log("check IDInput or PWInput");
        }
    }

    public void ACustomSignUp(string ID, string PW, Action OnSuccess, Action OnFail)
    {
        Debug.Log("-------------ACustomSignUp-------------");
        if (ID != "" && PW != "")
        {
            Backend.BMember.CustomSignUp(ID, PW, "tester", isComplete =>
            {
                Debug.Log(isComplete.ToString());
                if(isComplete.IsSuccess())
                {
                    InsertData();
                    OnSuccess();
                }
                else
                {
                    OnFail();
                }
            });
        }
        else
        {
            Debug.Log("check IDInput or PWInput");
            OnFail();
        }
    }

    public void AddItem(string itemID)
    {
        Param param = new Param();
        param.Add("ItemID", itemID);

        Backend.GameData.Insert("Item", param);


    }

    public void GetItems()
    {
        var bro = Backend.GameData.GetMyData("Item", new Where(), 10);
        if (bro.IsSuccess() == false)
        {
            // 요청 실패 처리
            Debug.Log(bro);
            return;
        }
        for (int i = 0; i < bro.Rows().Count; ++i)
        {
            var inDate = bro.Rows()[i]["ItemID"]["S"].ToString();
            Debug.Log(inDate);
        }
    }

    public void UpdateUserData()
    {
        Param param = new Param();
        param.Add("Score", UserDatas.Score.Value);
        Where where = new Where();
        Backend.GameData.Update("Data", where, param);
    }

    public void InsertData()
    {
        Param param = new Param();
        param.Add("Score", 0);
        Backend.GameData.Insert("Data", param);
    }

    private void SetDatas(string ID, string PW)
    {
        UserDatas.ID.Value = ID;
        UserDatas.PW.Value = PW;
        UserDatas.NickName.Value = Backend.UserNickName;
        UserDatas.isLogin.Value = true;

        var bro = Backend.GameData.GetMyData("Data", new Where(), 10);
        for (int i = 0; i < bro.Rows().Count; ++i)
        {
            UserDatas.Score.Value = int.Parse(bro.Rows()[i]["Score"]["N"].ToString());
        }
    }

    // 커스텀 로그인
    public void CustomLogin(string ID, string PW, Action successCallBack, Action failCallBack)
    {
        Debug.Log("-------------CustomLogin-------------");
        if (ID != "" && PW != "")
        {
            BackendReturnObject returnObj = Backend.BMember.CustomLogin(ID, PW);
            if(returnObj.IsSuccess())
            {
                Debug.Log(Backend.BMember.CustomLogin(ID, PW, "tester").ToString());
                SetDatas(ID, PW);
                successCallBack();
            }
            else
            {
                Debug.Log("login failed");
                failCallBack();
            }
        }
        else
        {
            Debug.Log("check IDInput or PWInput");
            failCallBack();
        }
    }

    private void OnAutoLogin()
    {
        PlayerPrefs.SetInt("isLogin", 1);
        PlayerPrefs.SetString("ID", UserDatas.ID.Value);
        PlayerPrefs.SetString("PW", UserDatas.PW.Value);
    }

    private void OffAutoLogin()
    {
        PlayerPrefs.SetInt("isLogin", 0);
        PlayerPrefs.DeleteKey("ID");
        PlayerPrefs.DeleteKey("PW");
    }

    public void UpdateUserScoreRank()
    {
        Param param = new Param();
        param.Add("Score", UserDatas.Score.Value);

        Where where = new Where();
        where.Equal("owner_inDate", Backend.BMember.GetUserInfo().GetInDate());
        Debug.Log(Backend.BMember.GetUserInfo().GetInDate());

        string[] select = { "inDate" };
        var bro = Backend.GameData.Get("Data", where);

        if (bro.IsSuccess() == false)
        {
            // 요청 실패 처리
            Debug.Log(bro);
            return;
        }
        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
        {
            // 요청이 성공해도 where 조건에 부합하는 데이터가 없을 수 있기 때문에
            // 데이터가 존재하는지 확인
            Debug.Log(bro);
            return;
        }
        string inData = bro.Rows()[0]["inDate"]["S"].ToString();
        Debug.Log(inData);
        var retuned = Backend.URank.User.UpdateUserScore("a57a8b80-e268-11eb-9840-d53bae0fb2d5", "Data", inData, param);
        Debug.Log(retuned);
    }

    public void LoadChart()
    {
        var returned = Backend.Chart.GetChartContents("25097");
        Debug.Log("char count : " + returned.Rows().Count);
    }

    public void ACustomLogin(string ID, string PW, Action successCallBack, Action failCallBack)
    {
        Debug.Log("-------------ACustomLogin-------------");
        if (ID != "" && PW != "")
        {
            Backend.BMember.CustomLogin( ID, PW, "tester", isComplete =>
            {
                if(isComplete.IsSuccess())
                {
                    Debug.Log(isComplete.ToString());
                    SetDatas(ID, PW);
                    OnAutoLogin();
                    successCallBack();
                }
                else
                {
                    failCallBack();
                }
                
            });
        }
        else
        {
            Debug.Log("check IDInput or PWInput");
            failCallBack();
        }
    }

    //뒤끝 RefreshToken 을 통해 뒤끝 AccessToken 을 재발급 받습니다
    public void RefreshTheBackendToken()
    {
        Debug.Log("-------------RefreshTheBackendToken-------------");
        Debug.Log(Backend.BMember.RefreshTheBackendToken().ToString());
    }

    public void ARefreshTheBackendToken()
    {
        Debug.Log("-------------ARefreshTheBackendToken-------------");
        // RefreshTheBackendToken 대신 RefreshTheBackendTokenAsync 사용
        SendQueue.Enqueue(Backend.BMember.RefreshTheBackendToken, isComplete =>
        {
            // 성공시 - Update() 문에서 토큰 저장
            Debug.Log(isComplete.ToString());
            isSuccess = isComplete.IsSuccess();
            bro = isComplete;
        });
    }

    // 서버에서 뒤끝 access_token과 refresh_token을 삭제
    public void Logout()
    {
        Debug.Log("-------------Logout-------------");
        Debug.Log(Backend.BMember.Logout().ToString());
    }

    public void ALogout(Action callBack, Action fail)
    {
        Debug.Log("-------------ALogout-------------");
        Backend.BMember.Logout( isComplete =>
        {
            Debug.Log(isComplete.ToString());
            if(isComplete.IsSuccess())
            {
                UserDatas.isLogin.Value = false;
                UserDatas.ID.Value = "";
                OffAutoLogin();
                callBack();
            }
            else
            {
                fail();
            }
            
        });
    }

    // 회원 탈퇴 
    public void SignOut()
    {
        Debug.Log("-------------SignOut-------------");
        Debug.Log(Backend.BMember.SignOut("탈퇴 사유").ToString());
    }

    public void ASignOut()
    {
        Debug.Log("-------------ASignOut-------------");
        SendQueue.Enqueue(Backend.BMember.SignOut, "탈퇴 사유", isComplete =>
        {
            Debug.Log(isComplete.ToString());
        });
    }

    public void CheckNicknameDuplication()
    {
        Debug.Log("-------------CheckNicknameDuplication-------------");
        if (UserDatas.NickName.Value != "")
        {
            Debug.Log(Backend.BMember.CheckNicknameDuplication(UserDatas.NickName.Value).ToString());
        }
        else
        {
            Debug.Log("check NicknameInput");
        }
    }

    public void ACheckNicknameDuplication()
    {
        Debug.Log("-------------A CheckNicknameDuplication-------------");

        if (UserDatas.NickName.Value != "")
        {
            SendQueue.Enqueue(Backend.BMember.CheckNicknameDuplication, UserDatas.NickName.Value, bro =>
            {
                Debug.Log(bro);
            });
        }
        else
        {
            Debug.Log("check NicknameInput");
        }
    }
    

    // 닉네임 생성 
    public void CreateNickname()
    {
        Debug.Log("-------------CreateNickname-------------");
        if (UserDatas.NickName.Value != "")
        {
            Debug.Log(Backend.BMember.CreateNickname(UserDatas.NickName.Value).ToString());
        }
        else
        {
            Debug.Log("check NicknameInput");
        }
    }
    

    public void ACreateNickname()
    {
        Debug.Log("-------------ACreateNickname-------------");
        if (UserDatas.NickName.Value != "")
        {
            SendQueue.Enqueue(Backend.BMember.CreateNickname, UserDatas.NickName.Value, isComplete =>
            {
                Debug.Log(isComplete.ToString());
            });
        }
        else
        {
            Debug.Log("check NicknameInput");
        }
    }

    // 닉네임 수정
    public void UpdateNickname(Action<bool, string> func)
    {
        Debug.Log("-------------UpdateNickname-------------");
        if (UserDatas.NickName.Value != "")
        {
            SendQueue.Enqueue(Backend.BMember.UpdateNickname, UserDatas.NickName.Value, bro =>
            {
                loginSuccessFunc = func;
                // 닉네임이 없으면 매치서버 접속이 안됨
                if (!bro.IsSuccess())
                {
                    Debug.LogError("닉네임 생성 실패\n" + bro.ToString());
                    loginSuccessFunc(false, string.Format("statusCode : {0}\nErrorCode : {1}\nMessage : {2}", bro.GetStatusCode(), bro.GetErrorCode(), bro.GetMessage()));
                    return;
                }

                OnBackendAuthorized();
            });
        }
        else
        {
            Debug.Log("check Nickname Input");
        }
    }

    // 유저 정보 불러오기 사전작업
    private void OnPrevBackendAuthorized()
    {
        isLogin = true;

        OnBackendAuthorized();
    }

    // 실제 유저 정보 불러오기
    private void OnBackendAuthorized()
    {
        SendQueue.Enqueue(Backend.BMember.GetUserInfo, callback =>
        {
            if (!callback.IsSuccess())
            {
                Debug.LogError("유저 정보 불러오기 실패\n" + callback);
                loginSuccessFunc(false, string.Format("statusCode : {0}\nErrorCode : {1}\nMessage : {2}",
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                return;
            }
            Debug.Log("유저정보\n" + callback);

            var info = callback.GetReturnValuetoJSON()["row"];
            if (info["nickname"] == null)
            {
                //LoginUI.GetInstance().ActiveNickNameObject();
                return;
            }
            myNickName = info["nickname"].ToString();
            myIndate = info["inDate"].ToString();

            if (loginSuccessFunc != null)
            {
                MatchServer.Instance.GetMatchList(loginSuccessFunc);
                // loginSuccessFunc(true, string.Empty);
            }
        });
    }

    public void AUpdateNickname()
    {
        Debug.Log("-------------AUpdateNickname-------------");
        if (UserDatas.NickName.Value != "")
        {
            SendQueue.Enqueue(Backend.BMember.UpdateNickname, UserDatas.NickName.Value, isComplete =>
            {
                Debug.Log(isComplete.ToString());
            });
        }
        else
        {
            Debug.Log("check NicknameInput");
        }
    }

    // 유저 정보 받아오기 - nickname
    public void GetUserInfo()
    {
        Debug.Log("-------------GetUserInfo-------------");
        BackendReturnObject userinfo = Backend.BMember.GetUserInfo();
        Debug.Log(userinfo);

        //text.text = userinfo.ToString();


        if (userinfo.IsSuccess())
        {
            JsonData Userdata = userinfo.GetReturnValuetoJSON()["row"];
            JsonData nicknameJson = Userdata["nickname"];

            // 닉네임 여부를 확인 하는 로직
            if (nicknameJson != null)
            {
                string nick = nicknameJson.ToString();
                Debug.Log("NickName is NOT null which is " + nick);
            }
            else
            {
                Debug.Log("NickName is null");
            }
        }

    }

    public void AGetUserInfo()
    {
        Debug.Log("-------------AGetUserInfo-------------");
        SendQueue.Enqueue(Backend.BMember.GetUserInfo, userinfo =>
        {
            Debug.Log(userinfo.ToString());
            JsonData Userdata = userinfo.GetReturnValuetoJSON()["row"];
            JsonData nicknameJson = Userdata["nickname"];

            // 닉네임 여부를 확인 하는 로직
            if (nicknameJson != null)
            {
                string nick = nicknameJson.ToString();
                Debug.Log("NickName is NOT null which is " + nick);
            }
            else
            {
                Debug.Log("NickName is null");
            }
        });
    }

    // 푸시 토큰 입력
    public void PutDeviceToken()
    {
        Debug.Log("-------------PutDeviceToken-------------");
#if UNITY_ANDROID
        try
        {
            bro = Backend.Android.PutDeviceToken();
            Debug.Log(bro);
            //text.text = bro.ToString();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
#else
        Debug.Log(Backend.iOS.PutDeviceToken(isDevelopment.iosDev));
#endif
    }

    public void APutDeviceToken()
    {
        Debug.Log("-------------APutDeviceToken-------------");
#if UNITY_ANDROID
        SendQueue.Enqueue(Backend.Android.PutDeviceToken, Backend.Android.GetDeviceToken(), bro =>
        {
            Debug.Log(bro);
        });
#else
        SendQueue.Enqueue(Backend.iOS.PutDeviceToken, isDevelopment.iosDev, bro =>
        {
            Debug.Log(bro);
        });
#endif
    }

    // 푸시 토큰 삭제
    public void DeleteDeviceToken()
    {
        Debug.Log("-------------DeleteDeviceToken-------------");
#if UNITY_ANDROID
        Debug.Log(Backend.Android.DeleteDeviceToken());
#else
        Debug.Log(Backend.iOS.DeleteDeviceToken());
#endif
    }

    public void ADeleteDeviceToken()
    {
        Debug.Log("-------------ADeleteDeviceToken-------------");
#if UNITY_ANDROID
        SendQueue.Enqueue(Backend.Android.DeleteDeviceToken, bro =>
        {
            Debug.Log(bro);
        });
#else
        SendQueue.Enqueue(Backend.iOS.DeleteDeviceToken, bro =>
        {
            Debug.Log(bro);
        });
#endif
    }


    public void IsAccessTokenAlive()
    {
        Debug.Log("-------------IsAccessTokenAlive-------------");
        Debug.Log(Backend.BMember.IsAccessTokenAlive().ToString());
    }


    public void AIsAccessTokenAlive()
    {
        Debug.Log("-------------A IsAccessTokenAlive-------------");
        SendQueue.Enqueue(Backend.BMember.IsAccessTokenAlive, callback =>
        {
            Debug.Log(callback);
        });
    }



    public void UpdatePasswordResetEmail()
    {
        Debug.Log("-------------UpdatePasswordResetEmail-------------");
        bro = Backend.BMember.UpdateCustomEmail(email);
        Debug.Log(bro);
    }

    public void AUpdatePasswordResetEmail()
    {
        Debug.Log("-------------A UpdatePasswordResetEmail-------------");
        SendQueue.Enqueue(Backend.BMember.UpdateCustomEmail, email, callback =>
        {
            Debug.Log(callback);
        });
    }

    public void ResetPassword()
    {
        Debug.Log("-------------ResetPassword-------------");
        if (UserDatas.ID.Value != "")
        {
            bro = Backend.BMember.ResetPassword(UserDatas.ID.Value, email);
            Debug.Log(bro);
        }
        else
        {
            Debug.Log("check IDInput");
        }
    }

    public void AResetPassword()
    {
        Debug.Log("-------------A ResetPassword-------------");
        if (UserDatas.ID.Value != "")
        {
            SendQueue.Enqueue(Backend.BMember.ResetPassword, UserDatas.ID.Value, email, callback =>
            {
                Debug.Log(callback);
            });
        }
        else
        {
            Debug.Log("check IDInput");
        }
    }

    public void UpdatePassword()
    {
        Debug.Log("-------------UpdatePassword-------------");
        if (UserDatas.PW.Value != "")
        {
            bro = Backend.BMember.UpdatePassword(updatedPW, UserDatas.PW.Value);
            Debug.Log(bro);
        }
        else
        {
            Debug.Log("check PWInput");
        }

    }

    public void AUpdatePassword()
    {
        Debug.Log("-------------A UpdatePassword-------------");
        if (UserDatas.PW.Value != "")
        {
            SendQueue.Enqueue(Backend.BMember.UpdatePassword, updatedPW, UserDatas.PW.Value, callback =>
            {
                Debug.Log(callback);
            });
        }
        else
        {
            Debug.Log("check PWInput");
        }

    }

    public void GuestLogin()
    {
        Debug.Log("-------------GuestLogin-------------");

        Action<bool, string> func = (bool result, string error) =>
        {
            if (!result)
            {
                Debug.Log("guest login error : " + error);
                return;
            }

            //Dispatcher.Current.BeginInvoke(() =>
            //{
                
            //});
        };

        //bro = Backend.BMember.GuestLogin();

        SendQueue.Enqueue(Backend.BMember.GuestLogin, callback => 
        {
            if (callback.IsSuccess())
            {
                Debug.Log("게스트 로그인 성공");
                loginSuccessFunc = func;

                OnPrevBackendAuthorized();
                return;
            }

            Debug.Log("게스트 로그인 실패\n" + callback);
            func(false, string.Format("statusCode : {0}\nErrorCode : {1}\nMessage : {2}",
                callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
        });
        
        //Debug.Log(bro);
        MatchServer.Instance.text.text += "게스트 로그인 성공\n";
    }
    public void AGuestLogin()
    {
        Debug.Log("-------------A GuestLogin-------------");

        Backend.BMember.GuestLogin( callback =>
        {
            LoadChart();
            Debug.Log(callback);
        });

    }
    public void GetGuestID()
    {
        Debug.Log("-------------GetGuestID-------------");
        Debug.Log("게스트 아이디 : " + Backend.BMember.GetGuestID());
    }

    public void DeleteGuestInfo()
    {
        Debug.Log("-------------DeleteGuestInfo-------------");
        Backend.BMember.DeleteGuestInfo();
    }

    public void GetRandomCardList(Action OnSuccess, Action OnFail)
    {
        Backend.Probability.GetProbabilityCardList((callback) =>
        {
            // 이후 처리
            if(callback.IsSuccess())
            {
                Debug.Log(callback.GetReturnValuetoJSON().ToJson());
                OnSuccess();
            }
            else
            {
                OnFail();
            }
        });
    }

    public void GetRandomCard(Action OnSuccess, Action OnFail)
    {
        Backend.Probability.GetProbability("2334", (callback) =>
        {
            // 이후 처리
            if (callback.IsSuccess())
            {
                Debug.Log(callback.GetReturnValuetoJSON().ToJson());
                OnSuccess();
            }
            else
            {
                OnFail();
            }
        });
    }
}

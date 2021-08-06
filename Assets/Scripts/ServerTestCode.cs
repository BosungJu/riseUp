using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerTestCode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ServerManager.Instance.GuestLogin();
        MatchServer.Instance.JoinMatchMakingServer();
        MatchServer.Instance.CreateRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

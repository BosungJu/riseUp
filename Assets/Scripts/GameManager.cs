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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        playerData.level = 1;
        playerData.count = 0;
        
        isPlay = true;
        Debug.Log(gameStartEvent.Method.Name);
        gameStartEvent();
    }

    public void EndGame()
    {
        isPlay = false;
        gameEndEvent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

}

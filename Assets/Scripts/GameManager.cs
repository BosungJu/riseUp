using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BSLibrary.MonoSingleton<GameManager>
{
    public Data data;
    public bool isPlay;

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
        data.level = 1;
        data.count = 0;
        
        isPlay = true;
        Debug.Log(data.gameStartEvent.Method.Name);
        data.gameStartEvent();
    }

    public void EndGame()
    {
        isPlay = false;
        data.gameEndEvent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

}

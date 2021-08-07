using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Data data;
    public Animator animator;

    private float[] speedTable = { 1, 1.1f, 1.15f, 1.2f, 1.25f };
    private Vector3 direction;
    private float speed;

    private void EventSetUp()
    {
        data.levelUpEvent += LevelUpEvent;
        data.jumpStartEvent += () => animator.SetBool("IsRunning", false);
        data.jumpEndEvent += () => animator.SetBool("IsRunning", true);
        GameManager.Instance.gameStartEvent += GameStartEvent;
    }

    // event function
    private void LevelUpEvent(int level) // level 1 ~ n
    {
        if (level <= 5)
        {
            speed = speedTable[level - 1];
        }
    }
    
    private void Collapse()
    {
        animator.SetBool("IsCollapse", true);
    }

    private void Start()
    {
        EventSetUp();
    }

    private void GameStartEvent()
    {
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsCollapse", false);
        transform.position = new Vector3(0, -1, 0);
        transform.eulerAngles = new Vector3(0, 0, 0);
        direction = Vector3.left;
        speed = speedTable[0];
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.isPlay) { transform.Translate(direction * speed * Time.deltaTime * 2); }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall")) 
        {
            if (collision.transform.position.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cliff"))
        {
            // TODO turn end animation
            GameManager.Instance.EndGame();
            Collapse();
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    public Data data;
    public Data playerData;
    public Animator animator;
    public GameObject plat;

    public float target_y;

    private float[] speedTable = { 1, 1.1f, 1.15f, 1.2f, 1.25f };
    private Vector3 direction;
    private float speed;
    public float jumpTime;
    public BSLibrary.Tween tween;
    public Transform pivot;

    public bool nowJumping { get; private set; }  = false;

    private void EventSetUp()
    {
        data.jumpStartEvent += () => animator.SetBool("IsRunning", false);
        data.jumpEndEvent += () => animator.SetBool("IsRunning", true);
        GameManager.Instance.gameStartEvent += GameStartEvent;
    }

    private void Collapse()
    {
        animator.SetBool("IsCollapse", true);
        MatchServer.Instance.SendCollapse();
    }


    private void Awake()
    {
        EventSetUp();
    }

    private void GameStartEvent()
    {
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsCollapse", false);
        pivot.position = new Vector3(0, 0, 0);
        transform.position = new Vector3(0, 0, 0);
        transform.eulerAngles = new Vector3(0, 0, 0);
        direction = Vector3.left;
        speed = speedTable[0];
        target_y = transform.position.y;
    }

    void FixedUpdate()
    {
        if (MatchServer.Instance.isGameStart) 
        {
            transform.Translate(direction * speed * Time.deltaTime * 2);
        }
    }

    public void PlayJump(Protocol.JumpMessage jumpMessage)
    {
        //transform.position = new Vector3(jumpMessage.userPos_x, 
        //    jumpMessage.count * plat.transform.localScale.y, 
        //    0);
        //transform.eulerAngles = new Vector3(0, jumpMessage.userDirection, 0);
        //data.count = jumpMessage.count;
        if (!nowJumping) 
        { 
            var coroutine = Jump();
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator Jump()
    {
        nowJumping = true;
        data.jumpStartEvent();
        tween.StartCoroutine();
        //target_y += plat.transform.localScale.y;
        Vector3 beforePos = pivot.position;

        while (tween.IsPlay)
        {
            pivot.position = beforePos + new Vector3(0, plat.transform.localScale.y * tween.ReturnValueToFloat, 0);
            yield return new WaitForFixedUpdate();
        }
        pivot.position = beforePos + new Vector3(0, plat.transform.localScale.y, 0);

        yield return new WaitForSeconds(0.05f);
        
        if (GameManager.Instance.isPlay) { data.count += 1; }

        if (data.count > GameManager.Instance.playerData.count) { MatchServer.Instance.mapGenerater.stock--; }

        data.jumpEndEvent();
        nowJumping = false;
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

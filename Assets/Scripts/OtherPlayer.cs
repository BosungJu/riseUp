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

    public bool nowJumping { get; private set; }  = false;

    private void EventSetUp()
    {
        GameManager.Instance.gameStartEvent += GameStartEvent;
    }

    private void Collapse()
    {
        animator.SetBool("IsCollapse", true);
    }


    private void Awake()
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
        target_y = transform.position.y;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.isPlay && MatchServer.Instance.mapGenerater.mapData.Length != 0) 
        {
            transform.Translate(direction * speed * Time.deltaTime * 2);
        }
        
        //if (transform.position.y > target_y)
        //{
        //    transform.Translate(Vector3.down * plat.transform.localScale.y);
        //}
        //else if (transform.position.y < target_y)
        //{
        //    transform.Translate(Vector3.up * plat.transform.localScale.y);
        //}
    }

    public void PlayJump()
    {
        if (!nowJumping) StartCoroutine("Jump");
    }

    private IEnumerator Jump()
    {
        nowJumping = true;
        //target_y += plat.transform.localScale.y;
        Vector3 beforePos = transform.position;

        while (tween.IsPlay)
        {
            transform.position = beforePos + new Vector3(0, plat.transform.localScale.y * tween.ReturnValueToFloat, 0);
            yield return new WaitForFixedUpdate();
        }
        transform.position = beforePos + new Vector3(0, plat.transform.localScale.y, 0);

        yield return new WaitForSeconds(0.05f);
        if (GameManager.Instance.isPlay) { data.count++; }
        if (data.count > MatchServer.Instance.mapGenerater.data.count) { MatchServer.Instance.mapGenerater.stock--; }
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

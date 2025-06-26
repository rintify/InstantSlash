using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Chara
{
    public float moveSpeed = 5.0f;   // 移動速度
    public float turnSpeed = 10.0f;  // 回転の補間速度

    public AudioClip killed;
    Player player;
    bool attack = false;

    Vector2 preMove;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        GameManager.Instance.enemies.Add(this);
        this.Delay(() => { attack = true; }, 4);
    }

    void Update()
    {
        if (death)
        {
            var targetRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            transform.localScale *= Mathf.Pow(0.2f, Time.deltaTime);
            Destroy(gameObject, 1);
            return;
        }
        else if (player.sword != null)
        {

            Vector2 moveDir = Vector2.zero;

            // プレイヤー方向のベクトルを2D平面（XZ面相当）に変換
            toPlayer = (this.player.transform.position - transform.position).to2D();
            toPlayerL = toPlayer.magnitude;
            toPlayerN = toPlayer.normalized;

            moveDir = BossMove(player,moveSpeed);

            if (moveDir.sqrMagnitude > 0.001f)
            {
                moveDir.Normalize();

                preMove = 0.5f * preMove + 0.5f * moveSpeed * Time.deltaTime * moveDir;

                // 位置の更新
                transform.position += preMove.to3D();
            }

            Quaternion targetRotation = Quaternion.LookRotation(toPlayer.to3D());

            // 現在の回転のY軸成分のみ取得
            float currentY = transform.rotation.eulerAngles.y;
            // ターゲットのY軸角度を取得
            float targetY = targetRotation.eulerAngles.y;

            // Y軸のみを補間
            float newY = Mathf.LerpAngle(currentY, targetY, turnSpeed * Time.deltaTime);

            // X,Zは0に固定してYのみ更新（必要に応じて他の軸の値を使う）
            transform.rotation = Quaternion.Euler(0, newY, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("ontriger");
        if (other.CompareTag("sword"))
        {
            Debug.Log("aaa");
            AudioSource.PlayClipAtPoint(killed, transform.position);
            death = true;
            GameManager.Instance.enemies.Remove(this);

            GameManager.Instance.NotifyKill(this);
            GameManager.Instance.SpornOrb(transform.position, 5);
        }
    }
}

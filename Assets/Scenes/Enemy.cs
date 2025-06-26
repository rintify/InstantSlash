using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class Enemy : Chara
{
    public float moveSpeed = 5.0f;   // 移動速度
    public float turnSpeed = 10.0f;  // 回転の補間速度

    public AudioClip killed;
    Player player;
    public float level = 0;

    Vector2 preMove;


    public void SetLevel(float level)
    {
        this.level = level;
        if (level < 2)
        {
            moveSpeed *= level * 0.3f + 0.4f;
            orbitSpeedBoost *= level * 0.3f + 0.4f;
        }
    }


    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        if (GameManager.Instance.enemies.Count > 0) nextChar_ToPlayer = GameManager.Instance.enemies[^1];
        no = GameManager.Instance.enemies.Count;
        GameManager.Instance.enemies.Add(this);
    }

    GameObject laserObject;

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

            // 各種距離・パラメータ（最初のコードの _pSword, _sword に相当）

            bool avoid = false;

            for (int i = 0; i < GameManager.Instance.enemies.Count; i++)
            {
                Chara e = GameManager.Instance.enemies[i];

                if (e == this) continue;

                if (toPlayerL < e.toPlayerL && priority >= e.priority) continue;

                //仲間を避ける
                Vector2 toEnemy = (e.transform.position - transform.position).to2D();
                Vector2 toEnemyN = toEnemy.normalized;
                float toEnemyL = toEnemy.magnitude;
                float priorityArea = priority >= e.priority ? 0 : e.sword.toEdgeL * 3;
                if (toEnemyL < sword.toEdgeL * 1.3 + e.sword.toEdgeL + priorityArea)
                {
                    moveDir += priority * priority * Mathf.Sign(-toEnemyN.Dot(toPlayerN.Right())) * toPlayerN.Right();
                    moveDir -= toPlayerN*0.5f;
                    avoid = true;

                    /*if (laserObject == null)
                    {
                        //laserObject = new GameObject("LaserLine");

                        // LineRenderer を追加
                        var line = laserObject.AddComponent<LineRenderer>();

                        // 線の幅（必要に応じて調整）
                        line.startWidth = 0.1f;
                        line.endWidth = 0.1f;

                        // 頂点数を2点に設定（始点・終点）
                        line.positionCount = 2;

                        // 頂点の位置を設定
                        line.SetPosition(0, this.transform.position);
                        line.SetPosition(1, e.transform.position);

                        // マテリアル設定（必要に応じて変更）
                        line.material = new Material(Shader.Find("Sprites/Default"));

                        // 色設定
                        line.startColor = Color.red;
                        line.endColor = Color.red;
                        Destroy(laserObject, 0.5f);
                    }*/
                }
            }

            if(!avoid && GameManager.Instance.Boss && toPlayerL < player.sword.toEdgeL + sword.toEdgeL*1.5f){
                moveDir = -toPlayerN;
                moveDir += -Mathf.Sign(player.sword.orbitSpeed) * toPlayer.normalized.Right();
                avoid = true;
            }

            if (!avoid
                && toPlayerL < player.sword.toEdgeL*2f + player.moveSpeed
                && nextChar_ToPlayer != null && nextChar_ToPlayer.sword.toEdge != null
                && toPlayerL < nextChar_ToPlayer.toPlayerL + nextChar_ToPlayer.sword.toEdgeL + sword.toEdgeL * 1.1f)
            {
                moveDir = -toPlayerN * 0.5f;
                moveDir += -Mathf.Sign(player.sword.orbitSpeed) * toPlayer.normalized.Right();
                avoid = true;
            }

            if (!avoid)
            {
                if (level >= 2)
                {
                    moveDir = BossMove(player,moveSpeed);
                }
                if (level >= 1)
                {
                    if (toPlayerL < sword.toEdgeL * 0.6f ||
                    toPlayerL < player.sword.toEdgeL * 1.4f &&
                    (player.sword.toEdgeN.Angle(-toPlayer, player.sword.orbitSpeed < 0) < 120
                    || player.sword.toEdgeN.Dot(-toPlayerN) > 0.7))
                    {
                        moveDir -= toPlayer.normalized;
                        moveDir += -Mathf.Sign(player.sword.orbitSpeed) * 0.3f * toPlayer.normalized.Right();
                    }
                    else if (toPlayerL > sword.toEdgeL * 0.8f)
                    {
                        moveDir += toPlayer.normalized;
                        moveDir += 0.15f * Mathf.Sign(player.sword.orbitSpeed) * toPlayer.normalized.Right();
                    }
                    else moveDir += Mathf.Sign(player.sword.orbitSpeed) * toPlayer.normalized.Right();

                }
                else
                {
                    if (toPlayerL < sword.toEdgeL * 0.4f)
                    {
                        moveDir -= toPlayerN;
                    }
                    else if (toPlayerL > sword.toEdgeL * 0.6f)
                    {
                        moveDir += toPlayerN;
                    }
                    else moveDir += Mathf.Sign(player.sword.orbitSpeed) * toPlayer.normalized.Right();
                }
            }


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
        if (other.CompareTag("sword"))
        {
            AudioSource.PlayClipAtPoint(killed, transform.position);
            death = true;
            GameManager.Instance.enemies.Remove(this);

            if (other.GetComponent<Orbit>().target.GetComponent<Player>() != null)
            {
                GameManager.Instance.NotifyKill(this);
                GameManager.Instance.SpornOrb(transform.position, 1);
            }
        }
    }
}

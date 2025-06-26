using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chara : MonoBehaviour
{
    [System.NonSerialized]
    public bool death = false;
    [System.NonSerialized]
    public Orbit sword;
    public float priority = 1;
    public float orbitSpeedBoost = 1;
    public float swordBoost = 1;

    [System.NonSerialized]
    public Vector2 toPlayer;
    [System.NonSerialized]
    public float toPlayerL;
    [System.NonSerialized]
    public Vector2 toPlayerN;

    [System.NonSerialized]
    public Chara nextChar_ToPlayer = null;
    [System.NonSerialized]
    public int no = 0;


    protected Vector2 BossMove(Player player,float moveSpeed)
    {
        Vector2 moveDir = new();

        if (toPlayerL > player.sword.toEdgeL + sword.toEdgeL)
        {
            moveDir += toPlayerN;
        }
        //巨大剣相手
        else if (player.sword.toEdgeL >= 2f * sword.toEdgeL)
        {
            Debug.Log("巨大剣相手");
            var gurdPos = toPlayerN;
            var gurdPos2 = -toPlayerN;
            if (Mathf.Sign(player.sword.orbitSpeed * sword.orbitSpeed) < 0) (gurdPos2, gurdPos) = (gurdPos, gurdPos2);
            gurdPos += -0.5f * Mathf.Sign(player.sword.orbitSpeed) * toPlayerN.Right();
            gurdPos2 += -2 * Mathf.Sign(player.sword.orbitSpeed) * toPlayerN.Right();

            float myTime = sword.toEdgeN.Angle(gurdPos, sword.orbitSpeed < 0) / Mathf.Abs(sword.orbitSpeed);
            float myTime2 = sword.toEdgeN.Angle(gurdPos2, sword.orbitSpeed < 0) / Mathf.Abs(sword.orbitSpeed);
            float pTime = player.sword.toEdge.Angle(-toPlayer, player.sword.orbitSpeed < 0) / Mathf.Abs(player.sword.orbitSpeed);

            float rotSpeed = moveSpeed * 180 / (Mathf.PI * toPlayerL);

            float pTime_ifMove = player.sword.toEdge.Angle(-toPlayer, player.sword.orbitSpeed < 0) / (Mathf.Abs(player.sword.orbitSpeed) - rotSpeed);

            float epTime = (toPlayerL - player.sword.toEdgeL + sword.toEdgeL * 0.3f) / moveSpeed;

            if (toPlayerL + sword.toEdgeL * 0.3f < player.sword.toEdgeL)
            {
                if (toPlayerL > sword.toEdgeL)
                {

                    if (myTime < pTime || myTime2 < pTime)
                    {
                        moveDir += toPlayerN;
                    }
                    else if (myTime < pTime_ifMove || myTime2 < pTime_ifMove)
                    {
                        moveDir += Mathf.Sign(player.sword.orbitSpeed) * toPlayerN.Right();
                    }
                    else
                    {
                        moveDir -= Mathf.Sign(player.sword.orbitSpeed) * toPlayerN.Right();
                    }
                }
                else
                {
                    if (toPlayerL > sword.toEdgeL * 0.4f)
                    {
                        moveDir += 0.5f * toPlayerN;
                    }
                    else
                    {
                        moveDir -= 0.1f * toPlayerN;
                    }
                    moveDir += toPlayerN.Right();
                }
            }
            else if (toPlayerL > player.sword.toEdgeL + sword.toEdgeL * 0.5f)
            {
                moveDir += toPlayerN;
                moveDir += Mathf.Sign(player.sword.orbitSpeed) * toPlayerN.Right();
            }
            else if (epTime < pTime && myTime < pTime)
            {
                moveDir += toPlayerN;
            }
            else
            {
                moveDir -= toPlayerN;
            }
        }
        // 戦闘モード
        else
        {
            // 自分の剣とプレイヤーの剣の角度計算（Angle() メソッドは与えられたベクトルとの角度差を返すものとする）
            float myTime = (10f + sword.toEdge.Angle(toPlayer, sword.orbitSpeed < 0)) / Mathf.Abs(sword.orbitSpeed);
            float pTime = player.sword.toEdge.Angle(-toPlayer, player.sword.orbitSpeed < 0) / Mathf.Abs(player.sword.orbitSpeed);

            // 攻撃モード
            if (myTime < pTime && player.sword.toEdgeN.Dot(-toPlayerN) < 0.8f)
            {
                Debug.Log("攻撃");
                if (toPlayerL > sword.toEdgeL * 0.8f)
                {
                    moveDir += toPlayerN;
                }
                else if (toPlayerL < sword.toEdgeL * 0.7f)
                {
                    moveDir -= toPlayerN;
                }
                // 横方向の補正（右方向のベクトルを加算）
                moveDir += 0.3f * Mathf.Sign(player.sword.orbitSpeed) * toPlayerN.Right();
            }
            // 回避モード
            else if (pTime < myTime && toPlayerL < player.sword.toEdgeL /*&& (player.sword.orbitSpeed * sword.orbitSpeed < 0)*/)
            {
                Debug.Log("回避");
                if (toPlayerL > player.sword.toEdgeL * 0.6f)
                    moveDir += toPlayerN * 0.4f;
                else
                    moveDir -= toPlayerN * 0.2f;

                moveDir += Mathf.Sign(player.sword.orbitSpeed) * toPlayerN.Right();
            }
            // 防御モード
            else
            {
                Debug.Log("防御");
                if (toPlayerL < player.sword.toEdgeL * 1.5f)
                {
                    moveDir -= toPlayerN;
                }
                else if (toPlayerL > player.sword.toEdgeL * 1.6f)
                {
                    moveDir += toPlayerN;
                }
                moveDir += -Mathf.Sign(player.sword.orbitSpeed) * 0.5f * toPlayerN.Right();
            }
        }

        return moveDir;
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Chara
{
    public TouchAreaJoystick joystick;
    public float moveSpeed = 5.0f;   // 移動速度
    public float turnSpeed = 10.0f;  // 回転の補間速度

    public AudioClip killed;

    private bool born = true;

    private Camera mainCamera;      // メインカメラ

    public bool invincible = false;

    void Start()
    {
        mainCamera = Camera.main;    // メインカメラを取得
        transform.localScale *= 0.1f;
    }

    void Update()
    {
        if(born){
            transform.localScale *= Mathf.Pow(20f,Time.deltaTime);
            if(transform.localScale.x > 1){
                transform.localScale = Vector3.one;
                born = false;
            }
            return;
        }
        if(death){
            var targetRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            return;
        }

        // 入力取得（W/Sで縦、A/Dで横）
        float horizontal = joystick.Horizontal() + Input.GetAxis("Horizontal");
        float vertical = joystick.Vertical() + Input.GetAxis("Vertical");

        // カメラの正面方向（ただしY成分は無視）
        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        // カメラの右方向（ただしY成分は無視）
        Vector3 camRight = mainCamera.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // 入力に応じた移動方向の算出
        Vector3 moveDir = (camForward * vertical) + (camRight * horizontal);

        if (moveDir.sqrMagnitude > 0.001f)
        {

            moveDir.Normalize();

            // 位置の更新
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            // 目標の回転を計算
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            // 現在の回転から目標の回転へスムーズに補間
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(born) return;
        if(other.CompareTag("sword")){ 
            AudioSource.PlayClipAtPoint(killed,transform.position);
            if(invincible) return;
            death = true;
            StartCoroutine(GameOver(3f));
            GameManager.Instance.GameOver();
        }
        else if(other.TryGetComponent<Life>(out var life))
        {
            GameManager.Instance.lvMeter.AddExp(life.score);
        }
    }

    IEnumerator GameOver(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        // ここに3秒後に実行したい処理を書く
        Debug.Log("3秒後に実行されました！");
        SceneManager.LoadScene("Start");
    }
}

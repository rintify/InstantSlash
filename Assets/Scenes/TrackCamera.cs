using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;          // 追跡対象（プレイヤー）
    public float smoothSpeed = 1f;  // 追従のスムーズさ

    private Vector3 offset;           // 初期のターゲットとの距離

    void Start()
    {
        if (target != null)
        {
            // カメラの初期位置とターゲットの位置の差をオフセットとして保存
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // ターゲットの現在位置にオフセットを加えた位置を目標位置とする
        Vector3 desiredPosition = target.position + offset;
        // 現在のカメラ位置から目標位置へスムーズに補間
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed*Time.deltaTime);
        transform.position = smoothedPosition;

        // 追跡対象の方向にカメラを向ける
        transform.LookAt(target);
    }
}

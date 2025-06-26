using UnityEngine;
using UnityEngine.EventSystems;

public class TouchAreaJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // ジョイスティックプレハブを Inspector で割り当て
    public GameObject joystickPrefab;

    // 現在アクティブなジョイスティックの制御スクリプト
    private JoystickController activeJoystick;

    // タッチ開始時の処理
    public void OnPointerDown(PointerEventData eventData)
    {
        // 既にアクティブなジョイスティックが存在する場合は破棄
        if (activeJoystick != null)
        {
            Destroy(activeJoystick.gameObject);
            activeJoystick = null;
        }

        // プレハブからジョイスティックを生成（親はタッチエリアの Transform）
        GameObject joystickObj = Instantiate(joystickPrefab, transform);
        activeJoystick = joystickObj.GetComponent<JoystickController>();
        if (activeJoystick != null)
        {
            // タッチ開始位置を渡して初期化
            activeJoystick.Initialize(eventData.position);
        }
    }

    // ドラッグ中の処理は現在のジョイスティックに転送
    public void OnDrag(PointerEventData eventData)
    {
        if (activeJoystick != null)
        {
            activeJoystick.OnDrag(eventData.position);
        }
    }

    // タッチ終了時の処理
    public void OnPointerUp(PointerEventData eventData)
    {
        if (activeJoystick != null)
        {
            activeJoystick.OnPointerUp();
            // activeJoystick はジョイスティック側で終了アニメーション後に破棄されるため、ここでは参照を解除
            activeJoystick = null;
        }
    }

    /// <summary>
    /// 外部から現在の水平入力（-1～1）を取得。
    /// もしジョイスティックが存在しなければ 0 を返す。
    /// </summary>
    public float Horizontal()
    {
        return activeJoystick != null ? activeJoystick.Horizontal() : 0f;
    }

    /// <summary>
    /// 外部から現在の垂直入力（-1～1）を取得。
    /// もしジョイスティックが存在しなければ 0 を返す。
    /// </summary>
    public float Vertical()
    {
        return activeJoystick != null ? activeJoystick.Vertical() : 0f;
    }
}

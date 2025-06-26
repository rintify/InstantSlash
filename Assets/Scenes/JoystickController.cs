using UnityEngine;
using System.Collections;

public class JoystickController : MonoBehaviour
{
    // Inspector で割り当てる各オブジェクトの RectTransform
    public RectTransform joystickBG;
    public RectTransform joystickCenter;         // 常に中央（固定）
    public RectTransform joystickIntermediate;   // 中間位置
    public RectTransform joystickTip;            // 先端（入力方向）

    // タッチ開始位置（スクリーン座標）
    private Vector2 pointerDownPos;
    // 入力値（-1～1）の方向ベクトル
    private Vector2 inputVector;
    // 中央復帰アニメーションの時間（秒）
    public float returnDuration = 0.2f;

    private CanvasGroup canvasGroup;

    /// <summary>
    /// タッチ開始位置を設定して初期化する。
    /// </summary>
    public void Initialize(Vector2 startPos)
    {
        pointerDownPos = startPos;
        if (joystickBG != null)
        {
            // 背景をタッチ開始位置に移動
            joystickBG.position = startPos;
        }
        // 子オブジェクトは初期化（中心は原点）
        if (joystickCenter != null)
            joystickCenter.anchoredPosition = Vector2.zero;
        if (joystickIntermediate != null)
            joystickIntermediate.anchoredPosition = Vector2.zero;
        if (joystickTip != null)
            joystickTip.anchoredPosition = Vector2.zero;


        canvasGroup = gameObject.AddComponent<CanvasGroup>();

    }

    /// <summary>
    /// ドラッグ中の処理。タッチ開始位置との差から入力方向を計算し、先端と中間の位置を更新する。
    /// </summary>
    public void OnDrag(Vector2 currentPos)
    {
        if (joystickBG == null)
            return;

        Vector2 direction = currentPos - pointerDownPos;
        float bgRadius = joystickBG.sizeDelta.x * 0.5f; // 背景の半径として計算
        inputVector = direction / bgRadius;
        if (inputVector.magnitude > 1f)
            inputVector = inputVector.normalized;

        // 先端の位置更新
        if (joystickTip != null)
            joystickTip.anchoredPosition = inputVector * bgRadius;
        // 中間は先端の半分
        if (joystickIntermediate != null)
            joystickIntermediate.anchoredPosition = inputVector * bgRadius * 0.5f;
        // 中心は常に原点
        if (joystickCenter != null)
            joystickCenter.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// タッチ終了時の処理。中央復帰アニメーションを開始し、完了後に自身を破棄する。
    /// </summary>
    public void OnPointerUp()
    {
        inputVector = Vector2.zero;
        StartCoroutine(AnimateReturnAndDestroy());
    }

    private IEnumerator AnimateReturnAndDestroy()
    {
        float elapsed = 0f;
        Vector2 tipStartPos = joystickTip != null ? joystickTip.anchoredPosition : Vector2.zero;
        Vector2 intermediateStartPos = joystickIntermediate != null ? joystickIntermediate.anchoredPosition : Vector2.zero;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;

            // 位置補間
            if (joystickTip != null)
                joystickTip.anchoredPosition = Vector2.Lerp(tipStartPos, Vector2.zero, t);
            if (joystickIntermediate != null)
                joystickIntermediate.anchoredPosition = Vector2.Lerp(intermediateStartPos, Vector2.zero, t);

            // ★ 透明度を徐々に 0 に
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (joystickTip != null)
            joystickTip.anchoredPosition = Vector2.zero;
        if (joystickIntermediate != null)
            joystickIntermediate.anchoredPosition = Vector2.zero;

        Destroy(gameObject);
    }

    /// <summary>
    /// 外部から水平入力（-1～1）を取得
    /// </summary>
    public float Horizontal()
    {
        return inputVector.x;
    }

    /// <summary>
    /// 外部から垂直入力（-1～1）を取得
    /// </summary>
    public float Vertical()
    {
        return inputVector.y;
    }
}

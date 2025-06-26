using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextMove : MonoBehaviour
{
    // テキストのRectTransformをインスペクターからセット
    public RectTransform textRect;
    
    // 移動開始位置と終了位置（Anchored Position で指定）
    public Vector2 startPos = new Vector2(-800, 0);
    public Vector2 endPos = new Vector2(800, 0);
    
    // アニメーションカーブ：X軸＝正規化した経過時間（0～1）、Y軸＝補間値（0～1）
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // アニメーションの総時間
    public float duration = 3.0f;

    private void Start()
    {
        // 初期位置にセット
        textRect.anchoredPosition = startPos;
        
        // アニメーション開始
        StartCoroutine(MoveText());
    }

    private IEnumerator MoveText()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 経過時間の正規化（0～1）
            float t = elapsed / duration;
            // アニメーションカーブで加減速を制御した補間値を取得
            float curveValue = moveCurve.Evaluate(t);
            // 始点から終点へ線形補間（カーブで制御された値で補間）
            textRect.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 終了時に正確な終了位置へ設定
        textRect.anchoredPosition = endPos;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelBlinkCustomSequence : MonoBehaviour
{
    // Imageコンポーネントを割り当て
    public Image panelImage;
    // 各フェードにかかる時間（秒）
    public float blinkDuration = 1.0f;
    // 中間に使用するα値（例えば0.5なら半透明）
    public float middleAlpha = 0.5f;
    // 中間のパターン（デフォルト→中間→デフォルト）を何回繰り返すか
    public int blinkCount = 1; 

    // Imageに設定されたデフォルトのα値（Startで取得）
    private float defaultAlpha;

    void Start()
    {
        // Imageの元々のα値を保存
        defaultAlpha = panelImage.color.a;

        // 開始時は完全に透明に設定（α = 0）
        Color startColor = panelImage.color;
        startColor.a = 0f;
        panelImage.color = startColor;

        StartCoroutine(BlinkSequence());
    }

    IEnumerator BlinkSequence()
    {
        // 1. 0からデフォルトのαにフェード
        yield return StartCoroutine(FadeTo(defaultAlpha, blinkDuration));

        // 2. ブリンクパターンを指定回数実施
        //    (デフォルト→中間→デフォルト) を1回のパターンとして
        for (int i = 0; i < blinkCount; i++)
        {
            // デフォルト→中間
            yield return StartCoroutine(FadeTo(middleAlpha, blinkDuration));
            // 中間→デフォルト
            yield return StartCoroutine(FadeTo(defaultAlpha, blinkDuration));
        }

        // 3. 最終的にデフォルト→0（完全透明）にフェード
        yield return StartCoroutine(FadeTo(0f, blinkDuration));
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = panelImage.color.a;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            Color newColor = panelImage.color;
            newColor.a = newAlpha;
            panelImage.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最終値に固定
        Color finalColor = panelImage.color;
        finalColor.a = targetAlpha;
        panelImage.color = finalColor;
    }
}

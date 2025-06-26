using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemListController : MonoBehaviour
{
    [Header("アイテム画像設定")]
    public Sprite powerUpSprite;     // パワーアップアイテムの画像
    public Sprite speedUpSprite;     // スピードアップアイテムの画像
    public GameObject itemPrefab;    // Image+Button を含むアイテム枠プレハブ

    [Header("レイアウト設定")]
    public float itemSize = 100f;        // アイコンは正方形（Width=Height）
    public float itemSpacing = 20f;      // ペア間の縦スペース
    public float orWidth = 50f;          // “or” テキストの幅
    public float slideDuration = 0.3f;   // スライドイン／スライドダウン時間
    public float fadeDuration = 0.3f;    // フェードアウト時間

    [Header("“or” テキスト設定")]
    public string orString = "or";
    public Font orFont;                  // Inspectorで Arial.ttf 等をセット
    public int orFontSize = 24;
    public Color orColor = Color.black;

    private RectTransform rectTransform;

    // ペアごとにまとめて扱うデータ
    private class ItemPairData
    {
        public GameObject container;   // このペア全体を動かす親コンテナ
        public GameObject goA, goB;    // 左右のアイコン
        public GameObject orGo;        // “or” テキスト
        public ItemType typeA, typeB;
    }
    private List<ItemPairData> pairs = new List<ItemPairData>();

    public enum ItemType
    {
        PowerUp,
        SpeedUp
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // フォント未設定ならデフォルトをロード
        if (orFont == null)
            orFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    /// <summary>
    /// 同時に２つのアイテムを追加するときはこのメソッドを呼ぶ
    /// </summary>
    public void AddItem(ItemType typeA, ItemType typeB)
    {
        // １）既存のペアを下にスライド
        for (int i = 0; i < pairs.Count; i++)
        {
            RectTransform rt = pairs[i].container.GetComponent<RectTransform>();
            float fromY = -i * (itemSize + itemSpacing);
            float toY = -(i + 1) * (itemSize + itemSpacing);
            StartCoroutine(SlideVertical(rt, fromY, toY));
        }

        // ２）新しいペア用コンテナを作成
        GameObject container = new GameObject("ItemPair", typeof(RectTransform));
        container.transform.SetParent(rectTransform, false);
        RectTransform cret = container.GetComponent<RectTransform>();
        cret.anchorMin = cret.anchorMax = new Vector2(0, 1);
        cret.pivot = new Vector2(0, 1);
        float groupWidth = itemSize * 2 + orWidth;
        cret.sizeDelta = new Vector2(groupWidth, itemSize);
        // 右外からスライドインする初期位置
        cret.anchoredPosition = new Vector2(rectTransform.rect.width, 0);

        // ３）左アイテム生成
        GameObject goA = Instantiate(itemPrefab, container.transform);
        SetupItem(goA, typeA, new Vector2(0, 0));

        // ４）“or” テキスト生成
        GameObject orGo = new GameObject("OrText", typeof(RectTransform));
        orGo.transform.SetParent(container.transform, false);
        RectTransform orRt = orGo.GetComponent<RectTransform>();
        orRt.anchorMin = orRt.anchorMax = new Vector2(0, 1);
        orRt.pivot = new Vector2(0, 1);
        orRt.anchoredPosition = new Vector2(itemSize, 0);
        orRt.sizeDelta = new Vector2(orWidth, itemSize);
        Text txt = orGo.AddComponent<Text>();
        txt.text = orString;
        txt.font = orFont;
        txt.fontSize = orFontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = orColor;
        // フェード用
        CanvasGroup cgOr = orGo.AddComponent<CanvasGroup>();

        // ５）右アイテム生成
        GameObject goB = Instantiate(itemPrefab, container.transform);
        SetupItem(goB, typeB, new Vector2(itemSize + orWidth, 0));

        // ６）ペア情報を保存し、リスト先頭に追加
        var pairData = new ItemPairData
        {
            container = container,
            goA = goA,
            goB = goB,
            orGo = orGo,
            typeA = typeA,
            typeB = typeB
        };
        pairs.Insert(0, pairData);

        // ７）スライドイン
        StartCoroutine(SlideHorizontal(cret, rectTransform.rect.width, 0f, 0f));

        // ８）クリックリスナー設定（先頭ペアは index=0）
        UpdatePairListeners();
    }

    // 個々のアイテム（プレハブ）に画像・ボタン・CanvasGroupをセットアップ
    // -----------------------------
    // 変更部分：SetupItem メソッド
    // -----------------------------
    private void SetupItem(GameObject go, ItemType type, Vector2 anchoredPos)
    {
        // RectTransform の設定（省略）

        // --- CanvasGroup の追加（フェード用） ---
        if (go.GetComponent<CanvasGroup>() == null)
            go.AddComponent<CanvasGroup>();

        // --- アイコン用 Image の取得 & 差し替え ---
        Image iconImg = null;

        // 1) 子オブジェクトに "Icon" という名前があればそちらを優先
        Transform iconTf = go.transform.Find("Icon");
        if (iconTf != null)
        {
            iconImg = iconTf.GetComponent<Image>();
        }
        // 2) なければ、子を含めた最初の Image を取得
        if (iconImg == null)
        {
            iconImg = go.GetComponentInChildren<Image>();
        }
        // 3) それでも null ならエラー
        if (iconImg == null)
        {
            Debug.LogError("Prefab に Image コンポーネントが見つかりません: " + go.name);
            return;
        }

        // スプライト差し替え
        iconImg.sprite = (type == ItemType.PowerUp)
            ? powerUpSprite
            : speedUpSprite;

        // --- ボタンリスナーの設定 ---
        var btn = go.GetComponentInChildren<Button>();
        // (リスナーは AddItem 側でまとめてやり直すのでここでは不要でも OK)

        // RectTransform の位置・サイズ設定
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(itemSize, itemSize);
    }


    // 各ペアの “左 or 右” ボタンに正しい index を再設定
    private void UpdatePairListeners()
    {
        for (int i = 0; i < pairs.Count; i++)
        {
            int idx = i;
            // 左
            var btnA = pairs[i].goA.GetComponentInChildren<Button>();
            btnA.onClick.RemoveAllListeners();
            btnA.onClick.AddListener(() => OnPairClicked(idx, true));
            // 右
            var btnB = pairs[i].goB.GetComponentInChildren<Button>();
            btnB.onClick.RemoveAllListeners();
            btnB.onClick.AddListener(() => OnPairClicked(idx, false));
        }
    }

    // ペアのどちらかがクリックされた
    private void OnPairClicked(int pairIndex, bool isLeft)
    {
        if (pairIndex < 0 || pairIndex >= pairs.Count) return;
        StartCoroutine(HandlePairSelection(pairIndex, isLeft));
    }

    // 選択時：効果発動→フェードアウト→削除→残り下詰め
    private IEnumerator HandlePairSelection(int index, bool isLeft)
    {
        var pair = pairs[index];
        // どちらを選んだか＆効果発動
        ItemType chosen = isLeft ? pair.typeA : pair.typeB;
        ApplyEffect(chosen);

        // フェードアウト開始
        var cgA = pair.goA.GetComponent<CanvasGroup>();
        var cgB = pair.goB.GetComponent<CanvasGroup>();
        var cgOr = pair.orGo.GetComponent<CanvasGroup>();

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            cgA.alpha = cgB.alpha = cgOr.alpha = a;
            yield return null;
        }

        // 完全に削除
        Destroy(pair.container);
        pairs.RemoveAt(index);

        // 残りペアを上詰め（下方向へのスライドを打ち消す＝上に戻す）
        for (int i = index; i < pairs.Count; i++)
        {
            var rt = pairs[i].container.GetComponent<RectTransform>();
            float fromY = rt.anchoredPosition.y;
            float toY = -i * (itemSize + itemSpacing);
            StartCoroutine(SlideVertical(rt, fromY, toY));
        }

        // リスナーを再設定
        UpdatePairListeners();
    }

    // 横スライド（入場用）
    private IEnumerator SlideHorizontal(RectTransform rt, float fromX, float toX, float y)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float x = Mathf.Lerp(fromX, toX, t);
            rt.anchoredPosition = new Vector2(x, y);
            yield return null;
        }
        rt.anchoredPosition = new Vector2(toX, y);
    }

    // 縦スライド（下詰め／上詰め用）
    private IEnumerator SlideVertical(RectTransform rt, float fromY, float toY)
    {
        float elapsed = 0f;
        float x = rt.anchoredPosition.x;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float y = Mathf.Lerp(fromY, toY, t);
            rt.anchoredPosition = new Vector2(x, y);
            yield return null;
        }
        rt.anchoredPosition = new Vector2(x, toY);
    }

    public float swordDSize, orbitDSpeed;

    // アイテム効果（例）
    private void ApplyEffect(ItemType type)
    {
        if(!GameManager.Instance.player.sword) return;
        switch (type)
        {
            case ItemType.PowerUp:
                GameManager.Instance.player.sword.Big(swordDSize);
                // ゲームロジック追加…
                break;
            case ItemType.SpeedUp:
                GameManager.Instance.player.sword.orbitSpeed += Mathf.Sign(GameManager.Instance.player.sword.orbitSpeed)*orbitDSpeed;
                // ゲームロジック追加…
                break;
        }
    }
}

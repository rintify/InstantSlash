using UnityEngine;
using UnityEngine.UI;

public class EnemyIndicator : MonoBehaviour
{
    [Tooltip("UI上のアイコン (RectTransform)")]
    public RectTransform icon;
    
    [Tooltip("アイコンの子オブジェクトにある三角形 (RectTransform)")]
    public RectTransform triangle;

    [Header("設定")]
    [Tooltip("使用するカメラ（指定がなければメインカメラを使用）")]
    public Camera cam;
    
    [Tooltip("スクリーン端からの余白（ピクセル単位）")]
    public float margin = 50f;

    [Tooltip("三角形のアイコンからのオフセット（ピクセル単位）")]
    private float orbitRadius;

    // アイコンが属しているキャンバスのRectTransform
    private RectTransform canvasRect;

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        // アイコンの親オブジェクトからCanvasを取得する
        Canvas canvas = icon.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        orbitRadius = triangle.localPosition.magnitude;
    }

    void Update()
    {
        // 敵(Boss)の参照を取得。敵が存在しなければアイコンを非表示にする
        Chara enemyObject = GameManager.Instance.Boss;
        if (enemyObject == null)
        {
            if (icon.gameObject.activeSelf)
                icon.gameObject.SetActive(false);
            return;
        }
        Transform enemy = enemyObject.transform;

        // ワールド座標からViewport座標(0～1)に変換
        Vector3 viewportPos = cam.WorldToViewportPoint(enemy.position);

        // カメラ背後にいるか判定（zが負の場合は背後）
        bool isBehind = viewportPos.z < 0;

        // Viewport座標を元に、敵が画面内にいるかどうかを判定
        bool onScreen = (!isBehind && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1);

        if (onScreen)
        {
            // 画面内にいる場合はアイコン(および三角形)を非表示
            if (icon.gameObject.activeSelf)
                icon.gameObject.SetActive(false);
        }
        else
        {
            // 画面外の場合はアイコンを表示
            if (!icon.gameObject.activeSelf)
                icon.gameObject.SetActive(true);

            // 敵が背後にいる場合は方向を反転して表示
            if (isBehind)
            {
                viewportPos.x = 1f - viewportPos.x;
                viewportPos.y = 1f - viewportPos.y;
            }
            
            // Viewport座標をScreen座標に変換
            Vector3 screenPos = new Vector3(viewportPos.x * Screen.width, viewportPos.y * Screen.height, 0f);

            // Screen Space Overlayの場合、キャンバスの中央が原点になるため変換
            Vector2 canvasPos = new Vector2(screenPos.x - Screen.width * 0.5f,
                                            screenPos.y - Screen.height * 0.5f);

            // キャンバスが利用可能なら、余白を考慮してクランプ
            if (canvasRect != null)
            {
                float halfWidth = canvasRect.rect.width * 0.5f;
                float halfHeight = canvasRect.rect.height * 0.5f;
                canvasPos.x = Mathf.Clamp(canvasPos.x, -halfWidth + margin, halfWidth - margin);
                canvasPos.y = Mathf.Clamp(canvasPos.y, -halfHeight + margin, halfHeight - margin);
            }
            // アイコンの位置更新
            icon.anchoredPosition = canvasPos;

            // ============================
            // 三角形の回転と位置調整処理
            // ============================
            // 三角形はアイコンの子オブジェクトとして扱うので、ローカル座標空間を利用
            // ここでは、アイコン中心から敵の方向を示すベクトルを使って配置する

            // 画面中心（キャンバスの原点）からアイコンの位置ベクトル
            Vector2 direction = canvasPos.normalized;
            if (direction == Vector2.zero)
            {
                direction = Vector2.up;
            }
            // アイコン自体の大きさに加え、任意のオフセット分だけ外側に配置する
            triangle.localPosition = direction * orbitRadius;
            // 三角形が敵方向を向くように回転させる
            // ※デフォルトで三角形が上向きの場合、90度調整する
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            triangle.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}

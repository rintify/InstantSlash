using UnityEngine;
using UnityEngine.UI;

public class LvMeter : MonoBehaviour 
{
    public RectTransform meter;      // ゲージのRectTransform
    public Text lv;                  // レベル表示用Text
    public float level = 1;          // 現在のレベル（初期値は1）
    public float score = 0;
    public Text scoreText;  
    
    // 現在のレベルでの進捗状態と、獲得済みだが未消費の経験値
    public float currentExp = 0f;    // 現在のレベル内で消費済みの経験値
    public float expBuffer = 0f;     // 獲得済みで、まだ消費されていない経験値

    public float consumptionRate = 50f; // 1秒あたりに消費する経験値の量

    float expToNextLevel;            // 次のレベルに必要な経験値
    float firstWidth;                // ゲージバーの初期幅


    void Start()
    {
        firstWidth = meter.sizeDelta.x;
        expToNextLevel = ExpToNextLevel();
        UpdateGauge(currentExp / expToNextLevel);
        lv.text = level.ToString();
    }

    void Update()
    {
        // expBufferに経験値があれば、一定速度で消費してcurrentExpに反映する
        if(expBuffer > 0f)
        {
            // このフレームで消費できる経験値量
            float expConsumed = consumptionRate * Time.deltaTime;
            if(expConsumed > expBuffer)
            {
                expConsumed = expBuffer;
            }

            // expBufferから減算し、currentExpに加算
            expBuffer -= expConsumed;
            currentExp += expConsumed;

            // 現在のレベルアップに必要な経験値を超えた場合はレベルアップ処理（余剰分はそのまま繰り越す）
            while(currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                level++;
                lv.text = level.ToString();

                // 新レベルでの必要経験値を計算
                expToNextLevel = ExpToNextLevel();

                this.Delay(()=>{
                    GameManager.Instance.itemListController.AddItem(ItemListController.ItemType.PowerUp,ItemListController.ItemType.SpeedUp);
                },1);
                
            }

            // ゲージ表示を更新（currentExpが実体の経験値として反映）
            UpdateGauge(currentExp / expToNextLevel);
        }
    }

    float ExpToNextLevel(){
        return -241 + 300 * Mathf.Pow(1.1f,level - 1);
    }

    // 外部から経験値を獲得したときに呼び出す
    // アニメーション中でも expBuffer に加算され、既存の増加量に追加される
    public void AddExp(float dExp)
    {
        expBuffer += dExp;
        score += dExp;
        scoreText.text = score.ToString();
    }

    // ゲージのfill率（0～1）に応じて横幅を更新
    void UpdateGauge(float fill)
    {
        var sizeDelta = meter.sizeDelta;
        sizeDelta.x = firstWidth * fill;
        meter.sizeDelta = sizeDelta;
    }
}

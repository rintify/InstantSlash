using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ItemListController;

public class GameManager : MonoBehaviour
{
    public Canvas canvas;
    static bool virgin = true;
    // シングルトンのインスタンス
    public static GameManager Instance { get; private set; }
    public ItemListController itemListController;

    // ゲームの状態を表す変数（例：スコア、ライフなど）
    private int bossGage = 0;
    public int bossGageMax = 15;

    public Text  result;
    public GameObject tutorial, tutorial2, tutorial3;

    public Enemy enemyPrefab;
    public Boss bossPrefab;
    public Orbit swordPrefab;
    [NonSerialized]
    public Player player;

    public LvMeter lvMeter;

    EX.Intervalist spornEnemy;
    public int spornMax;
    public float r = 13;
    public float interval = 2f;

    public List<Chara> enemies = new();
    private Chara boss = null;
    public Chara Boss {get {
        return boss;
    }}

    DateTime startTime;
    public float swordDSize = 0.1f;

    float spornAngle;

    public Life lifePrephab;

    public GameObject bossPanel;

    void Awake()
    {
        // すでにインスタンスが存在する場合は削除
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // インスタンスを設定して、シーン間で持続させる
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        result.gameObject.SetActive(false);
        startTime = DateTime.UtcNow;

        spornAngle = UnityEngine.Random.Range(0f, 360f);
    }

    public void GameOver()
    {
        TimeSpan elapsed = DateTime.UtcNow - startTime;
        string formatted = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";

        result.text = "- 戦績 -\n" + "スコア: " + lvMeter.score + "\n" + "生存時間: " + formatted;
        result.gameObject.SetActive(true);
    }

    void Start()
    {
        spornEnemy = new(() =>
        {
            enemies.Sort((a, b) => a.toPlayerL > b.toPlayerL ? 1 : -1);

            for (int i = 0; i < enemies.Count; i++)
            {
                if (i == 0) enemies[i].nextChar_ToPlayer = null;
                else enemies[i].nextChar_ToPlayer = enemies[i - 1];
                enemies[i].no = i;
            }

            if (enemies.Count > spornMax) return;
            SpornEnemy();

        }, interval, virgin ? -interval : interval);
        player = GameObject.Find("Player").GetComponent<Player>();

        if (virgin)
        {
            this.Delay(() =>
            {
                tutorial.SetActive(true);
                this.Delay(() =>
                {
                    tutorial.SetActive(false);
                    this.Delay(() =>
                    {
                        tutorial2.SetActive(true);
                        this.Delay(() =>
                        {
                            tutorial2.SetActive(false);
                        }, 3f);
                    }, 1f);
                }, 3f);
            }, 1.5f);
        }
    }

    public void SpornOrb(Vector3 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var a = Instantiate(Instance.lifePrephab);
            a.transform.position = pos + new Vector3(UnityEngine.Random.Range(-0.1f * count, 0.1f * count), 0, UnityEngine.Random.Range(-0.1f * count, 0.1f * count));
        }
    }

    void SpornEnemy()
    {
        var enemy = Instantiate(enemyPrefab); // Prefabをインスタンス化
        enemy.SetLevel(
            lvMeter.level >= 4 ? UnityEngine.Random.Range(2f, 3f) :
            lvMeter.level >= 3 ? UnityEngine.Random.Range(1.5f, 2f) :
            lvMeter.level >= 2 ? UnityEngine.Random.Range(1f, 1.5f) :
            UnityEngine.Random.Range(0f, 0.6f)
        );
        Sporn(enemy, r);
    }

    void Sporn(Chara enemy,float spornRadius)
    {
        if (player.sword == null) return;
        enemy.transform.position = player.transform.position + (
            spornRadius * UnityEngine.Random.Range(1f, 1.3f) 
            * (spornAngle += UnityEngine.Random.Range(90f, 270f)).Deg2Direction()
        ).to3D();
        var sword = Instantiate(swordPrefab); // Prefabをインスタンス化
        sword.transform.position = enemy.transform.position + sword.orbitRadius * Vector3.left;
        sword.transform.position = sword.transform.position.SetY(player.sword.transform.position.y);
        sword.target = enemy;
    }

    void Update()
    {
        spornEnemy.Update();
    }

    // スコアを更新するメソッド
    public void NotifyKill(Chara chara)
    {
        if(chara == boss){
            boss = null;
        }

        bossGage++;

        if (boss == null && bossGage > bossGageMax)
        {
            this.Delay(()=>{
                if (boss != null) return;
                var enemy = Instantiate(bossPrefab); // Prefabをインスタンス化
                Sporn(enemy,r*3f);
                bossGage = 0;
                boss = enemy;
                Destroy(Instantiate(bossPanel,canvas.transform),5f);
            },1f);
        }

        /*if (virgin)
        {
            this.Delay(() =>
            {
                tutorial3.SetActive(true);
                this.Delay(() =>
                {
                    tutorial3.SetActive(false);
                }, 3);
            }, 1);
            virgin = false;
        }*/

    }
}

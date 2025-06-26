using System;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    [Tooltip("モードフラグ（false: 定常位置へ移動・回転補間, true: 公転）")]
    public bool isOrbiting = false;

    [Tooltip("公転するターゲットのTransform")]
    public Chara target;
    
    [Tooltip("公転速度（度/秒）")]
    public float orbitSpeed = 30f;
    
    [Tooltip("公転半径")]
    public float orbitRadius = 5f;
    
    [Tooltip("移動速度（定常位置へ移動するとき）")]
    public float moveSpeed = 5f;
    
    // 現在の角度（度）
    private float currentAngle;
    
    // 初期のy座標を固定
    private float fixedY;

    public ParticleSystem particle;
    public AudioClip clip;

    Transform edge;
    public Vector2 toEdge;
    public float toEdgeL;
    public Vector2 toEdgeN;

    public void Big(float swordDSize){
        this.transform.localScale += swordDSize*Vector3.one;
        Camera.main.fieldOfView *= 1.1f;
    }

    void Start()
    {
        edge = transform.Find("edge");
        fixedY = transform.position.y;
        target.sword = this;
        orbitSpeed *= target.orbitSpeedBoost;
        this.transform.localScale *= target.swordBoost;
    }

    void Update()
    {
        toEdge = (edge.position - target.transform.position).to2D();
        toEdgeL = toEdge.magnitude;
        toEdgeN = toEdge.normalized;

        if(target.death){
            transform.localScale *= Mathf.Pow(0.2f,Time.deltaTime);
            Destroy(gameObject,1);
            tag = "dead";
            return;
        }

        if (!isOrbiting)
        {
            // --- モード1: 定常位置（ターゲット中心の円上でキャラクターに最も近い位置）へ移動・回転補間 ---
            // キャラクターの水平位置からターゲット方向へのベクトル（y成分は無視）
            Vector3 toCharacter = transform.position - target.transform.position;
            toCharacter.y = 0;
            
            Vector3 stationaryPosition;
            if (toCharacter == Vector3.zero)
            {
                // 万一キャラクターがターゲットと重なった場合は適当な方向を指定
                stationaryPosition = target.transform.position + new Vector3(orbitRadius, 0, 0);
            }
            else
            {
                // ターゲットからキャラクター方向に沿って、半径orbitRadiusの円周上の最も近い位置
                stationaryPosition = target.transform.position + toCharacter.normalized * orbitRadius;
            }
            stationaryPosition.y = fixedY;
            
            // 定常回転：ターゲットからstationaryPositionへの方向（xz平面のみ）
            Quaternion stationaryRotation = Quaternion.LookRotation(stationaryPosition - target.transform.position);
            
            // 位置と回転を補間して移動
            transform.position = Vector3.MoveTowards(transform.position, stationaryPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, stationaryRotation, 2f * Mathf.Abs(orbitSpeed) * Time.deltaTime);
            
            // 位置と回転がほぼ一致したらモード切替え
            if (Vector3.Distance(transform.position, stationaryPosition) < 0.1f &&
                Quaternion.Angle(transform.rotation, stationaryRotation) < 1f)
            {
                isOrbiting = true;
                // 公転モード開始時、stationaryPositionから角度を再計算
                Vector3 dir = (stationaryPosition - target.transform.position).normalized;
                currentAngle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            }
        }
        else
        {
            // --- モード2: 公転 ---
            // 角度を更新し、円周上の位置を直接指定
            currentAngle += orbitSpeed * Time.deltaTime;
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 orbitPos = target.transform.position + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * orbitRadius;
            orbitPos.y = fixedY;
            transform.position = orbitPos;

            // ターゲットの背面を向く回転を直接設定（xz平面のみ）
            Vector3 lookDirection = target.transform.position - orbitPos;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion orbitRotation = Quaternion.LookRotation(-lookDirection);
                transform.rotation = orbitRotation;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("sword"))
        {
            orbitSpeed *= -1;

            if(particle != null && clip != null){
                Vector3 pos = (transform.position + other.transform.position)*0.5f;
                ParticleSystem newParticle = Instantiate(particle);
                newParticle.transform.position = pos;
                newParticle.Play();
                Destroy(newParticle.gameObject, 2.0f);

                AudioSource.PlayClipAtPoint(clip,pos);
            }
        }

    }
}

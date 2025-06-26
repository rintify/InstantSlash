using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TextCore;

public class Life : MonoBehaviour
{
    public float speed = 3;
    bool mov = false;
    public int score = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(this.gameObject,5);
        this.Delay(()=>{mov = true;},0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        var s = GameManager.Instance.player;
        if(!mov || s == null) return;
        transform.position += speed * Time.deltaTime * (s.transform.position - transform.position).normalized;
        if((transform.position - s.transform.position).sqrMagnitude < 0.1){
            this.gameObject.SetActive(false);
        }
    }
}

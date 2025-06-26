using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    public AudioClip tap;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool a;

    public void StartGame(){
        if(a) return;
        a = true;
        this.Delay(()=>{SceneManager.LoadScene("GameScene");},0.6f);
        AudioSource.PlayClipAtPoint(tap,Vector3.zero);
    }
}

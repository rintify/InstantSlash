using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class EX
{
    public static float Cross(this Vector2 self,Vector2 v){
        return self.x*v.y - self.y*v.x;
    }

    public static float Dot(this Vector2 self,Vector2 v){
        return self.x*v.x + self.y*v.y;
    }

    public static Vector2 to2D(this Vector3 self){
        return new Vector2(self.x,self.z);
    }

    public static Vector3 to3D(this Vector2 self){
        return new Vector3(self.x,0,self.y);
    }

    public static Vector2 Right(this Vector2 v){
        return new(v.y,-v.x);
    }

    public static Vector3 SetY(this Vector3 self, float y){
        return new Vector3(self.x, y, self.z);
    }

     public static float Angle(this Vector2 a, Vector2 b, bool reverse){
        float angle = Vector3.Angle(a, b);

        // 外積を計算して符号を取得
        if (reverse ? a.Cross(b) > 0 : a.Cross(b) < 0)
        {
            // 反時計回りの角度に変換
            angle = 360f - angle;
        }
        return angle;
    }

    public class Intervalist{
        float t = 0;
        public float interval;
        Action action;
        public Intervalist(Action action,float interval){
            this.interval = interval;
            this.action = action;
        }
        public Intervalist(Action action,float interval, float t0){
            this.interval = interval;
            this.action = action;
            t = t0;
        }
        public void Update(){
            t += Time.deltaTime;
            if(t > interval){
                action.Invoke();
                t = 0;
            }
        }
    }

    public static Vector2 Deg2Direction(this float degree){
        float rad = degree*Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad),Mathf.Sin(rad));
    }

    public static Coroutine Delay(this MonoBehaviour monoBehaviour, System.Action action, float delay)
    {
        return monoBehaviour.StartCoroutine(CoroutineAction(action, delay));
    }

    private static IEnumerator CoroutineAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}
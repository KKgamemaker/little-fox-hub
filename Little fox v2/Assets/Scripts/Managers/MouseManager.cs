using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using UnityEngine.Events;

//[System.Serializable]
//public class EventVector3: UnityEvent<Vector3> { }

public class MouseManager : Singleton<MouseManager>
{
    //public static MouseManager Instance; //单例模式,现在因为使用了singleton的写法，就不需要这么写了，所以删除了捏

    public Texture2D point, doorway, attack, target, arrow;   //这部分是鼠标指针的图片

    RaycastHit hitInfo;

    public event Action<Vector3> OnMouseClicked;            //action方法被触发的时候，可以激活每一个它所调用的方法
    public event Action<GameObject> OnEnemyClicked;

    //void Awake()
    //{
    //    if (Instance != null)
    //    {
    //        Destroy(gameObject);
    //    }
    //    Instance = this;
    //}

    protected override void Awake()
    {
        base.Awake();
        //DontDestroyOnLoad(this);
    }

    void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture()
    {
        Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition); //从摄像机出去的，经过鼠标点击位置的一条射线，

        if(Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图

            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }

        }
    }

    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)  //如果点击鼠标并且从摄像头到鼠标pos的射线有穿过collider
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                OnMouseClicked?.Invoke(hitInfo.point);
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
        }
    }

}

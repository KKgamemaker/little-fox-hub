using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using UnityEngine.Events;

//[System.Serializable]
//public class EventVector3: UnityEvent<Vector3> { }

public class MouseManager : Singleton<MouseManager>
{
    //public static MouseManager Instance; //����ģʽ,������Ϊʹ����singleton��д�����Ͳ���Ҫ��ôд�ˣ�����ɾ������

    public Texture2D point, doorway, attack, target, arrow;   //�ⲿ�������ָ���ͼƬ

    RaycastHit hitInfo;

    public event Action<Vector3> OnMouseClicked;            //action������������ʱ�򣬿��Լ���ÿһ���������õķ���
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
        Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition); //���������ȥ�ģ����������λ�õ�һ�����ߣ�

        if(Physics.Raycast(ray, out hitInfo))
        {
            //�л������ͼ

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
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)  //��������겢�Ҵ�����ͷ�����pos�������д���collider
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

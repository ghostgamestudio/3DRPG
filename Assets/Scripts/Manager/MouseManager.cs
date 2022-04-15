using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//[System.Serializable]
//public class EventVector3 : UnityEvent<Vector3> { };
public class MouseManager : Singleton<MouseManager>
{

    public Texture2D point, doorway, attack, target, arrow;
    RaycastHit hitInfo;
    private Collider hitBox;
    public event UnityAction OnMouseClicked;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                //case "Ground":
                //    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                //    break;
                case "Enemy":
                    Cursor.SetCursor(attack,new Vector2(16,16),CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway,new Vector2(16,16),CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(arrow,new Vector2(16,16),CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //if (hitInfo.collider.gameObject.CompareTag("Ground"))
            //{
            //    OnMouseClicked?.Invoke(hitInfo.point);
            //}
            //if (hitBox.gameObject.CompareTag("Enemy"))
            //{
            //    OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            //}
            //if (hitInfo.collider.gameObject.CompareTag("Attackable"))
            //{
            //    OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            //}
            //if (hitInfo.collider.gameObject.CompareTag("Portal"))
            //{
            //    OnMouseClicked?.Invoke(hitInfo.point);
            //}
            OnMouseClicked?.Invoke();
        }
    }
}

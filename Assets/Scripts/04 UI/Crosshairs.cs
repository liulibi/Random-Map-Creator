using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask targetMask;

    private SpriteRenderer spriteRenderer;
    public Color highlightColor;
    private Color originColor;
    [SerializeField] private float rotateSpeed = 75;

    private void Start()
    {
        Cursor.visible = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originColor = spriteRenderer.color;
    }

    private void Update()
    {
        //transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
        transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime), Space.Self);
    }

    //在PlayerController脚本中调用
    public void DetectTargets(Ray _ray)
    {
        if(Physics.Raycast(_ray, 100, targetMask))
        {
            //如果检测到敌人的话，也就是置顶层的话，那么就鼠标变色
            spriteRenderer.color = highlightColor;
        }
        else
        {
            spriteRenderer.color = originColor;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//作为基类使用，应该由包含网络功能的脚本继承S
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody rb ;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private float camerRotationLimit = 85f;

    private Vector3 velocity = Vector3.zero;//速度：每秒钟移动的距离
    private Vector3 yRotation = Vector3.zero;//旋转角色
    private Vector3 xRotation = Vector3.zero;//旋转摄像机
    private Vector3 thrusterForce = Vector3.zero;//向上的推力
    private float camerRotationTotal = 0f;//累计转了多少度
    private float recoilForce = 0f;//后坐力
    private Vector3 lastFramePosition = Vector3.zero;//记录上一帧的位置
    private float eps = 0.001f;
    private Animator animator;
    private float distToGround = 0f;


    private void Start()
    {
        lastFramePosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        distToGround = GetComponent<Collider>().bounds.extents.y;
    }

    public void Rotate(Vector3 _yRotation,Vector3 _xRotation) 
    {
        yRotation = _yRotation;
        xRotation = _xRotation;
    }

    public void Move(Vector3 _velocity) 
    {
        velocity = _velocity;
    }

    public void Thrust(Vector3 _thrusterForce) 
    {
        thrusterForce = _thrusterForce;
    }

    public void AddRecoilForce(float newRecoilForce)
    {
        recoilForce += newRecoilForce;
    }

    private void PerformRotation() 
    {
        if (recoilForce < 0.1)
        {
            recoilForce = 0f;
        }

        if (yRotation != Vector3.zero || recoilForce > 0)//围绕y轴左右转
        {
            rb.transform.Rotate(yRotation + rb.transform.up * Random.Range(-2f * recoilForce, 2f * recoilForce));
        }
        if (xRotation != Vector3.zero || recoilForce > 0)//围绕x轴上下转
        {
            camerRotationTotal += xRotation.x - recoilForce;
            camerRotationTotal = Mathf.Clamp(camerRotationTotal, -camerRotationLimit, camerRotationLimit);
            cam.transform.localEulerAngles = new Vector3(camerRotationTotal, 0f, 0f);
        }

        recoilForce *= 0.5f;
    }

    private void PerformMovment() 
    {
        if (velocity != Vector3.zero)
        {
            //fixedDeltaTime 表示距离上一次FixedUpdate()执行时间 deltaTime：表示距离上一次Update()执行时间
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime );
        }
        //不重要
        if (thrusterForce != Vector3.zero)//向上移动
        {
            rb.AddForce(thrusterForce);//作用Time.fixedDeltaTime秒：0.02s
            thrusterForce = Vector3.zero;
        }
    }

    private void PerformAnimation() 
    {
        Vector3 deltaPosition = transform.position - lastFramePosition;
        lastFramePosition = transform.position;

        float forward = Vector3.Dot(deltaPosition, transform.forward);
        float right = Vector3.Dot(deltaPosition, transform.right);

        int direction = 0;
        if (forward > eps)
        {
            direction = 1;//前
        }
        else if (forward < -eps)
        {
            if (right > eps)
            {
                direction = 4;//右后
            }
            else if (right < -eps)
            {
                direction = 6;//左后
            }
            else
            {
                direction = 5;//后
            }
        }
        else
        {
            if (right > eps)
            {
                direction = 3;//右
            }
            else if (right < -eps)
            {
                direction = 7;//左
            }
        }

        //if (!Physics.Raycast(transform.position, Vector3.down, distToGround + 0.1f))
        //{
        //    direction = 8;
        //}//bug1

        //if (!Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f))
        //{
        //    print("true中心y值" + transform.position.x + "\n中心距离脚底距离" + distToGround);
        //    direction = 8;
        //}
        //else
        //{
        //    print("false中心y值" + transform.position.x + "\n中心距离脚底距离" + distToGround);
        //}

        animator.SetInteger("direction",direction);
    }

    private void FixedUpdate()
    {
        if (IsLocalPlayer)
        {
            PerformMovment();
            PerformRotation();
            PerformAnimation();
        }
    }

    private void Update()
    {
        if (!IsLocalPlayer)
        {
            PerformAnimation();
        }
    }
}

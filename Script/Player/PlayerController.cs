using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//��Ϊ����ʹ�ã�Ӧ���ɰ������繦�ܵĽű��̳�S
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody rb ;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private float camerRotationLimit = 85f;

    private Vector3 velocity = Vector3.zero;//�ٶȣ�ÿ�����ƶ��ľ���
    private Vector3 yRotation = Vector3.zero;//��ת��ɫ
    private Vector3 xRotation = Vector3.zero;//��ת�����
    private Vector3 thrusterForce = Vector3.zero;//���ϵ�����
    private float camerRotationTotal = 0f;//�ۼ�ת�˶��ٶ�
    private float recoilForce = 0f;//������
    private Vector3 lastFramePosition = Vector3.zero;//��¼��һ֡��λ��
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

        if (yRotation != Vector3.zero || recoilForce > 0)//Χ��y������ת
        {
            rb.transform.Rotate(yRotation + rb.transform.up * Random.Range(-2f * recoilForce, 2f * recoilForce));
        }
        if (xRotation != Vector3.zero || recoilForce > 0)//Χ��x������ת
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
            //fixedDeltaTime ��ʾ������һ��FixedUpdate()ִ��ʱ�� deltaTime����ʾ������һ��Update()ִ��ʱ��
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime );
        }
        //����Ҫ
        if (thrusterForce != Vector3.zero)//�����ƶ�
        {
            rb.AddForce(thrusterForce);//����Time.fixedDeltaTime�룺0.02s
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
            direction = 1;//ǰ
        }
        else if (forward < -eps)
        {
            if (right > eps)
            {
                direction = 4;//�Һ�
            }
            else if (right < -eps)
            {
                direction = 6;//���
            }
            else
            {
                direction = 5;//��
            }
        }
        else
        {
            if (right > eps)
            {
                direction = 3;//��
            }
            else if (right < -eps)
            {
                direction = 7;//��
            }
        }

        //if (!Physics.Raycast(transform.position, Vector3.down, distToGround + 0.1f))
        //{
        //    direction = 8;
        //}//bug1

        //if (!Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f))
        //{
        //    print("true����yֵ" + transform.position.x + "\n���ľ���ŵ׾���" + distToGround);
        //    direction = 8;
        //}
        //else
        //{
        //    print("false����yֵ" + transform.position.x + "\n���ľ���ŵ׾���" + distToGround);
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

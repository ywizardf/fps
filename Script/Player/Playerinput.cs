using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerinput : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private PlayerController controller;
    [SerializeField]
    private float lookSensitivity = 8f;//�Ӵ�����ƶ������Ľ�ɫ��ת
    [SerializeField]
    private float thrusterForce = 20f;//����

    private float distToGround = 0f;
    
    //private ConfigurableJoint joint;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        distToGround = GetComponent<Collider>().bounds.extents.y;
        //joint = GetComponent<ConfigurableJoint>();//????????
    }

    // Update is called once per frame
    void Update()
    {
        //x�᷽�����ң�
        float xMov = Input.GetAxisRaw("Horizontal");//0,-1,1
        //z�᷽��(��ǰ)
        float yMov = Input.GetAxisRaw("Vertical"); //0,-1,1

        //�ƶ��ٶ�
        Vector3 velocity = (transform.right * xMov + transform.forward * yMov).normalized * speed;
        controller.Move( velocity );

        float xMouse = Input.GetAxisRaw("Mouse X");//Χ��x������ת��
        float yMouse = Input.GetAxisRaw("Mouse Y");//Χ��y������ת��
        //print(xMouse.ToString() + " " + yMouse.ToString());

        Vector3 yRotation = new Vector3(0f ,xMouse, 0f) * lookSensitivity;//Χ��y������ת
        Vector3 xRotation = new Vector3(-yMouse, 0f, 0f) * lookSensitivity;//Χ��x������ת

        controller.Rotate( yRotation , xRotation );

        //Vector3 force = Vector3.zero;
        //if (Input.GetButton("Jump"))
        //{
        //    force = Vector3.up * thrusterForce;
        //    joint.yDrive = new JointDrive
        //    {
        //        positionSpring = 0f,
        //        positionDamper = 0f,
        //        maximumForce = 0f,
        //    };
        //}
        //else 
        //{
        //    joint.yDrive = new JointDrive
        //    {
        //        positionSpring = 20f,
        //        positionDamper = 0f,
        //        maximumForce = 40f,
        //    };
        //}
        //controller.Thrust(force);
        if (Input.GetButton("Jump"))
        {
            if (Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f)) //���µ��߶Σ����������Ϳ�����Ծ
            {
                Vector3 force = Vector3.up * thrusterForce;
                controller.Thrust(force);
            }
        }
    }
}

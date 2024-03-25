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
    private float lookSensitivity = 8f;//加大鼠标移动带来的角色旋转
    [SerializeField]
    private float thrusterForce = 20f;//推力

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
        //x轴方向（左右）
        float xMov = Input.GetAxisRaw("Horizontal");//0,-1,1
        //z轴方向(向前)
        float yMov = Input.GetAxisRaw("Vertical"); //0,-1,1

        //移动速度
        Vector3 velocity = (transform.right * xMov + transform.forward * yMov).normalized * speed;
        controller.Move( velocity );

        float xMouse = Input.GetAxisRaw("Mouse X");//围绕x轴上下转向
        float yMouse = Input.GetAxisRaw("Mouse Y");//围绕y轴左右转向
        //print(xMouse.ToString() + " " + yMouse.ToString());

        Vector3 yRotation = new Vector3(0f ,xMouse, 0f) * lookSensitivity;//围绕y轴左右转
        Vector3 xRotation = new Vector3(-yMouse, 0f, 0f) * lookSensitivity;//围绕x轴上下转

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
            if (Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f)) //向下的线段，如果有物体就可以跳跃
            {
                Vector3 force = Vector3.up * thrusterForce;
                controller.Thrust(force);
            }
        }
    }
}

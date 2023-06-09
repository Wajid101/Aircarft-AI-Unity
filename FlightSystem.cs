/// <summary>
/// Flight system. This script is Core plane system
/// </summary>
using UnityEngine;
// included all necessary component
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]


public class FlightSystem : MonoBehaviour
{

    public AIController Instance;
    public float Speed = 50.0f;// Speed
    public float SpeedMax = 60.0f;// Max speed
    public float RotationSpeed = 50.0f;// Turn Speed
    public float SpeedPitch = 2;// rotation X
    public float SpeedRoll = 3;// rotation Z
    public float SpeedYaw = 1;// rotation Y
    public float DampingTarget = 10.0f;// rotation speed to facing to a target
    public bool AutoPilot = false;// if True this plane will follow a target automatically
    private float MoveSpeed = 10;// normal move speed

    [HideInInspector]
    public bool SimpleControl = false;// set true is enabled casual controling
    [HideInInspector]
    public bool FollowTarget = false;
    [HideInInspector]
    public Vector3 PositionTarget = Vector3.zero;// current target position
    [HideInInspector]
    private Vector3 positionTarget = Vector3.zero;
    private Quaternion mainRot = Quaternion.identity;
    [HideInInspector]
    public float roll = 0;
    [HideInInspector]
    public float pitch = 0;
    [HideInInspector]
    public float yaw = 0;
    public Vector2 LimitAxisControl = new Vector2(2, 1);// limited of axis rotation magnitude
    public bool FixedX;
    public bool FixedY;
    public bool FixedZ;
    public float Mess = 30;
    public bool DirectVelocity = true;// if true this riggidbody will not receive effect by other force.
    public float DampingVelocity = 5;
    private Rigidbody Rigbdy;
    bool flyup = false,takeup=false;
    void Start()
    {
        // define all component
        mainRot = this.transform.rotation;
        Rigbdy = GetComponent<Rigidbody>();
        Instance = GetComponent<AIController>();
        Rigbdy.mass = Mess;
    }

    void FixedUpdate()
    {
        if (!this.Rigbdy)
            return;

        Quaternion AddRot = Quaternion.identity;
        Vector3 velocityTarget = Vector3.zero;
        
        if (Time.time>8  && flyup==false)
        {
            flyup = true;
            pitch = -0.05f;
        }
        else if (Time.time>20 && takeup==false)
        {
            takeup = true;
            Instance.enabled = true;
            pitch = 0;
           // Debug.Log(pitch);
        }
       
       
        if (AutoPilot)
        {// if auto pilot
            if (FollowTarget)
            {
                // rotation facing to the positionTarget
                positionTarget = Vector3.Lerp(positionTarget, PositionTarget, Time.fixedDeltaTime * DampingTarget);
                Vector3 relativePoint = this.transform.InverseTransformPoint(positionTarget).normalized;
                mainRot = Quaternion.LookRotation(positionTarget - this.transform.position);
                Rigbdy.rotation = Quaternion.Lerp(Rigbdy.rotation, mainRot, Time.fixedDeltaTime * (RotationSpeed * 0.01f));
                Rigbdy.rotation *= Quaternion.Euler(-relativePoint.y * 2, 0, -relativePoint.x * 10);

            }
            velocityTarget = (Rigbdy.rotation * Vector3.forward) * (Speed + MoveSpeed);
        }
        else
        {
            // axis control by input
            AddRot.eulerAngles = new Vector3(pitch, yaw, -roll);
            mainRot *= AddRot;

            if (SimpleControl)
            {
                Quaternion saveQ = mainRot;

                Vector3 fixedAngles = new Vector3(mainRot.eulerAngles.x, mainRot.eulerAngles.y, mainRot.eulerAngles.z);

                if (FixedX)
                    fixedAngles.x = 1;
                if (FixedY)
                    fixedAngles.y = 1;
                if (FixedZ)
                    fixedAngles.z = 1;

                saveQ.eulerAngles = fixedAngles;


                mainRot = Quaternion.Lerp(mainRot, saveQ, Time.fixedDeltaTime * 2);
            }


            Rigbdy.rotation = Quaternion.Lerp(Rigbdy.rotation, mainRot, Time.fixedDeltaTime * RotationSpeed);
            velocityTarget = (Rigbdy.rotation * Vector3.forward) * (Speed + MoveSpeed);

        }
        // add velocity to the riggidbody
        if (DirectVelocity)
        {
            Rigbdy.velocity = velocityTarget;
        }
        else
        {
            Rigbdy.velocity = Vector3.Lerp(Rigbdy.velocity, velocityTarget, Time.fixedDeltaTime * DampingVelocity);
        }
        yaw = Mathf.Lerp(yaw, 0, Time.deltaTime);
        MoveSpeed = Mathf.Lerp(MoveSpeed, Speed, Time.deltaTime);
    }

    // Input function. ( roll and pitch)
    public void AxisControl(Vector2 axis)
    {
        if (SimpleControl)
        {
            LimitAxisControl.y = LimitAxisControl.x;
        }
        roll = Mathf.Lerp(roll, Mathf.Clamp(axis.x, -LimitAxisControl.x, LimitAxisControl.x) * SpeedRoll, Time.deltaTime);
        pitch = Mathf.Lerp(pitch, Mathf.Clamp(axis.y, -LimitAxisControl.y, LimitAxisControl.y) * SpeedPitch, Time.deltaTime);
    }
    // Input function ( yaw) 
    public void TurnControl(float turn)
    {
        yaw += turn * Time.deltaTime * SpeedYaw;
    }
    // Speed up
    public void SpeedUp(float delta)
    {
        if (delta >= 0)
            MoveSpeed = Mathf.Lerp(MoveSpeed, SpeedMax, Time.deltaTime * (10 * delta));
    }
    public void SpeedUp()
    {
        MoveSpeed = Mathf.Lerp(MoveSpeed, SpeedMax, Time.deltaTime * 10);
    }
}

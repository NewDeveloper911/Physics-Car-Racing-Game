using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField]
    Transform[] wheels;

    [SerializeField]
    float enginePower;
    [SerializeField]
    float power, brake, steer;

    const float maxSteer = 25f;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb.centerOfMass = new Vector3(0, -0.5f, 0.3f);
    }

    WheelCollider GetCollider(int n)
    {
        return wheels[n].gameObject.GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        power = Input.GetAxis("Vertical") * enginePower * Time.deltaTime;

        steer = Input.GetAxis("Horizontal") * maxSteer;
        brake = Input.GetKey(KeyCode.Space) ? rb.mass * 0.75f : 0.0f;
    }

    private void FixedUpdate()
    {

        GetCollider(0).steerAngle = steer;
        GetCollider(1).steerAngle = steer;

        if (brake > 0.0f)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                GetCollider(i).brakeTorque = brake;
            }
            GetCollider(2).motorTorque = 0.0f;
            GetCollider(3).motorTorque = 0.0f;
        }
        else
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                GetCollider(i).brakeTorque = 0.0f;
            }
            GetCollider(2).motorTorque = power;
            GetCollider(3).motorTorque = power;
        }
    }
}

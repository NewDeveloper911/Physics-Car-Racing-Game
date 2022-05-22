using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCarController : MonoBehaviour
{
    [Header("Miscellaneous")]
    [SerializeField] LayerMask track;

    [Header("Forces")]
    [SerializeField] Vector3 tractionForce, dragForce, rollResistForce, longForce;
    [SerializeField] int liftForce, flipForce;
    [SerializeField] Vector3 maxForce, driveForce, lateralForce, totalForce;
    [SerializeField] float enginePower, userForward, userRight;
    [SerializeField] Vector3 frontWeight, backWeight;
    

    [Header("Constants")]
    [SerializeField] float dragConstant, rollResistConstant;
    [SerializeField] float corneringStiffness;
    [SerializeField] float tyreFrictionCoefficient = 1.5f;
    [SerializeField] float constMultiplier = 30f;
    [SerializeField] float airDensity = 1.29f; //Measured in kg/m^3 

    [Header("Car bodies")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform carTransform;
    [Range(0,1)]
    [SerializeField] float frictionCoefficient = 0.3f;
    [SerializeField] float frontalArea = 2.2f;
    [SerializeField] Vector3 weight;
    [SerializeField] Transform[] wheels;
    [SerializeField] Rigidbody[] rb_wheels;
    [SerializeField] float c, b, L, R, h, steeringRadius, rearSlipAngle, frontSlipAngle, sideSlipAngle, alpha;
    [SerializeField] float maxSteer;

    //Use Wheelhit.sidewaysslip for the sideslip

    /*
     A lot of this code is derived from Marco Monster's Car Physics for Games.
     The equations used to calculate the physics of the car are found in that document.
     Some of the equations relating to weight distribution (he called it weight transfer), some torque
     ,gear ratios and others have not been included here but could be quite beneficial for
     those aiming to achieve a more realistic experience.
     */


     // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        weight = rb.mass * Physics.gravity;
        L = Vector3.Distance(wheels[0].transform.position, wheels[2].transform.position);
        b = wheels[0].transform.position.x - rb.centerOfMass.x;
        c = wheels[3].transform.position.x - rb.centerOfMass.x;
    }

    WheelCollider GetCollider(int n)
    {
        return wheels[n].gameObject.GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        // Here, I just get the inputs for my movement
        userForward = Input.GetAxis("Vertical") * Time.deltaTime * rb.mass;
        userRight = Input.GetAxis("Horizontal") * maxSteer;

        //Here, I set the steering angle of the car
       
            GetCollider(0).steerAngle = userRight;
            GetCollider(1).steerAngle = userRight;

        //Flips the car when it falls upside-down
        if (Input.GetKeyDown(KeyCode.T))
        {
            rb.AddForce(0,liftForce,0);
            rb.AddRelativeTorque(new Vector3(0,0,flipForce));
        }
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        Ray downRay = new Ray(rb.centerOfMass, -Vector3.up);
        if (Physics.Raycast(downRay, out hit))
        {
            h = hit.distance;
        }

        frontWeight = (c / L) * weight - (h / L) * totalForce;
        backWeight = (b / L) * weight + (h / L) * totalForce;
        maxForce = frictionCoefficient * weight;

        //Here, we calculate the constants for the car
        float v = 0.5f * frictionCoefficient * frontalArea * airDensity * rb.velocity.sqrMagnitude;
        dragConstant = v;
        rollResistConstant = constMultiplier * dragConstant;

        //This controls the force which causes forward movement
        tractionForce = carTransform.forward.normalized * enginePower * userForward;
        //This controls the resistive forward forces
        dragForce = -dragConstant * rb.velocity * rb.velocity.magnitude;
        rollResistForce = -rollResistConstant * rb.velocity;

        maxForce = tyreFrictionCoefficient * weight;

        
        //Here, all of the longitudinal (forward) forces are totalled

        longForce = tractionForce + dragForce + rollResistForce;
        /* 
            If the traction forces exceed all other forces, then the car accelerates. This would cause
            velocity and therefore resistive forces to also increase up to a point where they cancel
            out. This is the top speed for a certain engine power
        */
        //Raycast only works for calculating mass. Can somehow still fly in the sky. What is this?
        if (Physics.CheckSphere(rb.transform.position, 1f, track))
        {
            rb.AddForce(longForce);
        }
    
    }

}

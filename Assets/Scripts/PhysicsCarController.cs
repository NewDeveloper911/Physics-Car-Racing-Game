using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCarController : MonoBehaviour
{
    [Header("Forces")]
    [SerializeField] Vector3 tractionForce, dragForce, rollResistForce, longForce;
    [SerializeField] Vector3 brakingForce, maxForce, driveForce, lateralForce, totalForce;
    [SerializeField] Vector3 corneringForce, latRearForce, latFrontForce, centripedalForce;
    [SerializeField] float enginePower, userForward, userRight;
    [SerializeField] Vector3 frontWeight, backWeight;
    [SerializeField] Vector3 rearTorque, frontTorque, totalTorque;
    

    [Header("Constants")]
    [SerializeField] float dragConstant, rollResistConstant, brakingConstant;
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
    [SerializeField] Vector3 latVelocity, longVelocity;
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


        if (Input.GetKey(KeyCode.Space))
        {
            brakingForce = -carTransform.forward.normalized * brakingConstant;
            if(rb.velocity.magnitude > 0)
            {
                rb.AddForce(brakingForce);
            }
        }

        
        //Here, all of the longitudinal (forward) forces are totalled

        longForce = tractionForce + dragForce + rollResistForce;
        /* 
            If the traction forces exceed all other forces, then the car accelerates. This would cause
            velocity and therefore resistive forces to also increase up to a point where they cancel
            out. This is the top speed for a certain engine power
        */
        if (Physics.Raycast(downRay, out hit))
        {
            rb.AddForce(longForce);
        }

    
        ////This section relates to low-speed cornering, not really useful for drifting
        //    //This will determine the circle radius related to turning, in effect, calculating how much we turn
        //    R = L / Mathf.Sin(userRight);

        ////This section covers high-speed turning
        //    //This relates to the car
        //        //sideSlipAngle = Vector3.Angle(carTransform.forward, rb.velocity);
        //        sideSlipAngle = Mathf.Atan(rb.velocity.z / rb.velocity.x);

        ////This relates to the wheels
        //for(int i = 0; i < 4; i++)
        //{
        //    //alpha represents the slip angle for each wheel
        //    alpha = Vector3.Angle(wheels[i].forward, rb.velocity) - 90f;

        //    //These values are supposed to represent the magnitudes of these velocities
        //    longVelocity = Mathf.Cos(alpha) * rb.velocity;
        //    latVelocity = Mathf.Sin(alpha) * rb.velocity;

        //    /*
        //        Remember that these forces are supposed to be acting upon the wheels themselves,
        //        so add rigidbodies accordingly if that works out
        //     */

        //    //The lateral velocity causes the cornering force to counteract it
        //        //This relates to only the front wheels
        //        frontSlipAngle = Mathf.Atan((latVelocity.magnitude + (rb.angularVelocity.magnitude * b)) / longVelocity.magnitude) - (userRight * Mathf.Sign(longVelocity.magnitude));
        //        for (i = 0; i < 2; i++)
        //        {
        //            //Calculates the lateral force of the wheels
        //            latFrontForce = lateralForce.normalized * (frontWeight.magnitude / 2f);

        //            corneringForce = latRearForce + Mathf.Cos(userRight) * latFrontForce;
        //            lateralForce += latFrontForce;

        //            frontTorque = Mathf.Cos(userRight) * latFrontForce * b;
        //            //rb_wheels[i].AddForce(lateralForce);
        //        }

        //        centripedalForce = corneringForce;
        //        steeringRadius = centripedalForce.magnitude / (rb.mass * Mathf.Pow(rb.velocity.magnitude, 2f));

        //    lateralForce = new Vector3(0f, 0f, 0f);
        //    //This relates to only the rear wheels
        //    rearSlipAngle = Mathf.Atan((latVelocity.magnitude - (rb.angularVelocity.magnitude * c)) / longVelocity.magnitude);
        //    for (i = 2; i < 4; i++)
        //    {
        //        //Calculates the lateral force of the wheels
        //        latRearForce = lateralForce.normalized * (backWeight.magnitude / 2f);

        //        lateralForce += latRearForce;

        //        rearTorque = -latRearForce * c;
        //        //rb_wheels[i].AddForce(lateralForce);
        //    }

        //}

        //totalTorque = frontTorque + rearTorque;
        //rb.AddTorque(totalTorque);
    
    }
}

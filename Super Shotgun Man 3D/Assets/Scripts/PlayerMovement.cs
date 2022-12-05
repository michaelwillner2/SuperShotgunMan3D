using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float friction, c_friction;
    public float _movespeed, _accelspeed, horizontal_gravity;
    public float air_cap, jump_speed;

    public float ground_check_distance;

    private bool grounded;
    private float rot_x, rot_y;

    public Vector2 mouse_sens;

    private Transform cam_transform;
    private Rigidbody rb;

    void Look()
    {
        rot_x += Input.GetAxis("Mouse X") * mouse_sens.x;
        rot_y += Input.GetAxis("Mouse Y") * mouse_sens.y;

        rot_y = Mathf.Clamp(rot_y, -90.0f, 90.0f);

        cam_transform.localRotation = Quaternion.Euler(-rot_y, 0.0f, 0.0f);
        transform.rotation = Quaternion.Euler(0.0f, rot_x, 0.0f);
    }

    bool CheckGrounded()
    {
        Debug.DrawRay(transform.position - Vector3.up * 0.9f, Vector3.down * ground_check_distance, Color.blue, Time.deltaTime);
        return Physics.Raycast(transform.position - Vector3.up * 0.9f, -Vector3.up, ground_check_distance, LayerMask.GetMask("Ground"));
    }

    void ApplyFriction()
    {
        Vector2 input_vector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        float control = 1.0f;
        if (input_vector.magnitude == 0.0f) control = c_friction;
        float new_speed = _movespeed - Time.deltaTime * friction * control;
        if (new_speed < 0.0f) new_speed = 0.0f;
        new_speed /= _movespeed;

        rb.velocity *= new_speed;
    }

    void GroundAccel()
    {
        float addspeed, accelspeed, currentspeed, wishspeed;

        Vector3 wishdir = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") * horizontal_gravity;
        wishdir.Normalize();

        wishspeed = wishdir.magnitude * _movespeed;

        currentspeed = Vector3.Dot(rb.velocity, wishdir);
        addspeed = wishspeed - currentspeed;

        if (addspeed <= 0)
            return;

        accelspeed = _accelspeed * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        rb.velocity += new Vector3(accelspeed * wishdir.x, accelspeed * wishdir.y, accelspeed * wishdir.z);
    }

    void AirAccel()
    {
        float addspeed, accelspeed, currentspeed, wishspeed;

        Vector3 wishdir = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") * horizontal_gravity;
        wishdir.Normalize();

        wishspeed = wishdir.magnitude * _movespeed;

        if (wishspeed > air_cap)
            wishspeed = air_cap;

        currentspeed = Vector3.Dot(rb.velocity, wishdir);
        addspeed = wishspeed - currentspeed;

        if (addspeed <= 0)
            return;

        accelspeed = _accelspeed * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        rb.velocity += new Vector3(accelspeed * wishdir.x, accelspeed * wishdir.y, accelspeed * wishdir.z);
    }

    // Start is called before the first frame update
    void Start()
    {
        rot_y = 0.0f;
        cam_transform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = CheckGrounded();
        Look();

        if (grounded)
        {
            ApplyFriction();
            GroundAccel();
            if (Input.GetButton("Jump") && rb.velocity.y <= 0.0f) rb.velocity += Vector3.up * jump_speed;
        }
        else
        {
            AirAccel();
        }
    }
}

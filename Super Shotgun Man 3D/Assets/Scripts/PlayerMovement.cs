using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float friction, c_friction;
    public float _movespeed, _accelspeed, horizontal_gravity;
    public float air_cap, jump_speed;

    public float i_slidespeed, slidespeed, sj_speed, max_sj_speed;

    public float ground_check_distance;

    private bool grounded, sliding, aircrouching;
    private float rot_x, rot_y;

    public Vector2 mouse_sens;

    private Transform cam_transform;
    private Rigidbody rb;
    private CapsuleCollider col;

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
        Vector3 crouch_offset = Vector3.zero;
        
        if (aircrouching)
            crouch_offset = new Vector3(0.0f, 1.0f, 0.0f);

        Debug.DrawRay(transform.position - Vector3.up * 0.9f + crouch_offset, Vector3.down * ground_check_distance, Color.blue, Time.deltaTime);
        return Physics.Raycast(transform.position - Vector3.up * 0.9f + crouch_offset, -Vector3.up, ground_check_distance, LayerMask.GetMask("Ground"));
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

    //set sliding flag, capsule collider height, and camera positions
    void CheckSliding()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (sliding == false)
            {
                col.height = 1.0f;
                if (grounded)
                {
                    cam_transform.localPosition = new Vector3(0.0f, -.25f, 0.0f);
                    col.center = new Vector3(0.0f, -0.5f, 0.0f);
                }
                else
                {
                    aircrouching = true;
                    cam_transform.localPosition = new Vector3(0.0f, .75f, 0.0f);
                    col.center = new Vector3(0.0f, 0.5f, 0.0f);
                }
            }
            sliding = true;
        }
        else
        {
            if (sliding == true)
            {
                if (aircrouching)
                {
                    transform.position += Vector3.up * 0.5f;
                    aircrouching = false;
                }
                cam_transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
                col.height = 2.0f;
                col.center = new Vector3(0.0f, 0.0f, 0.0f);
            }
            sliding = false;
        }
    }

    void Slide()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        rot_y = 0.0f;
        cam_transform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = CheckGrounded();
        Look();
        CheckSliding();
        Debug.Log($"Sliding: {sliding}, Air Crouching: {aircrouching}");
        if (grounded)
        {
            ApplyFriction();
            if (!sliding)
                GroundAccel();
            else
                Slide();

            if (Input.GetButton("Jump") && rb.velocity.y <= 0.0f)
            {
                rb.velocity += Vector3.up * jump_speed;
                if(sliding)
                    rb.velocity += transform.forward * Mathf.Clamp(sj_speed * new Vector2(rb.velocity.x, rb.velocity.z).magnitude, 0.0f, max_sj_speed);
            }
        }
        else
        {
            if (!sliding)
                AirAccel();
            else
                Slide();
        }
    }
}

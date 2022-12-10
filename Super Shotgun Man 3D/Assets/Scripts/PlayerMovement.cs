using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float friction, c_friction;
    public float _movespeed, _accelspeed, horizontal_gravity;
    public float airmult_cap, jump_speed;

    public float slidespeed, sj_speed, max_sj_speed;

    public float ground_check_distance;

    public float max_sj_airspeed, slide_timer, current_airspeed, slope_accel;

    public float viewbob_amplitude, viewbob_frequency, cam_roll_amount;

    private bool grounded, sliding, aircrouching, set_slide_vector, set_slide_speed;
    private float rot_x, rot_y;

    private float max_slide_timer, current_slide_speed;

    private float animation_tick;

    public Vector2 mouse_sens;

    private Transform cam_transform;
    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector3 slide_vector;
    private Vector3 cam_pivot;

    void Look()
    {
        //handle camera rotation
        rot_x += Input.GetAxis("Mouse X") * mouse_sens.x;
        rot_y += Input.GetAxis("Mouse Y") * mouse_sens.y;

        rot_y = Mathf.Clamp(rot_y, -90.0f, 90.0f);

        transform.rotation = Quaternion.Euler(0.0f, rot_x, 0.0f);

        //handle camera roll and bobbing
        cam_transform.localPosition = cam_pivot;
        float velocity_scalar = rb.velocity.magnitude / _movespeed;
        if (grounded && !sliding) {
            cam_transform.localPosition += Vector3.up * Mathf.Sin(animation_tick * viewbob_frequency) * viewbob_amplitude * velocity_scalar;
        }

        float vel_proj = Vector3.Dot(rb.velocity.normalized, transform.right) * Mathf.Clamp01(velocity_scalar);
        cam_transform.localRotation = Quaternion.Euler(-rot_y, 0.0f, Mathf.Lerp(cam_roll_amount, -cam_roll_amount, (vel_proj + 1.0f) / 2.0f));
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

        if (wishspeed > airmult_cap)
            wishspeed = airmult_cap;

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
                    if (aircrouching)
                    {
                        transform.position += Vector3.up;
                        aircrouching = false;
                    }
                    cam_pivot = new Vector3(0.0f, -.25f, 0.0f);
                    col.center = new Vector3(0.0f, -0.5f, 0.0f);
                    sliding = true;
                }
                else
                {
                    aircrouching = true;
                    cam_pivot = new Vector3(0.0f, .75f, 0.0f);
                    col.center = new Vector3(0.0f, 0.5f, 0.0f);
                }
            }
        }
        else
        {
            if (sliding == true || aircrouching)
            {
                if (aircrouching)
                {
                    transform.position += Vector3.up * 0.5f;
                    aircrouching = false;
                }
                cam_pivot = new Vector3(0.0f, 0.5f, 0.0f);
                col.height = 2.0f;
                col.center = new Vector3(0.0f, 0.0f, 0.0f);
            }
            sliding = false;
            set_slide_vector = false;
            set_slide_speed = false;
        }
    }

    //all slide physics
    void Slide()
    {
        //cap airspeed
        if (!grounded) {
            if (rb.velocity.magnitude > max_sj_airspeed)
            {
                Vector3 air_vel = rb.velocity.normalized;
                rb.velocity = air_vel * max_sj_airspeed;
            }
            slide_timer = max_slide_timer;
            current_airspeed = rb.velocity.magnitude;
        }
        //simply slide in a direction
        else
        {
            if (!set_slide_vector)
            {
                slide_vector = (transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal")).normalized;
                if (slide_vector.magnitude == 0.0f) slide_vector = transform.forward;
                set_slide_vector = true;
            }
            if (!set_slide_speed)
            {
                current_slide_speed = current_airspeed;
            }
            //Update the slide vector based off from slopes that the player is currently on
            Vector3 crouch_offset = Vector3.zero;

            if (aircrouching)
                crouch_offset = new Vector3(0.0f, 1.0f, 0.0f);
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position - Vector3.up * 0.9f + crouch_offset, -Vector3.up, out hit, ground_check_distance, LayerMask.GetMask("Ground")))
            {
                //first get the normal of the surface if it is facing straight up then don't do anything else
                Vector3 slope_norm = hit.normal;

                if (hit.normal != Vector3.up)
                {
                    //calculate the tangent and bitangent of the slope, adjust the slide vector accordingly
                    Vector3 tangent = Vector3.Cross(slope_norm, Vector3.up);
                    Vector3 bitangent = Vector3.Cross(slope_norm, tangent);

                    slide_vector += bitangent * slope_accel * Time.deltaTime* Mathf.Lerp(slidespeed, current_slide_speed, slide_timer / max_slide_timer);
                }
                else
                {
                    if (slide_timer > 0.0f)
                        slide_timer -= Time.deltaTime;
                    else
                        slide_timer = 0.0f;
                }
            }
            
            rb.velocity += slide_vector * 1000.0f;
            float target_slide_speed = Mathf.Lerp(slidespeed, current_slide_speed, slide_timer / max_slide_timer);
            rb.velocity = rb.velocity.normalized * target_slide_speed;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rot_y = 0.0f;
        max_slide_timer = slide_timer;
        cam_transform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        cam_pivot = cam_transform.localPosition;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = CheckGrounded();
        Look();
        CheckSliding();
        if (grounded)
        {
            ApplyFriction();
            if (!sliding)
                GroundAccel();
            else
                Slide();
            
            if (Input.GetButtonDown("Jump") && rb.velocity.y <= 0.0f)
            {
                rb.velocity = new Vector3(rb.velocity.x, jump_speed, rb.velocity.z);
                if (sliding)
                {
                    float current_vel = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
                    if (current_vel < 1.0f) current_vel = 1.0f;
                    rb.velocity = transform.forward * Mathf.Clamp(sj_speed * current_vel, 0.0f, max_sj_speed) + transform.up * jump_speed + new Vector3(0.0f, Mathf.Clamp01(rb.velocity.y), 0.0f);
                    set_slide_vector = false;
                }
            }
        }
        else
        {
            if (!sliding)
                AirAccel();
            else
                Slide();
            set_slide_speed = false;
        }
    }

    private void FixedUpdate()
    {
        animation_tick += Time.deltaTime * rb.velocity.magnitude / _movespeed;
    }
}

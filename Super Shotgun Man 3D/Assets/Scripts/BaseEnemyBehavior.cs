using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseEnemyBehavior : MonoBehaviour
{
    [System.Serializable]
    private struct AnimAction
    {
        public int activation_frame;
        public UnityEvent action;
    }

    [System.Serializable]
    private struct Animation
    {
        public int starting_index;
        public int frame_count;
        public bool looping;
        public float speed_fps;

        public List<AnimAction> actions;
    }

    private int _hp;

    private int current_frame;
    private int current_animation;

    [SerializeField]
    private int step_count_max, step_count_min, reaction_time;

    [SerializeField]
    private float step_distance, step_frequency, max_step_height;
    [SerializeField]
    private float min_attack_distance, max_attack_distance;
    [SerializeField]
    private float knockback_resistance;

    private int last_animation, last_frame;

    [SerializeField]
    private int step_count, targeting_threshold, max_reaction_time;

    [SerializeField]
    private Vector3 lookdir;
    [SerializeField]
    private Texture2DArray spritesheet;

    private float anim_tick;

    private float step_frequency_max, step_height;

    private bool anim_completed;

    private Material visual_mat;
    [SerializeField]
    private List<Animation> animations;
    private BoxCollider col;
    private GameObject target;

    public int HP
    {
        get { return _hp; }
        set
        {
            _hp = value;
        }
    }

    public void TakeDamage(int amount)
    {
        _hp -= amount;
        if (_hp < 0)
            _hp = 0;
    }

    public void TakeDamage(int amount, Vector3 direction)
    {
        _hp -= amount;
        if (_hp < 0)
            _hp = 0;
        GetComponent<Rigidbody>().AddForce(direction.normalized * ((float)amount / knockback_resistance));
    }

    //function handles collision checks
    public virtual bool CheckNextPostion(Vector3 direction)
    {
        Vector3 half_extents = new Vector3(col.size.x, col.size.y, col.size.z) * 0.99f;
        RaycastHit box_hit;
        bool detect_wall = !Physics.BoxCast(transform.position, half_extents, direction, out box_hit, transform.rotation, step_distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("Player"));

        //if a wall is detected see if you can step over the wall
        Vector3 raycast_offset = transform.position + new Vector3(half_extents.x * direction.x, 0.0f, half_extents.z * direction.z) + direction * step_distance * 0.6f;
        if (!detect_wall)
        {
            RaycastHit hit;
            Physics.Raycast(raycast_offset, -Vector3.up, out hit, half_extents.y + max_step_height, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == box_hit.collider.gameObject)
                {
                    float wall_height = transform.position.y - hit.distance - (transform.position.y - half_extents.y);

                    if (wall_height <= max_step_height)
                    {
                        step_height = wall_height;
                        return true;
                    }
                }
            }
            return detect_wall;
        }
        //check if there is a dropoff bigger than the step height
        if (!Physics.Raycast(raycast_offset, -Vector3.up, (2.02f * half_extents.y + max_step_height), LayerMask.GetMask("Ground")))
            return false;
        return true;
    }

    public virtual void UpdateAnimationViewAngle()
    {
        //First get the player's viewing angle and take the dot product with the enemy's look direction
        Vector2 eviewangle = new Vector2(lookdir.x, lookdir.z).normalized;
        Vector2 pviewangle = new Vector2(transform.position.x - Camera.main.transform.position.x, transform.position.z - Camera.main.transform.position.z).normalized;
        float frame_lerp = Vector2.Dot(eviewangle, pviewangle);

        //interpolate the current frame of animation
        int calculated_frame = animations[current_animation].starting_index + current_frame * 5;

        //looking at:
        //front
        if (frame_lerp < -0.75f)
            calculated_frame += 0;
        //3/4 front
        else if (frame_lerp < -0.25f)
            calculated_frame += 1;
        //side
        else if (frame_lerp < .25f)
            calculated_frame += 2;
        //3/4 back
        else if (frame_lerp < .75f)
            calculated_frame += 3;
        //back
        else
            calculated_frame += 4;
        
        visual_mat.SetFloat("_SpriteIndex", calculated_frame);

        //now handle flipping the sprite if viewed from the right or left
        Vector2 erightangle = -new Vector2(Vector3.Cross(lookdir, Vector3.up).x, Vector3.Cross(lookdir, Vector3.up).z);
        frame_lerp = Vector2.Dot(erightangle, pviewangle);

        if (frame_lerp > 0.0f)
            visual_mat.SetFloat("_Flip", 0);
        else
            visual_mat.SetFloat("_Flip", 1);
    }

    public virtual void Animate()
    {
        if (animations.Count == 0) return;
        //first get the active animation
        Animation anim = animations[current_animation];

        if(last_animation != current_animation)
        {
            anim_tick = 0.0f;
            last_animation = current_animation;
        }

        if (anim.looping)
        {
            if ((int)anim_tick >= anim.frame_count)
                anim_tick = 0.0f;
            current_frame = (int)anim_tick;
            anim_completed = false;
        }
        else
        {
            if ((int)anim_tick >= anim.frame_count)
                anim_tick = anim.frame_count;
            current_frame = (int)anim_tick;
            if (current_frame >= anim.frame_count)
                current_frame = anim.frame_count - 1;
            anim_completed = anim_tick == anim.frame_count;
        }

        anim_tick += Time.deltaTime * anim.speed_fps;

        if(last_frame != current_frame)
        {
            //loop through all actions the sprite can take at a frame if that frame is hit then perform said action
            for (int i = 0; i < anim.actions.Count; i++)
            {
                if (anim.actions[i].activation_frame == current_frame)
                    anim.actions[i].action.Invoke();
            }
        }

        last_frame = current_frame;
    }

    public virtual void ChasePlayer(GameObject target)
    {
        //if ready to pick a new direction
        if (step_count == 0)
        {
            //decide randomly how many steps to take towards the target
            step_count = (int)MathUtils.GaussianRandom(step_count_min, step_count_max);

            //create 8 movement directions based off from cardinals and orthogonals
            List<Vector2> possible_movements = new List<Vector2>() { new Vector2(-1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f),
                                                                     new Vector2(1.0f, 0.0f), new Vector2(1.0f, -1.0f), new Vector2(0.0f, -1.0f),
                                                                     new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 0.0f)
                                                                    };

            for (int i = 0; i < possible_movements.Count; i++)
                possible_movements[i].Normalize();

            //out of all of the possible movements, find which one brings the enemy closest to the player
            int closest_index = 0;
            float closest_distance = Mathf.Infinity;
            for (int i = 0; i < possible_movements.Count; i++)
            {
                Vector2 new_position = new Vector2(transform.position.x + possible_movements[i].x * step_distance * step_count, transform.position.z + possible_movements[i].y * step_distance * step_count);
                float distance = Vector3.Distance(target.transform.position, new Vector3(new_position.x, transform.position.y, new_position.y));
                if (distance < closest_distance)
                {
                    closest_distance = distance;
                    closest_index = i;
                }
            }

            Vector3 potential_lookdir = new Vector3(possible_movements[closest_index].x, 0.0f, possible_movements[closest_index].y);

            //if this direction is blocked, 1) check its orthogonals, 2) if both of those are blocked check if it's original direction is blocked, 3) if that is blocked then turn around
            //4) if that final direction is blocked, you're boned so set the step counter to 0

            //Not blocked, ignore everything
            if (CheckNextPostion(potential_lookdir))
            {
                lookdir = potential_lookdir;
                transform.position += lookdir * step_distance + Vector3.up * step_height;
                step_height = 0.0f;
                return;
            }
            //1) ortho
            //corner cases where orthogonals don't follow a standard calculation
            if (closest_index == 0)
            {
                //right ortho
                potential_lookdir = new Vector3(possible_movements[1].x, 0.0f, possible_movements[1].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance + Vector3.up * step_height;
                    step_height = 0.0f;
                    return;
                }

                //left ortho
                potential_lookdir = new Vector3(possible_movements[7].x, 0.0f, possible_movements[7].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance + Vector3.up * step_height;
                    step_height = 0.0f;
                    return;
                }
            }
            else if(closest_index == 7)
            {
                //right ortho
                potential_lookdir = new Vector3(possible_movements[0].x, 0.0f, possible_movements[0].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance + Vector3.up * step_height;
                    step_height = 0.0f;
                    return;
                }

                //left ortho
                potential_lookdir = new Vector3(possible_movements[6].x, 0.0f, possible_movements[6].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance + Vector3.up * step_height;
                    step_height = 0.0f;
                    return;
                }
            }
            else
            {
                //right ortho
                potential_lookdir = new Vector3(possible_movements[closest_index + 1].x, 0.0f, possible_movements[closest_index + 1].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance + Vector3.up * step_height;
                    step_height = 0.0f;
                    return;
                }

                //left ortho
                potential_lookdir = new Vector3(possible_movements[closest_index - 1].x, 0.0f, possible_movements[closest_index - 1].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance + Vector3.up * step_height;
                    step_height = 0.0f;
                    return;
                }
            }

            //2) current look direction
            if (CheckNextPostion(lookdir))
            {
                transform.position += lookdir * step_distance + Vector3.up * step_height;
                step_height = 0.0f;
                return;
            }

            //3) turn around!
            int opposite_index = closest_index + 4;
            if (opposite_index > 7) opposite_index -= 8;

            potential_lookdir = new Vector3(possible_movements[opposite_index].x, 0.0f, possible_movements[opposite_index].y);
            if (CheckNextPostion(potential_lookdir))
            {
                lookdir = potential_lookdir;
                transform.position += lookdir * step_distance + Vector3.up * step_height;
                step_height = 0.0f;
                return;
            }

            //4) you tried!
            step_count = 0;

        }
        //new direction is known
        else
        {
            //first see if the desired step location is valid, if it is then take a step, lower the step count and lower the targeting threshold
            if (CheckNextPostion(lookdir))
            {
                transform.position += lookdir * step_distance + Vector3.up * step_height;
                step_height = 0.0f;
                step_count--;
                targeting_threshold--;
                reaction_time--;
            }
            //if it isn't, then set the step count to 0
            else
            {
                step_count = 0;
            }
        }
    }

    public void TestAction()
    {
        Debug.Log($"Hey, how's it goin! The current frame is: {current_frame}!");
    }

    public virtual void AI()
    {
        //in this scenario attack the player at greater frequencies at close range, must have LOS
        if (step_count == 0 && reaction_time <= 0)
        {
            if (anim_completed)
            {
                current_animation = 0;
                ChasePlayer(target);
                return;
            }
            //calculate attack probability
            float max_magnitude = max_attack_distance - min_attack_distance;
            float attack_magnitude = Vector3.Distance(target.transform.position, transform.position) - min_attack_distance;
            float distance_lerp = attack_magnitude / max_magnitude;

            float attack_probability = Mathf.Lerp(100.0f, 0.0f, distance_lerp);
            if (MathUtils.GaussianRandom(0.0f, 100.0f) <= attack_probability)
            {
                current_animation = 1;
                lookdir = (target.transform.position - transform.position).normalized;
            }
            else if(current_animation != 1)
            {
                current_animation = 0;
                ChasePlayer(target);
                return;
            }
        }
        else
        {
            if (step_frequency > 0.0f)
                step_frequency -= Time.deltaTime;
            else
            {
                ChasePlayer(target);
                step_frequency = step_frequency_max;
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        targeting_threshold = 0;
        step_count = 0;
        anim_tick = 0.0f;
        step_frequency_max = step_frequency;
        max_reaction_time = reaction_time;
        step_height = 0.0f;
        lookdir = transform.forward;

        //create a new material instance so that other enemies are unaffected
        visual_mat = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        col = GetComponent<BoxCollider>();

        target = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimationViewAngle();
        Animate();
        AI();
    }
}

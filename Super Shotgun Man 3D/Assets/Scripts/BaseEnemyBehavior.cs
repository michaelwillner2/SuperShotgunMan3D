using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyBehavior : MonoBehaviour
{
    [System.Serializable]
    private struct Animation
    {
        public int starting_index;
        public int frame_count;
        public bool looping;
        public float speed_fps;
    }

    public int current_frame;
    public int current_animation;

    private int last_animation;

    public Vector3 lookdir;
    public Texture2DArray spritesheet;

    private float anim_tick;

    private Material visual_mat;
    [SerializeField]
    private List<Animation> animations;

    void UpdateAnimationViewAngle()
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

    void Animate()
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
        }
        else
        {
            if ((int)anim_tick >= anim.frame_count)
                anim_tick = anim.frame_count;
            current_frame = (int)anim_tick;
        }

        anim_tick += Time.deltaTime * anim.speed_fps;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        anim_tick = 0.0f;
        lookdir = transform.forward;

        //create a new material instance so that other enemies are unaffected
        visual_mat = transform.GetChild(0).GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimationViewAngle();
        Animate();
    }
}

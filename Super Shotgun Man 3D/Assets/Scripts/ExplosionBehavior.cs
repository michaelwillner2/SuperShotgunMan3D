using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehavior : MonoBehaviour
{
    public bool damaging;
    public int damage;
    public float effect_duration, max_radius;

    private float life_time;
    private SphereCollider col;
    private Material mat;

    private List<GameObject> already_damaged;

    IEnumerator VisualRoutine()
    {
        float delay_increment = effect_duration / 21.0f;
        for(int i=0; i<21; i++)
        {
            mat.SetFloat("_SpriteIndex", i);
            yield return new WaitForSeconds(delay_increment);
        }

        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<SphereCollider>();
        mat = GetComponent<Renderer>().material;
        already_damaged = new List<GameObject>();

        col.radius = 0.0f;
        life_time = 0.0f;

        StartCoroutine(VisualRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (damaging)
        {
            life_time += Time.deltaTime / effect_duration;
            life_time = Mathf.Clamp01(life_time);
            col.radius = Mathf.Lerp(0.0f, max_radius, life_time);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!damaging)
            return;
        if(already_damaged.Contains(other.gameObject))
            return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player") && other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            return;

        //do a raycast check to make sure the explosion has LOS to object caught in blast
        RaycastHit hit;
        Vector3 dir = other.transform.position - transform.position;
        Physics.Raycast(transform.position, dir.normalized, out hit, dir.magnitude);
        if (hit.collider == null)
            return;
        if (hit.collider.gameObject != other.gameObject)
            return;

        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            int calc_damage = (int)((max_radius / Vector3.Distance(other.gameObject.transform.position, transform.position)) * damage);
            stats.TakeDamage(calc_damage, dir.normalized);
            already_damaged.Add(stats.gameObject);
        }
    }
}

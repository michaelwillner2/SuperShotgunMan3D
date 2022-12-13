using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    private int hp, ap, shells;
    private float announce_text_fade, powerup_time, powerup_timer_max;
    [SerializeField]
    private float announce_text_fade_increment, knockback_resistance, damage_vignette_increment;
    private string announce_text;
    private Text hp_text, ap_text, announce_ui;
    private Image shell_ui, powerup_ui, damage_vignette;
    [SerializeField]
    private Sprite[] shell_ui_images;
    private Sprite powerup_sprite;
    private GameObject last_damage_dealer;

    public int HP
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp > 200)
                hp = 200;
            else if (hp < -100)
                hp = -100;

            int text_value = Mathf.Clamp(hp, 0, 200);
            hp_text.text = $"HP:\n{text_value}%";
        }
    }

    public int AP
    {
        get { return ap; }
        set
        {
            ap = value;
            if (ap > 100)
                ap = 100;
            else if (ap < 0)
                ap = 0;

            ap_text.text = $"AP:\n{ap}%";
        }
    }

    public int Shells
    {
        get { return shells; }
        set {
            shells = value;
            if (shells > 2)
                shells = 2;
            else if (shells < 0)
                shells = 0;

            shell_ui.sprite = shell_ui_images[2 - shells];
        }
    }

    public float PowerupTime
    {
        get { return powerup_time; }
        set
        {
            powerup_time = value;
            powerup_timer_max = powerup_time;
        }
    }

    public string AnnounceText
    {
        get { return announce_text; }
        set
        {
            announce_text = value;
            announce_ui.text = announce_text;
            announce_text_fade = 2.0f;
        }
    }

    public Sprite PowerUpSprite
    {
        get { return powerup_sprite; }
        set
        {
            powerup_sprite = value;
            powerup_ui.sprite = powerup_sprite;
            powerup_ui.transform.GetChild(0).GetComponent<Image>().sprite = powerup_sprite;
        }
    }

    public GameObject LastDamageDealer
    {
        get { return last_damage_dealer; }
        set
        {
            last_damage_dealer = value;
        }
    }

    public void TakeDamage(int amount, GameObject dealer = null)
    {
        int armor_reduction = 0;
        if (ap > 0)
            armor_reduction = (int)((float)amount * (1.0f / 3.0f));

        if(armor_reduction > 0)
        {
            int calc_armor_reduction = armor_reduction;
            for(int i=0; i<calc_armor_reduction; i++)
            {
                if (ap == 0)
                    armor_reduction--;
                else
                    AP--;
            }
        }

        HP = hp - amount + armor_reduction;
        if(hp > 0)
        damage_vignette.color = Color.Lerp(new Color(1.0f, 0.0f, 0.0f, 0.0f), Color.red, Mathf.Clamp01(amount / 100.0f));
        if (dealer != null && hp > 0) last_damage_dealer = dealer;
    }

    public void TakeDamage(int amount, Vector3 direction, GameObject dealer = null)
    {
        int armor_reduction = 0;
        if (ap > 0)
            armor_reduction = (int)((float)amount * (1.0f / 3.0f));

        if (armor_reduction > 0)
        {
            int calc_armor_reduction = armor_reduction;
            for (int i = 0; i < calc_armor_reduction; i++)
            {
                if (ap == 0)
                    armor_reduction--;
                else
                    AP--;
            }
        }

        HP = hp - amount + armor_reduction;
        GetComponent<Rigidbody>().AddForce(direction.normalized * ((float)amount / knockback_resistance));
        if(hp > 0)
        damage_vignette.color = Color.Lerp(new Color(1.0f, 0.0f, 0.0f, 0.0f), Color.red, Mathf.Clamp01(amount / 100.0f));
        if (dealer != null && hp > 0) last_damage_dealer = dealer;
    }

    void UpdateAnnounceText()
    {
        announce_ui.color = new Color(announce_ui.color.r, announce_ui.color.g, announce_ui.color.b, Mathf.Clamp01(announce_text_fade));
        if (announce_text_fade > 0.0f)
            announce_text_fade -= Time.deltaTime * announce_text_fade_increment;
        else
            announce_text_fade = 0.0f;
    }

    void UpdatePowerupTimer()
    {
        powerup_ui.transform.GetChild(0).GetComponent<Image>().fillAmount = powerup_time / powerup_timer_max;
        if (powerup_time > 0.0f)
        {
            powerup_ui.color = new Color(powerup_ui.color.r, powerup_ui.color.g, powerup_ui.color.b, .760784f);
            powerup_ui.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            powerup_time -= Time.deltaTime;
        }
        else
        {
            powerup_ui.color = new Color(powerup_ui.color.r, powerup_ui.color.g, powerup_ui.color.b, 0.0f);
            powerup_ui.transform.GetChild(0).GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            powerup_time = 0.0f;
        }
    }

    void UpdateDamageVignette()
    {
        if (damage_vignette.color.a == 0.0f) return;
        float progress = damage_vignette.color.a;
        if (progress > 0.0f)
            progress -= Time.deltaTime * damage_vignette_increment;
        else
            progress = 0.0f;
        damage_vignette.color = new Color(1.0f, 0.0f, 0.0f, progress);
    }

    private void Awake()
    {
        hp_text = transform.GetChild(1).GetChild(2).GetComponent<Text>();
        ap_text = transform.GetChild(1).GetChild(3).GetComponent<Text>();
        announce_ui = transform.GetChild(1).GetChild(6).GetComponent<Text>();
        shell_ui = transform.GetChild(1).GetChild(5).GetChild(0).GetComponent<Image>();
        powerup_ui = transform.GetChild(1).GetChild(7).GetComponent<Image>();
        damage_vignette = transform.GetChild(1).GetChild(8).GetComponent<Image>();

        HP = 100;
        AP = 0;
        Shells = 2;
        PowerupTime = 20.0f;

        announce_ui.color = new Color(announce_ui.color.r, announce_ui.color.g, announce_ui.color.b, 0.0f);
        damage_vignette.color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
    }

    private void Update()
    {
        UpdateAnnounceText();
        UpdatePowerupTimer();
        UpdateDamageVignette();
    }
}

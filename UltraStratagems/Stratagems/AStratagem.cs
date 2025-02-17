using System;
using System.Collections.Generic;
using System.Text;

namespace UltraStratagems.Stratagems;
public class AStratagem : MonoBehaviour
{
    public GameObject owner;
    public string name;
    public string description;
    public string icon;


    public float CallInTime;
    public float ElapsedCallInTime;

    public float Cooldown;
    public float ElapsedCooldownTime;    
    
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }

    public virtual void BeginAttack(Vector3 pos, Vector3 Dir) { }

}
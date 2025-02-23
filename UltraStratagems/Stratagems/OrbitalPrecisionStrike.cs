
namespace UltraStratagems.Stratagems;

class OrbitalPrecisionStrike : AStratagem
{
    // Req
    public GameObject owner;
    public string name = "Orbital Airburst Strike";
    public string description = "A projectile which explodes while airborne, creating a deadly rain of shrapnel. Not effective against heavy armor.";
    public Texture2D icon;

    float runTime = .1f +  3f; // fireTime aimTime
    float totalRunTime => runTime;

    // Runtime
    float StartTime;
    float endTime;

    bool callingIn = false;
    bool attacking = false;
    GameObject DeathRay;
    NewMovement nm => NewMovement.Instance;

    float fireTimeLeft = .025f;
    float aimTime = 3f;

    bool DeathRayInProgress = false;
    bool DeathRayAiming = false;
    bool deathRayFiring = false;

    public override void Start()
    {
        Stream iconStream = AssetStuff.GetEmbeddedAsset("Orbital_Airburst_Strike_Icon.png");
        icon = AssetStuff.StreamToTex(iconStream);

        if (icon == null)
        {
            Debug.LogError("Icon texture is null!");
            return;
        }

        print($"Set up: {this.GetType()}, Loaded icon: {icon.name}, {icon}");
        //displayTexture.texture = bytesToTexture2D(data);

    }
    public override void Update()
    {
        if (!attacking)
            return;

        if (Time.time > endTime && attacking)
        {
            Debug.Log($"Ending stratagem: {this.GetType()}");
            Complete();
            attacking = false;
            return;
        }

    }
    Vector3 deathRayOrign;
    public override void BeginAttack(Vector3 pos, Vector3 Dir)
    {
        StartTime = Time.time;
        endTime = StartTime + totalRunTime;
        callingIn = true;
        deathRayOrign = pos + new Vector3(0, 100, 0);
        DeathRay = Instantiate(AssetStuff.LoadAsset<GameObject>("stratagem beam.prefab"), pos += new Vector3(0, 100, 0), Quaternion.identity);
                
        Vector3 direction = (pos - DeathRay.transform.position).normalized;
        Quaternion newRot = Quaternion.LookRotation(direction, DeathRay.transform.up);
        DeathRay.transform.rotation = newRot;

        DeathRay.GetComponent<LineRenderer>().useWorldSpace = true;
        DeathRay.GetComponent<LineRenderer>().material.color = new(1, 0, 0, 0);

        DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = false;
        DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = false;
        DeathRay.GetComponent<ContinuousBeam>().damage = 0;
        LayerMask envMask = DeathRay.GetComponent<ContinuousBeam>().environmentMask;
        envMask = envMask | (1 << 10); // limb
        envMask = envMask | (1 << 12); // enemytrigger
        DeathRay.GetComponent<ContinuousBeam>().environmentMask = envMask;

        DeathRay.transform.Find("Ring (1)").GetComponent<Light>().color = Color.red;

        DeathRayAiming = true;
        attacking = true;

        StartCoroutine(DeathRayAimSequence());
    }

    Vector3? GetPoint()
    {
        Ray ray = new Ray(nm.cc.transform.position, nm.cc.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "Default")))
        {
            return hitInfo.point;
        }
        return null;
    }
    int DecoPartiles = 8;
    IEnumerator DeathRayAimSequence()
    {
        DeathRayInProgress = true;
        Vector3 targetPoint = Vector3.zero;
        float timeTillShoot = 0f;
        EnemyIdentifier? Target = null!;

        targetPoint = GetPoint() ?? Vector3.zero;

        Vector3 direction = (targetPoint - DeathRay.transform.position).normalized;
        Quaternion newRot = Quaternion.LookRotation(direction, DeathRay.transform.up);

        DeathRay.transform.rotation = newRot;
            
        DeathRayFindTarget(out Target, targetPoint);
        Vector3 pos = DeathRay.transform.Find("Ring (1)").position;
        
        

        float totalTime = aimTime;
        int instanceCount = 0;
        float fixedTimer = 0f;


        while (DeathRayAiming)
        {
            timeTillShoot += Time.deltaTime;
            if (Target == null || Target.dead)
            {
                DeathRayFindTarget(out Target, targetPoint);
            }

            DeathRayRotateTowards(Target?.gameObject.GetComponent<Collider>().bounds.center, 5f);

            DeathRay.GetComponent<LineRenderer>().material.color = new Color(1, 0, 0, timeTillShoot / aimTime);

            if (timeTillShoot > aimTime - (DecoPartiles * (Time.fixedDeltaTime)))
            {
                fixedTimer += Time.deltaTime;
                if (fixedTimer >= (Time.fixedDeltaTime / 2) && instanceCount < DecoPartiles)
                {
                    pos = DeathRay.transform.Find("Ring (1)").position;
                    fixedTimer -= Time.fixedDeltaTime;
                    float t = (float)instanceCount / (DecoPartiles - 1);
                    Vector3 instantiationPos = Vector3.Lerp(deathRayOrign, pos, t);

                    GameObject vfx = Instantiate(rocketLauncherFire, instantiationPos, DeathRay.transform.rotation);
                    float scale = Mathf.Lerp(1f, .3f, (float)instanceCount/DecoPartiles);
                    vfx.transform.localScale = new(scale, scale, scale);


                    instanceCount++;
                }

            }

            if (timeTillShoot > aimTime)
                DeathRayAiming = false;

            yield return null;
        }
        print($"Death ray aiming complete");
        //HudMessageReceiver.instance.SendHudMessage($"Death ray aiming complete", silent: true);
        


        deathRayFiring = true;
        DeathRayRotateTowards(Target?.gameObject.GetComponent<Collider>().bounds.center, 25f);

        DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = true;
        DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = true;
        DeathRay.GetComponent<LineRenderer>().material.color = Color.red;



        GameObject effect1 = Instantiate(dustExplosion, pos + new Vector3(0, 1f, 0), Quaternion.Euler(-90f,0, 0));
        GameObject effect2 = Instantiate(bulletSpark, pos, Quaternion.identity);
        Instantiate(lightningExplosion, pos, Quaternion.identity);


        effect1.transform.localScale = new(20f, 20f, 20f);
        effect2.transform.localScale = new(5f, 5f, 5f);

        float maxRotateSpeed = 30f;

        while (fireTimeLeft > 0)
        {
            fireTimeLeft -= Time.deltaTime;

            if (Target == null || Target.dead)
            {
                DeathRayFindTarget(out Target, targetPoint);
                maxRotateSpeed = 6f;
            }
            
            DeathRayRotateTowards(Target?.gameObject.GetComponent<Collider>().bounds.center, maxRotateSpeed);

            DeathRay.GetComponent<ContinuousBeam>().damage = 5000;
            
            yield return null;
        }
        DeathRay.GetComponent<ContinuousBeam>().damage = 0;
        deathRayFiring = false;
        DeathRayInProgress = false;
        Complete();
    }


    void DeathRayRotateTowards(Vector3? Point, float speed)
    {
        try
        {
            if (Point == null)
                return;

            Vector3 direction = (Point - DeathRay.transform.position).Value.normalized;
            Quaternion newRot = Quaternion.LookRotation(direction, DeathRay.transform.up);
            DeathRay.transform.rotation = Quaternion.Slerp(DeathRay.transform.rotation, newRot, speed * Time.deltaTime);
        }
        catch (NullReferenceException e)
        {
            print("Can no longer rotate towards target");
        }
    }

    void DeathRayFindTarget(out EnemyIdentifier Target, Vector3 targetPoint)
    {
        float radius = 50f;
        Target = null!;
        try
        {
            RaycastHit[] hits = Physics.SphereCastAll(new Ray(targetPoint, Vector3.up), radius, radius, LayerMask.GetMask("EnemyTrigger"));
            if (hits == null || hits.Length < 1)
            {
                Debug.Log("No targets found in SphereCastAll");
                return;
            }

            List<EnemyIdentifier> identifiers = new List<EnemyIdentifier>();
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != null)
                {
                    var identifier = hit.collider.gameObject.GetComponent<EnemyIdentifier>();
                    if (identifier != null)
                    {
                        identifiers.Add(identifier);
                    }
                }
            }

            Debug.Log($"Targets in area: {identifiers.Count}");

            if (identifiers.Count == 0)
            {
                Debug.Log("No valid enemy identifiers found");
                return;
            }

            Target = identifiers[0];
            if (Target == null)
            {
                Debug.Log("First target is null");
                return;
            }

            float closestDistance = Vector3.Distance(targetPoint, Target.transform.position);
            foreach (var item in identifiers)
            {
                if (item == null || !EnemyStrengthRanks.ContainsKey(item.enemyType))
                {
                    continue;
                }

                float distance = Vector3.Distance(targetPoint, item.transform.position);
                if (!EnemyStrengthRanks.ContainsKey(Target.enemyType) ||
                    EnemyStrengthRanks[item.enemyType] > EnemyStrengthRanks[Target.enemyType] ||
                    (EnemyStrengthRanks[item.enemyType] == EnemyStrengthRanks[Target.enemyType] && distance < closestDistance))
                {
                    Target = item;
                    closestDistance = distance;
                }
            }

            if (Target != null && Target.gameObject != null)
            {
                Debug.Log($"Target Found: {Target.gameObject.name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in DeathRayFindTarget: {e.Message}\n{e.StackTrace}");
            Target = null!;
        }
    }

    void Complete()
    {
        print("Destorying death ray");
        //HudMessageReceiver.instance.SendHudMessage($"Death ray fin", silent: true);
        StopAllCoroutines();
        Destroy(owner);
        Destroy(DeathRay);
        Destroy(gameObject);
    }
}

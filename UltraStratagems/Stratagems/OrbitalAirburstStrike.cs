
namespace UltraStratagems.Stratagems;

public class OrbitalAirburstStrike : AStratagem
{
    // Req
    public GameObject owner;
    public string name = "Orbital Airburst Strike";
    public string description = "A projectile which explodes while airborne, creating a deadly rain of shrapnel. Not effective against heavy armor.";
    public Texture2D icon;

    float callInDelay = 3;
    float runTime = 12;
    float totalRunTime => callInDelay + runTime;

    int shrapnelCount = 13;
    int shrapnelBursts = 4;
    float coneAngle = 15f;
    //float rocketDelay = 0.1f;

    float rocketDelay => runTime / shrapnelBursts;

    // Runtime
    float StartTime;
    float endTime;

    bool callingIn = false;
    bool attacking = false;
    float rocketCountdown = 0f;

    public OrbitalAirburstStrike()
    {
        
    }

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
        //if (!attacking)
        //    return;

        if (Time.time > endTime && attacking)
        {
            Debug.Log($"Ending stratagem: {this.GetType()}"); 
            attacking = false;
            Complete();
            return;
        }
    }

    NewMovement nm => NewMovement.instance;

    Vector3? GetPoint()
    {
        Ray ray = new Ray(nm.cc.transform.position, nm.cc.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "Default")))
        {
            return hitInfo.point;
        }
        return null;
    }


    float minDistanceFromTarget = 10f;
    IEnumerator AirburstSequence()
    {
        //GameObject effect1 = Instantiate(dustBig, pos, Quaternion.identity);
        //GameObject effect2 = Instantiate(bulletSpark, pos, Quaternion.identity);
        Vector3 targetPoint = GetPoint() ?? Vector3.zero;
        Vector3 SpawnPoint = targetPoint += new Vector3(0f, 60f, 0f);
        Vector3 shipRocketSpawn = new(100f, 150f, 100f);

        //Rocket spawns from ship towards the spawnPoint, then explodes releasing the shrapnel
        Vector3 ang =  SpawnPoint - shipRocketSpawn;
        ang.Normalize();
        Quaternion angle = Quaternion.LookRotation(ang);

        float distance = Vector3.Distance(SpawnPoint, shipRocketSpawn);
        distance -= minDistanceFromTarget;
        float rocketJetlagTime = distance / rocket.GetComponent<Grenade>().rocketSpeed;
        print($"rocket delay: {rocketJetlagTime}");
        
        for (int i = 0; i < shrapnelBursts; i++)
        {
            
            while (rocketCountdown > 0)
            {
                rocketCountdown -= Time.deltaTime;

                yield return null;
            }
            rocketCountdown += rocketDelay;

            StartCoroutine(DoRockets(shipRocketSpawn, angle, SpawnPoint));
        }

        yield return null;
    }

    IEnumerator DoRockets(Vector3 shipRocketSpawn, Quaternion angle, Vector3 SpawnPoint)
    {

        _Stopwatch timewatch = new();
        GameObject orbital = Instantiate(rocket, shipRocketSpawn, angle);
        timewatch.Start();

        while (orbital != null && (Vector3.Distance(orbital.transform.position, SpawnPoint) > minDistanceFromTarget))
        {
            yield return new WaitForFixedUpdate();
        }

        if (orbital != null)
        {
            orbital.GetComponent<Grenade>().Explode(big: true);
            timewatch.Stop();
            print($"It took: {timewatch.Elapsed.TotalSeconds.ToString("F5")} seconds to arrive");
            DoShrapnel(SpawnPoint);
        }
    }

    void DoShrapnel(Vector3 SpawnPoint)
    {
        // Shrapnel
        List<Vector3> shrapnelNormals = PickShrapnelLocation(SpawnPoint);
        foreach (Vector3 normal in shrapnelNormals)
        {

            Quaternion rotation = Quaternion.LookRotation(normal);
            rotation.eulerAngles += new Vector3(90f, 0f, 0f);

            GameObject obj = Instantiate(rocket, SpawnPoint, rotation);
            //obj.transform.position += obj.transform.forward * 0.5f;

            //Grenade gren = obj.GetComponent<Grenade>();
            //gren.harmlessExplosion


            Explosion exp = obj.GetComponent<Grenade>().harmlessExplosion.transform.Find("Sphere_8 (1)").GetComponent<Explosion>();
            exp.friendlyFire = true;
            exp.canHit = AffectedSubjects.All;
            exp.enemyDamageMultiplier = 1.2f;
            exp.damage = 35;
            exp.friendlyFire = true;
            exp.halved = false;
            //obj.GetComponent<Grenade>().




            //obj.transform.localScale = new(0.2f, 2f, 0.2f);
            Shrapnel.Add(obj);
        }

    }

    List<GameObject> Shrapnel = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="orign">Where the shrapnel will origninate from</param>
    /// <returns>List of the shrapnel facing normal</returns>
    public List<Vector3> PickShrapnelLocation(Vector3 orign) 
    {
        List<Vector3> output = new();
        for (int i = 0; i < shrapnelCount; i++)
        {
            float angle = Random.Range(coneAngle, -coneAngle);
            float radialAngle = Random.Range(0f, 360f);


            Vector3 outNormal = new Vector3(
                Mathf.Cos(radialAngle * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                Mathf.Sin(radialAngle * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad)
            );


            output.Add(outNormal);
        }


        return output;
    }

    public override void BeginAttack(Vector3 pos, Vector3 Dir)
    {
        StartTime = Time.time;
        endTime = StartTime + totalRunTime;
        callingIn = true;

        StartCoroutine(AirburstSequence());
    }



    void Complete()
    {
        print("Destorying death ray");
        //HudMessageReceiver.instance.SendHudMessage($"Airburst fin", silent: true);
        StopAllCoroutines();
        Destroy(owner);
        //Destroy(DeathRay);
        Destroy(gameObject);
    }
}

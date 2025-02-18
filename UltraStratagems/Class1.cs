global using System;
global using System.IO;
global using System.Reflection;
global using System.Collections.Generic;

global using HarmonyLib;
global using BepInEx;
global using UnityEngine;
global using UnityEngine.AddressableAssets;
global using UnityEngine.UI;
global using Object = UnityEngine.Object;
global using UnityEngine.SceneManagement;

global using UltraStratagems.Stratagems;
global using static UltraStratagems.Class1;
global using static UltraStratagems.AssetStuff;
global using UnityEngine.Assertions;
global using System.Collections;
global using System.Linq;

namespace UltraStratagems;



[BepInPlugin("UltraStratagems", "UltraStratagems", "1.0.0")]
public partial class Class1 : BaseUnityPlugin
{
    public static Class1 instance;
    public Dictionary<string, Object> loadedAssets = new();

    public AssetBundle assets;

    public StratagemManager stratagemManager = new();
    public AssetStuff assetStuff = new();

    public HashSet<Action<string>> OnSceneLoaded = new();

    void Awake()
{
        instance = this;

        //SceneManager.sceneLoaded += (s, l) =>
        //{
        //    foreach (var item in OnSceneLoaded)
        //    {
        //        item.Invoke(s.name);
        //    }
        //};

        stratagemManager.Start();

        Stream checksum = AssetStuff.GetEmbeddedAsset("checksum.txt");
        using (StreamReader reader = new StreamReader(checksum))
        {
            if (reader.ReadToEnd() != "Test1234")
            {
                Debug.LogError($"Checksum failed, asset loading isnt working, {reader.ReadToEnd()}");
            }
        }
        

        AssetStuff.AssetBundleBs();

        bombPod = Addressables.LoadAssetAsync<GameObject>("Assets/Models/Objects/Bomb/BombPod_4.fbx").WaitForCompletion();
        BombMat = Addressables.LoadAssetAsync<Material>("Assets/Models/Objects/Bomb/Bomb.mat").WaitForCompletion();
        GameObject harmlessExplosion = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Rocket Harmless.prefab").WaitForCompletion();
        GameObject dustBig = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/DustBigEnemy.prefab").WaitForCompletion();
        GameObject bulletSpark = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/BulletSpark.prefab").WaitForCompletion();


        print($"BP: {bombPod}, Mat: {BombMat},{BombMat.color}");
    }

    GameObject bombPod;
    Material BombMat;

    NewMovement nm => NewMovement.Instance;

    Vector3 TargetPos;
    GameObject DeathRay;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Ray ray = new Ray(nm.cc.gameObject.transform.position, nm.cc.gameObject.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "Default")))
            {
                print($"Ray hit: {hitInfo.collider}, pos: {hitInfo.point}, {hitInfo.collider.name}");

                GameObject marker = Instantiate(bombPod, hitInfo.point, nm.cc.transform.rotation);

                Vector3 AttackDir = nm.cc.transform.right; 
                Vector3 AttackPos = hitInfo.point;

                //stratagemManager.ActivateStratagem<OrbitalAirburstStrike>(AttackPos, AttackDir);

                print($"obj: {marker}, {marker.name}, {marker.scene}");
            }
            else
            {
                print("Ray didnt hit");
            }

        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Ray ray = new Ray(nm.cc.transform.position, nm.cc.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "Default")))
            {
                print($"Ray hit: {hitInfo.collider}, pos: {hitInfo.point}, {hitInfo.collider.name}, lay: {hitInfo.collider.gameObject.layer}");

                DeathRay = Instantiate(AssetStuff.LoadAsset<GameObject>("assets/__stratagems/stratagem beam.prefab"), hitInfo.point, nm.cc.transform.rotation);
                DeathRay.GetComponent<LineRenderer>().useWorldSpace = true;
                DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = false;
                DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = false;
                DeathRay.GetComponent<ContinuousBeam>().damage = 0;
            }
        }



        if (DeathRay != null)
        {
            if (!DeathRayInProgress)
            {
                DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = false;
                DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = false;
                DeathRay.GetComponent<LineRenderer>().material.color = new(1, 0, 0, 0);
                DeathRay.GetComponent<ContinuousBeam>().damage = 0;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                StopCoroutine(DeathRayAimSequence());

                DeathRayInProgress = false;
                DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = false;
                DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = false;
                DeathRay.GetComponent<LineRenderer>().material.color = new(1, 0, 0, 0);
                DeathRay.GetComponent<ContinuousBeam>().damage = 0;
                DeathRayAiming = true;
                deathRayFiring = false;

                StartCoroutine(DeathRayAimSequence()); 
            }
        }

    }
    bool DeathRayInProgress = false;
    bool DeathRayAiming = false;
    bool deathRayFiring = false;
    IEnumerator DeathRayAimSequence()
    {
        DeathRayInProgress = true;
        Vector3 targetPoint = Vector3.zero;
        float timeTillShoot = 0f;
        EnemyIdentifier Target = null!;
        Ray ray = new Ray(nm.cc.transform.position, nm.cc.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "Default")))
        {
            targetPoint = hitInfo.point;
        }
        Vector3 direction = (targetPoint - DeathRay.transform.position).normalized;
        Quaternion newRot = Quaternion.LookRotation(direction, DeathRay.transform.up);

        DeathRay.transform.rotation = newRot;


        DeathRayFindTarget(out Target, targetPoint);

        while (DeathRayAiming)
        {
            timeTillShoot += Time.deltaTime;
            if (Target == null)
            {
                DeathRayFindTarget(out Target, targetPoint);
            }

            DeathRayRotateTowards(Target.gameObject.GetComponent<Collider>().bounds.center, 5f);

            DeathRay.GetComponent<LineRenderer>().material.color = new Color(1, 0, 0, timeTillShoot);

            if (timeTillShoot > 1)
                DeathRayAiming = false;

            yield return null;
        }
          
        print($"Death ray shot fired");
        float fireTimeLeft = 5f;

        deathRayFiring = true;
        DeathRayRotateTowards(Target.gameObject.GetComponent<Collider>().bounds.center, 25f);

        DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = true;
        DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = true;
        DeathRay.GetComponent<LineRenderer>().material.color = Color.red;

        while (deathRayFiring)
        {
            DeathRayRotateTowards(Target.gameObject.GetComponent<Collider>().bounds.center, 500f);
            DeathRay.GetComponent<ContinuousBeam>().damage = 1;

            fireTimeLeft -= Time.deltaTime;

            if (fireTimeLeft < 0)
                deathRayFiring = false;

            yield return null;
        }
        DeathRayInProgress = false;
    }


    void DeathRayRotateTowards(Vector3 Point, float speed)
    {
        Vector3 direction = (Point - DeathRay.transform.position).normalized;
        Quaternion newRot = Quaternion.LookRotation(direction, DeathRay.transform.up);
        //print($"Rotating Death ray: {newRot}");
        DeathRay.transform.rotation = Quaternion.Slerp(DeathRay.transform.rotation, newRot, speed * Time.deltaTime);
    }

    void DeathRayFindTarget(out EnemyIdentifier Target, Vector3 targetPoint)
    {
        float radius = 50f;
        Target = null!;
        
        try
        {
            RaycastHit[] hits = Physics.SphereCastAll(new Ray(targetPoint, Vector3.up), radius, radius, LayerMask.GetMask("EnemyTrigger"));
            if (hits.Length < 1)
            {
                Debug.LogWarning("Spherecast got nothin");
                return;
            }
            List<EnemyIdentifier> identifiers = hits.Select(_ => _.collider.gameObject.GetComponent<EnemyIdentifier>()).ToList();
            print($"Targets in area: {identifiers.Count}");
            Target = identifiers[0];

            foreach (var item in identifiers)
            {
                if (EnemyStrengthRanks[item.enemyType] > EnemyStrengthRanks[item.enemyType])
                {
                    Target = item;
                }
            }

        }
        catch (NullReferenceException e)
        {
            Debug.LogError($"Null ref: {e.Message}, {e.StackTrace}, {e.Source}, {e.HResult}");
            DeathRayAiming = false;
        }

        print($"Target Found: {Target.gameObject.name}");

    }

    public static Dictionary<EnemyType, int> EnemyStrengthRanks = new Dictionary<EnemyType, int>
    {
        { EnemyType.Filth, 1 },
        { EnemyType.Stray, 2 },
        { EnemyType.Schism, 3 },
        { EnemyType.Drone, 4 },
        { EnemyType.Turret, 5 },
        { EnemyType.Streetcleaner, 6 },
        { EnemyType.MaliciousFace, 7 },
        { EnemyType.Soldier, 8 },
        { EnemyType.Cerberus, 9 },
        { EnemyType.Swordsmachine, 10 },
        { EnemyType.Stalker, 11 },
        { EnemyType.HideousMass, 12 },
        { EnemyType.Mindflayer, 13 },
        { EnemyType.Virtue, 14 },
        { EnemyType.CancerousRodent, 15 },
        { EnemyType.VeryCancerousRodent, 16 },
        { EnemyType.Idol, 17 },
        { EnemyType.Mannequin, 18 },
        { EnemyType.Mandalore, 19 },
        { EnemyType.Gutterman, 20 },
        { EnemyType.Guttertank, 21 },
        { EnemyType.Minotaur, 22 },
        { EnemyType.Centaur, 23 },
        { EnemyType.Puppet, 24 },
        { EnemyType.BigJohnator, 25 },
        { EnemyType.Wicked, 26 },
        { EnemyType.Ferryman, 27 },
        { EnemyType.Leviathan, 28 },
        { EnemyType.FleshPanopticon, 29 },
        { EnemyType.FleshPrison, 30 },
        { EnemyType.V2, 31 },
        { EnemyType.V2Second, 32 },
        { EnemyType.Gabriel, 33 },
        { EnemyType.GabrielSecond, 34 },
        { EnemyType.Minos, 35 },
        { EnemyType.MinosPrime, 36 },
        { EnemyType.Sisyphus, 37 },
        { EnemyType.SisyphusPrime, 38 }
    };
}
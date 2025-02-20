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
global using Random = UnityEngine.Random;
global using _Stopwatch = System.Diagnostics.Stopwatch;

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
        /*
        Stream checksum = AssetStuff.GetEmbeddedAsset("checksum.txt");
        using (StreamReader reader = new StreamReader(checksum))
        {
            if (reader.ReadToEnd() != "Test1234")
            {
                Debug.LogError($"Checksum failed, asset loading isnt working, {reader.ReadToEnd()}");
            }
        }
        */
        AssetStuff.AssetBundleBs();


        bombPod = Addressables.LoadAssetAsync<GameObject>("Assets/Models/Objects/Bomb/BombPod_4.fbx").WaitForCompletion();
        BombMat = Addressables.LoadAssetAsync<Material>("Assets/Models/Objects/Bomb/Bomb.mat").WaitForCompletion();
        harmlessExplosion = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Rocket Harmless.prefab").WaitForCompletion();
        dustBig = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/DustBigEnemy.prefab").WaitForCompletion();
        bulletSpark = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/BulletSpark.prefab").WaitForCompletion();
        lazerHit = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/LaserHitParticle.prefab").WaitForCompletion();
        rocket = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Rocket.prefab").WaitForCompletion();
        shockwave = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/PhysicalShockwaveHarmless.prefab").WaitForCompletion();
        explosion = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Harmless.prefab").WaitForCompletion();
        lightningExplosion = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Lightning - No Lightning.prefab").WaitForCompletion();
        explosionRocketHarmless = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Rocket Harmless.prefab").WaitForCompletion();
        rocketLauncherFire = Addressables.LoadAssetAsync<GameObject>("Assets/Particles/RocketLauncherFire.prefab").WaitForCompletion();
        rocketLauncherFire.transform.Find("Point Light").GetComponent<Light>().enabled = false;

        Explosion lightningExp = lightningExplosion.transform.Find("Sphere_8").GetComponent<Explosion>();
        lightningExp.canHit = AffectedSubjects.PlayerOnly;
        lightningExp.damage = 0;
        lightningExp.electric = false;
        lightningExp.ignite = false;
        lightningExp.harmless = true;
        lightningExp.enemyDamageMultiplier = 0;
        lightningExp.maxSize = 5f;
        lightningExp.speed = 15f;


        lightningExplosion.transform.localScale = new(1.25f, 1.25f, 1.25f);
    }


    public static GameObject lazerHit;
    public static GameObject rocketLauncherFire;
    public static GameObject explosionRocketHarmless;
    public static GameObject lightningExplosion;
    public static GameObject explosion;
    public static GameObject shockwave; 
    public static GameObject harmlessExplosion;
    public static GameObject dustBig;
    public static GameObject bulletSpark;
    public static GameObject rocket;
    GameObject bombPod;
    Material BombMat;

    NewMovement nm => NewMovement.Instance;

    Vector3 TargetPos;
    GameObject DeathRay;

    public void Update()
    {
        /*
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
        */
        if (Input.GetKeyDown(KeyCode.L))
        {
            Ray ray = new Ray(nm.cc.transform.position, nm.cc.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "Default")))
            {
                print($"Ray hit: {hitInfo.collider}, pos: {hitInfo.point}, {hitInfo.collider.name}, lay: {hitInfo.collider.gameObject.layer}");

                //DeathRay = Instantiate(AssetStuff.LoadAsset<GameObject>("assets/__stratagems/stratagem beam.prefab"), hitInfo.point, nm.cc.transform.rotation);
                //DeathRay.GetComponent<LineRenderer>().useWorldSpace = true;
                //DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = false;
                //DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = false;
                //DeathRay.GetComponent<ContinuousBeam>().damage = 0;

                Vector3 pos = hitInfo.point;

                //GameObject effect1 = Instantiate(dustBig, pos, Quaternion.identity);
                //GameObject effect2 = Instantiate(bulletSpark, pos, Quaternion.identity);
                //GameObject effect3 = Instantiate(harmlessExplosion, pos, Quaternion.identity);



                stratagemManager.ActivateStratagem<OrbitalPrecisionStrike>(hitInfo.point, Vector3.zero);

            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Ray ray = new Ray(nm.cc.transform.position, nm.cc.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "Default")))
            {
                print($"Ray hit: {hitInfo.collider}, pos: {hitInfo.point}, {hitInfo.collider.name}, lay: {hitInfo.collider.gameObject.layer}");


                stratagemManager.ActivateStratagem<OrbitalAirburstStrike>(hitInfo.point, Vector3.zero);
            }
        }
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
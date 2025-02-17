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
using UnityEngine.Assertions;

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

        SceneManager.sceneLoaded += (s, l) =>
        {
            foreach (var item in OnSceneLoaded)
            {
                item.Invoke(s.name);
            }
        };

        stratagemManager.Start();

        Stream checksum = AssetStuff.GetEmbeddedAsset("checksum.txt");
        using (StreamReader reader = new StreamReader(checksum))
        {
            if (reader.ReadToEnd() != "Test1234")
            {
                Debug.LogError($"Checksum failed, asset loading isnt working, {reader.ReadToEnd()}");
            }
        }

        //Bomb pod:
        /*
        Assets/Asset_Bundles/Models/BombPod_4.prefab
          Pod_BombPart.002, Assets/Asset_Bundles/Models/Bomb.mat
          StreetCleanerPod_Door,  Assets/Asset_Bundles/Models/Bomb.mat
          StreetCleaner_PodModel, Assets/Asset_Bundles/Models/Bomb.mat 
        */

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

                //marker.name = "Missile";
                //marker.transform.Find("Pod_BombPart.002").GetComponent<Renderer>().material = BombMat;
                //marker.transform.Find("StreetCleanerPod_Door").GetComponent<Renderer>().material = BombMat;
                //marker.transform.Find("StreetCleaner_PodModel").GetComponent<Renderer>().material = BombMat;

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
                DeathRay.GetComponent<ContinuousBeam>().damage = 500;
            }
        }    

        if (DeathRay != null)// && Input.GetKeyDown(KeyCode.P))
        {
            Ray ray = new Ray(nm.cc.transform.position, nm.cc.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Environment", "Outdoors", "EnemyTrigger")))
            {
                Vector3 direction = (hitInfo.point - DeathRay.transform.position).normalized;
                Quaternion newRot = Quaternion.LookRotation(direction, DeathRay.transform.up);
                print($"Ray hit: {hitInfo.collider}, pos: {hitInfo.point}, {hitInfo.collider.name}, lay: {hitInfo.collider.gameObject.layer}");
                
                print($"NewRot: {newRot}");
                DeathRay.transform.rotation = newRot;
            } 
            else
            {
                print("Hit nothin");
            }
        }

        DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = false;
        DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = false;
        
        if (DeathRay != null && Input.GetKeyDown(KeyCode.P))
        {
            DeathRay.GetComponent<ContinuousBeam>().canHitEnemy = true;
            DeathRay.GetComponent<ContinuousBeam>().canHitPlayer = true;


        }
    }

}
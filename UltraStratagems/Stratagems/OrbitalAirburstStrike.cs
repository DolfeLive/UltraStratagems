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
    




    // Runtime
    float StartTime;
    float endTime;

    bool callingIn = false;
    bool attacking = false;


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
        if (Time.time > endTime && attacking)
        {
            Debug.Log($"Ending stratagem: {this.GetType()}");
            attacking = false;
            return;
        }


    }
    
    public override void BeginAttack(Vector3 pos, Vector3 Dir)
    {
        StartTime = Time.time;
        endTime = StartTime + totalRunTime;
        callingIn = true;
    }
}

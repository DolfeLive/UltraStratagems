using static UnityEngine.UI.GridLayoutGroup;

namespace UltraStratagems;
public class StratagemManager
{
    public List<AStratagem> stratagems = new();
    public List<Type> equippedStratagems = new();
    public List<AStratagem> UsingStratagems = new();
    
    public void Start()
    {
        Debug.Log("StratagemManager Inited");
        equippedStratagems.Add(typeof(OrbitalAirburstStrike));
        equippedStratagems.Add(typeof(OrbitalPrecisionStrike));
    }

    public void ActivateStratagem<T>(Vector3 position, Vector3 direction) where T : AStratagem
    {
        AStratagem? stratagem = equippedStratagems.Find(_ => _ == typeof(T)) as T;

        if (stratagem == null)
        {
            Debug.LogWarning("SelectedStratagem is not in equippedStratagems");
            ///return;
        }

        UsingStratagems.Add(stratagem);

        GameObject owner = new GameObject(typeof(T).Name);
        owner.AddComponent<DestroyOnCheckpointRestart>();
        T stratagemComponent = owner.AddComponent<T>();
        stratagemComponent.owner = owner;
        stratagemComponent.BeginAttack(position, direction);
    }
}
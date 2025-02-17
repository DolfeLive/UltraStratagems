using static UnityEngine.UI.GridLayoutGroup;

namespace UltraStratagems;
public class StratagemManager
{
    public List<AStratagem> stratagems = new();
    public List<AStratagem> equippedStratagems = new();
    public List<AStratagem> UsingStratagems = new();

    public void Start()
    {
        Debug.Log("StratagemManager Inited");
    }

    public void ActivateStratagem<T>(Vector3 position, Vector3 direction) where T : AStratagem
    {
        AStratagem stratagem = equippedStratagems.Find(s => s.GetType() == typeof(T));

        if (stratagem == null)
        {
            Debug.LogWarning("SelectedStratagem is not in equippedStratagems");
        }

        UsingStratagems.Add(stratagem);

        GameObject owner = new GameObject(typeof(T).Name);
        T stratagemComponent = owner.AddComponent<T>();
        stratagemComponent.owner = owner;
        stratagemComponent.BeginAttack(position, direction);
    }
}
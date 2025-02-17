namespace UltraStratagems;

public class StratBullet : MonoBehaviour
{
    public ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    public GameObject toSpawn;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Rigidbody rb = other.GetComponent<Rigidbody>();

        part.GetCollisionEvents(other, collisionEvents);
        if (collisionEvents.Count > 0)
        {
            if (toSpawn != null)
                Instantiate(toSpawn, collisionEvents[0].intersection, Quaternion.LookRotation(collisionEvents[0].normal)).SetActive(value: true);
        }

        if (rb)
        {
            Vector3 pos = collisionEvents[0].intersection;
            Vector3 force = collisionEvents[0].velocity * 100;
            rb.AddForce(force);
        }
    }
}

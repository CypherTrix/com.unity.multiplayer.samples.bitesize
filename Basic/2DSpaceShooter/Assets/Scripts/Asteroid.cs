using CodeMonkey.HealthSystem.Scripts;
using CodeMonkey.HealthSystemCM;
using DamageNumbersPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class Asteroid : NetworkBehaviour, IDamageable, ICollidable {
    static string s_ObjectPoolTag = "ObjectPool";

    public static int numAsteroids = 0;
    private readonly float collisionDamage = 5f;
    public float CollisionDamage => collisionDamage;

    NetworkObjectPool m_ObjectPool;

    public NetworkVariable<int> Size = new(4);

    public NetworkVariable<HealthSystem> healthSystem = new(new HealthSystem(100));

    [SerializeField]
    private int m_NumCreates = 3;

    [HideInInspector]
    public GameObject asteroidPrefab;
    [SerializeField] DamageNumber damageNumber;


    void Awake() {
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");
    }

    // Use this for initialization
    void Start() {
        numAsteroids += 1;
        healthSystem.Value = new(Size.Value * Bullet.BULLET_DAMAGE);
        healthSystem.Value.OnDead += Explode;
    }



    public override void OnNetworkSpawn() {
        var size = Size.Value;
        transform.localScale = new Vector3(size, size, size);
    }

    public void Explode(object sender, System.EventArgs e) {
        if (NetworkObject.IsSpawned == false) {
            return;
        }
        Assert.IsTrue(NetworkManager.IsServer);

        numAsteroids -= 1;


        var newSize = Size.Value - 1;

        if (newSize > 0) {
            int num = Random.Range(1, m_NumCreates + 1);

            for (int i = 0; i < num; i++) {
                int dx = Random.Range(0, 4) - 2;
                int dy = Random.Range(0, 4) - 2;
                Vector3 diff = new(dx * 0.3f, dy * 0.3f, 0);

                var go = m_ObjectPool.GetNetworkObject(asteroidPrefab, transform.position + diff, Quaternion.identity);

                var asteroid = go.GetComponent<Asteroid>();
                asteroid.Size.Value = newSize;
                asteroid.asteroidPrefab = asteroidPrefab;
                go.GetComponent<NetworkObject>().Spawn();
                go.GetComponent<Rigidbody2D>().AddForce(diff * 10, ForceMode2D.Impulse);
            }
        }

        healthSystem.Value.OnDead -= Explode;
        NetworkObject.Despawn(true);
    }

    public void Damage(float amount) {
        healthSystem.Value.Damage(amount);
        damageNumber?.Spawn(transform.position, amount);
    }
}

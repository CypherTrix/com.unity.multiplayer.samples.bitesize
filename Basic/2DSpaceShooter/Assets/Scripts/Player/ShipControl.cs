using CodeMonkey.HealthSystem.Scripts;
using DamageNumbersPro;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class Buff {
    public enum BuffType {
        Speed,
        Rotate,
        Triple,
        Double,
        Health,
        Energy,
        QuadDamage,
        Bounce,
        Last
    };

    public static Color[] buffColors = { Color.red, new Color(0.5f, 0.3f, 1), Color.cyan, Color.yellow, Color.green, Color.magenta, new Color(1, 0.5f, 0), new Color(0, 1, 0.5f) };

    public static Color GetColor(BuffType bt) {
        return buffColors[(int)bt];
    }
};

public class ShipControl : NetworkBehaviour, IDamageable {
    static string s_ObjectPoolTag = "ObjectPool";

    NetworkObjectPool m_ObjectPool;

    public GameObject BulletPrefab;

    [SerializeField] private DamageNumber damageNumbersHealth;

    [SerializeField] private GameObject shootingPoint;

    public AudioSource fireSound;

    float m_RotateSpeed = 200f;

    float m_Acceleration = 12f;

    float m_BulletLifetime = 2;

    float m_TopSpeed = 7.0f;

    public NetworkVariable<int> Health = new(100);

    public NetworkVariable<int> Energy = new(100);

    public NetworkVariable<float> SpeedBuffTimer = new(0f);

    public NetworkVariable<float> RotateBuffTimer = new(0f);

    public NetworkVariable<float> TripleShotTimer = new(0f);

    public NetworkVariable<float> DoubleShotTimer = new(0f);

    public NetworkVariable<float> QuadDamageTimer = new(0f);

    public NetworkVariable<float> BounceTimer = new(0f);

    public NetworkVariable<Color> LatestShipColor = new();

    float m_EnergyTimer = 0;

    bool m_IsBuffed;

    private NetworkVariable<FixedString32Bytes> playerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    ParticleSystem m_Friction;

    [SerializeField]
    ParticleSystem m_Thrust;

    [SerializeField]
    SpriteRenderer m_ShipGlow;

    [SerializeField]
    Color m_ShipGlowDefaultColor;

    [SerializeField]
    UIDocument m_UIDocument;

    VisualElement m_RootVisualElement;

    ProgressBar m_HealthBar;

    ProgressBar m_EnergyBar;

    VisualElement m_PlayerUIWrapper;

    TextElement m_PlayerName;

    Camera m_MainCamera;

    ParticleSystem.MainModule m_ThrustMain;

    private NetworkVariable<float> m_FrictionEffectStartTimer = new(-10);

    // for client movement command throttling
    float m_OldMoveForce = 0;

    float m_OldSpin = 0;

    // server movement
    private NetworkVariable<float> m_Thrusting = new();

    float m_Spin;

    Rigidbody2D m_Rigidbody2D;

    PlayerInputControls inputActions;

    void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        inputActions = new();
        inputActions.Player.Enable();
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(m_ObjectPool, $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?");

        m_ThrustMain = m_Thrust.main;
        m_ShipGlowDefaultColor = Color.white;
        m_ShipGlow.color = m_ShipGlowDefaultColor;
        m_IsBuffed = false;

        m_RootVisualElement = m_UIDocument.rootVisualElement;
        m_PlayerUIWrapper = m_RootVisualElement.Q<VisualElement>("PlayerUIWrapper");
        m_HealthBar = m_RootVisualElement.Q<ProgressBar>(name: "HealthBar");
        m_EnergyBar = m_RootVisualElement.Q<ProgressBar>(name: "EnergyBar");
        m_PlayerName = m_RootVisualElement.Q<TextElement>("PlayerName");
        m_MainCamera = Camera.main;
    }

    void Start() {
        DontDestroyOnLoad(gameObject);
        SetPlayerUIVisibility(true);
        if (IsOwner && IsLocalPlayer) {
            if (IsServer) {
                playerName.Value = GameManager.Instance.PlayerData.PlayerName;
            } else {
                SetNameServerRpc(GameManager.Instance.PlayerData.PlayerName);
            }
        }
    }

    public override void OnNetworkSpawn() {
        Energy.OnValueChanged += OnEnergyChanged;
        Health.OnValueChanged += OnHealthChanged;
        gameObject.name = $"Player Ship : {OwnerClientId}";

        playerName.OnValueChanged += (_, newPlayerName) =>{ SetPlayerName(newPlayerName.ToString());};

        if (IsServer) {
            m_ShipGlowDefaultColor = GameManager.Instance.PlayerData.PlayerColor;
            LatestShipColor.Value = m_ShipGlowDefaultColor;

            //playerName.Value = $"Player {OwnerClientId}";
             //playerName.Value = GameManager.Instance.PlayerData.PlayerName;

            if (!IsHost) {
                SetPlayerUIVisibility(false);
            }
        }

        if (IsOwner) m_ShipGlowDefaultColor = GameManager.Instance.PlayerData.PlayerColor;
        OnEnergyChanged(0, Energy.Value);
        OnHealthChanged(0, Health.Value);

       // SetPlayerName(playerName.Value.ToString());
       // SetPlayerColor(m_ShipGlowDefaultColor);
    }

    public override void OnNetworkDespawn() {
        Energy.OnValueChanged -= OnEnergyChanged;
        Health.OnValueChanged -= OnHealthChanged;
    }

    void OnEnergyChanged(int previousValue, int newValue) {
        SetEnergyBarValue(newValue);
    }

    void OnHealthChanged(int previousValue, int newValue) {
        int diff = Math.Abs(newValue - previousValue);
        if (newValue > previousValue) {
            //Heal
            damageNumbersHealth.Spawn(transform.position, diff, Color.green);
        } else {
            //damage
            diff *= -1;
            damageNumbersHealth.Spawn(transform.position, diff, Color.red);
        }
        SetHealthBarValue(newValue);
    }

    void Fire(Vector3 direction) {
        PlayFireSoundClientRpc();
        GameManager.Instance.PlayerData.ShootsFired++;

        var damage = Bullet.BULLET_DAMAGE;
        if (QuadDamageTimer.Value > NetworkManager.ServerTime.TimeAsFloat) {
            damage = Bullet.BULLET_DAMAGE * 4;
        }

        var bounce = BounceTimer.Value > NetworkManager.ServerTime.TimeAsFloat;

        var bulletGo = m_ObjectPool.GetNetworkObject(BulletPrefab).gameObject;
        bulletGo.transform.position = shootingPoint.transform.position + direction;

        var velocity = m_Rigidbody2D.velocity;
        velocity += (Vector2)(direction) * 10;
        bulletGo.GetComponent<NetworkObject>().Spawn(true);
        var bullet = bulletGo.GetComponent<Bullet>();
        bullet.Config(this, damage, bounce, m_BulletLifetime, m_ShipGlowDefaultColor);
        bullet.SetVelocity(velocity);

    }

    [ClientRpc] void PlayFireSoundClientRpc() { fireSound.Play(); }

    void Update() {
        if (IsServer) {
            UpdateServer();
        }

        if (IsClient) {
            UpdateClient();
        }
    }

    void LateUpdate() {
        if (IsLocalPlayer) {
            // center camera.. only if this is MY player!
            Vector3 pos = transform.position;
            pos.z = -50;
            m_MainCamera.transform.position = pos;
        }
        SetWrapperPosition();
    }

    void UpdateServer() {
        // energy regen
        if (m_EnergyTimer < NetworkManager.ServerTime.TimeAsFloat) {
            if (Energy.Value < 100) {
                if (Energy.Value + 20 > 100) {
                    Energy.Value = 100;
                } else {
                    Energy.Value += 20;
                }
            }

            m_EnergyTimer = NetworkManager.ServerTime.TimeAsFloat + 1;
        }

        // update rotation 
        float rotate = m_Spin * m_RotateSpeed;
        if (RotateBuffTimer.Value > NetworkManager.ServerTime.TimeAsFloat) {
            rotate *= 2;
        }

        m_Rigidbody2D.angularVelocity = rotate;

        // update thrust
        if (m_Thrusting.Value != 0) {
            float accel = m_Acceleration;
            if (SpeedBuffTimer.Value > NetworkManager.ServerTime.TimeAsFloat) {
                accel *= 2;
            }

            Vector3 thrustVec = transform.right * (m_Thrusting.Value * accel);
            m_Rigidbody2D.AddForce(thrustVec);

            // restrict max speed
            float top = m_TopSpeed;
            if (SpeedBuffTimer.Value > NetworkManager.ServerTime.TimeAsFloat) {
                top *= 1.5f;
            }

            if (m_Rigidbody2D.velocity.magnitude > top) {
                m_Rigidbody2D.velocity = m_Rigidbody2D.velocity.normalized * top;
            }
        }
    }

    private void HandleFrictionGraphics() {
        var time = NetworkManager.ServerTime.Time;
        var start = m_FrictionEffectStartTimer.Value;
        var duration = m_Friction.main.duration;

        bool frictionShouldBeActive = time >= start && time < start + duration; // 1f is the duration of the effect

        if (frictionShouldBeActive) {
            if (m_Friction.isPlaying == false) {
                m_Friction.Play();
            }
        } else {
            if (m_Friction.isPlaying) {
                m_Friction.Stop();
            }
        }
    }

    // changes color of the ship glow sprite and the trail effects based on the latest buff color
    void HandleBuffColors() {
        m_ThrustMain.startColor = m_IsBuffed ? LatestShipColor.Value : m_ShipGlowDefaultColor;
        m_ShipGlow.material.color = m_IsBuffed ? LatestShipColor.Value : m_ShipGlowDefaultColor;
    }

    void UpdateClient() {
        HandleFrictionGraphics();
        HandleIfBuffed();

        if (!IsLocalPlayer) {
            return;
        }

        // movement
        int spin = (int)inputActions.Player.Movement.ReadValue<Vector2>().x * -1;
        /*
        if (Input.GetKey(KeyCode.A)) {
            spin += 1;
        }

        if (Input.GetKey(KeyCode.D)) {
            spin -= 1;
        }
*/
        int moveForce = (int)inputActions.Player.Movement.ReadValue<Vector2>().y;
        /*
        if (Input.GetKey(KeyCode.W)) {
            moveForce += 1;
        }

        if (Input.GetKey(KeyCode.S)) {
            moveForce -= 1;
        }
*/
        if (m_OldMoveForce != moveForce || m_OldSpin != spin) {
            ThrustServerRpc(moveForce, spin);
            m_OldMoveForce = moveForce;
            m_OldSpin = spin;
        }

        // control thrust particles
        if (moveForce == 0.0f) {
            m_ThrustMain.startLifetime = 0.1f;
            m_ThrustMain.startSize = 1f;
            GetComponent<AudioSource>().Pause();
        } else {
            m_ThrustMain.startLifetime = 0.4f;
            m_ThrustMain.startSize = 1.2f;
            GetComponent<AudioSource>().Play();
        }
        // Fire Bullet
        if (inputActions.Player.Attack.WasPerformedThisFrame()) {
            FireServerRpc();
        }

    }

    // a check to see if there's currently a buff applied, returns ship to default color if not
    private void HandleIfBuffed() {
        if (SpeedBuffTimer.Value > NetworkManager.ServerTime.Time) {
            m_IsBuffed = true;
        } else if (RotateBuffTimer.Value > NetworkManager.ServerTime.Time) {
            m_IsBuffed = true;
        } else if (TripleShotTimer.Value > NetworkManager.ServerTime.Time) {
            m_IsBuffed = true;
        } else if (DoubleShotTimer.Value > NetworkManager.ServerTime.Time) {
            m_IsBuffed = true;
        } else if (QuadDamageTimer.Value > NetworkManager.ServerTime.Time) {
            m_IsBuffed = true;
        } else if (BounceTimer.Value > NetworkManager.ServerTime.Time) {
            m_IsBuffed = true;
        } else {
            m_IsBuffed = false;
        }
        HandleBuffColors();
    }

    public void AddBuff(Buff.BuffType buff) {
        GameManager.Instance.PlayerData.PowerUpsCollected++;
        if (buff == Buff.BuffType.Speed) {
            SpeedBuffTimer.Value = NetworkManager.ServerTime.TimeAsFloat + Powerup.BUFF_DURATION;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Speed);
        }

        if (buff == Buff.BuffType.Rotate) {
            RotateBuffTimer.Value = NetworkManager.ServerTime.TimeAsFloat + Powerup.BUFF_DURATION;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Rotate);
        }

        if (buff == Buff.BuffType.Triple) {
            TripleShotTimer.Value = NetworkManager.ServerTime.TimeAsFloat + Powerup.BUFF_DURATION;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Triple);
        }

        if (buff == Buff.BuffType.Double) {
            DoubleShotTimer.Value = NetworkManager.ServerTime.TimeAsFloat + Powerup.BUFF_DURATION;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Double);
        }


        if (buff == Buff.BuffType.QuadDamage) {
            QuadDamageTimer.Value = NetworkManager.ServerTime.TimeAsFloat + Powerup.BUFF_DURATION;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.QuadDamage);
        }

        if (buff == Buff.BuffType.Bounce) {
            BounceTimer.Value = NetworkManager.ServerTime.TimeAsFloat + Powerup.BUFF_DURATION;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Bounce);
        }

        if (buff == Buff.BuffType.Health) {
            Health.Value += 20;
            if (Health.Value >= 100) {
                Health.Value = 100;
            }
        }
        if (buff == Buff.BuffType.Energy) {
            Energy.Value += 50;
            if (Energy.Value >= 100) {
                Energy.Value = 100;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (NetworkManager.Singleton.IsServer == false) {
            return;
        }
        if (other.gameObject.TryGetComponent(out ICollidable collidableObj)) {
            Damage(collidableObj.CollisionDamage);
        }
    }

    // --- ServerRPCs ---

    [ServerRpc]
    public void ThrustServerRpc(float thrusting, int spin) {
        m_Thrusting.Value = thrusting;
        m_Spin = spin;
    }

    [ServerRpc]
    public void FireServerRpc() {
        if (Energy.Value >= 10) {
            var right = transform.right;
            if (TripleShotTimer.Value > NetworkManager.ServerTime.TimeAsFloat) {
                Fire(Quaternion.Euler(0, 0, 20) * right);
                Fire(Quaternion.Euler(0, 0, -20) * right);
                Fire(right);
            } else if (DoubleShotTimer.Value > NetworkManager.ServerTime.TimeAsFloat) {
                Fire(Quaternion.Euler(0, 0, -10) * right);
                Fire(Quaternion.Euler(0, 0, 10) * right);
            } else {
                Fire(right);
            }

            Energy.Value -= 10;
            if (Energy.Value <= 0) {
                Energy.Value = 0;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNameServerRpc(string name) {
        playerName.Value = name;
    }

    void SetWrapperPosition() {
        Vector2 screenPosition = RuntimePanelUtils.CameraTransformWorldToPanel(m_PlayerUIWrapper.panel, transform.position, m_MainCamera);
        m_PlayerUIWrapper.transform.position = screenPosition;
    }

    void SetHealthBarValue(int healthBarValue) {
        m_HealthBar.value = healthBarValue;
    }

    void SetEnergyBarValue(int resourceBarValue) {
        m_EnergyBar.value = resourceBarValue;
    }

    void SetPlayerName(string playerName) {
        m_PlayerName.text = playerName;
    }
    void SetPlayerColor(Color color) {
        m_PlayerName.style.color = color;
        m_ShipGlowDefaultColor = color;
        m_ShipGlow.color = color;
    }

    void SetPlayerUIVisibility(bool visible) {
        m_RootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void Damage(float amount) {
        Health.Value -= (int)amount;
        m_FrictionEffectStartTimer.Value = NetworkManager.LocalTime.TimeAsFloat;

        if (Health.Value <= 0) {
            Die();
        }
    }

    public void Die() {
        Health.Value = 0;
        GameManager.Instance.PlayerData.Deaths++;
        //todo: reset all buffs
        SpeedBuffTimer.Value = NetworkManager.ServerTime.TimeAsFloat;
        RotateBuffTimer.Value = NetworkManager.ServerTime.TimeAsFloat;
        TripleShotTimer.Value = NetworkManager.ServerTime.TimeAsFloat;
        DoubleShotTimer.Value = NetworkManager.ServerTime.TimeAsFloat;
        QuadDamageTimer.Value = NetworkManager.ServerTime.TimeAsFloat;
        BounceTimer.Value = NetworkManager.ServerTime.TimeAsFloat;

        HandleBuffColors();
        Health.Value = 100;
        transform.position = NetworkManager.GetComponent<RandomPositionPlayerSpawner>().GetNextSpawnPosition();
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0;

    }
}

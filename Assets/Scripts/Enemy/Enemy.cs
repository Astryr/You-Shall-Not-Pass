using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Tipos de enemigo usados por oleadas / portal (Basic es el más simple para la muestra).
public enum EnemyType { Basic, Fast, Heavy, Swarm, Stealth, Flying, BossSpider, None }

// Comportamiento base: recorre waypoints del portal con NavMeshAgent, recibe daño y vuelve al pool al morir.
public class Enemy : MonoBehaviour, IDamagable
{
    public Enemy_Visuals visuals { get; private set; }

    protected ObjectPoolManager objectPool;
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected EnemyPortal myPortal;
    protected GameManager gameManager;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centerPoint;
    public float maxHp = 100;
    public float currentHp = 4;
    protected bool isDead;

    [Header("Movement")]
    [SerializeField] private float turnSpeed = 10;
    [SerializeField] protected Vector3[] myWaypoints;

    protected int nextWaypointIndex;
    protected int currentWaypointIndex;
    protected float totalDistance;
    protected float originalSpeed;

    protected bool canBeHidden = true;
    protected bool isHidden;
    private Coroutine hideCo;
    private Coroutine disableHideCo;
    private int originalLayerIndex;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        // Rotación manual (FaceTarget); el agente solo calcula path y velocidad.
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);

        visuals = GetComponent<Enemy_Visuals>();
        originalLayerIndex = gameObject.layer;

        gameManager = FindFirstObjectByType<GameManager>();
        originalSpeed = agent.speed;

        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void Start()
    {

    }


    /// <summary>Llama el portal al sacar el enemigo del pool: copia ruta, resetea stats y arranca movimiento.</summary>
    public void SetupEnemy(EnemyPortal myNewPortal)
    {
        myPortal = myNewPortal;

        UpdateWaypoints(myPortal.CurrentWaypoints);
        CollectTotalDistance();
        ResetEnemy();
        BeginMovement();
    }

    private void UpdateWaypoints(Vector3[] newWaypoints)
    {
        myWaypoints = new Vector3[newWaypoints.Length];

        for (int i = 0; i < myWaypoints.Length; i++)
            myWaypoints[i] = newWaypoints[i];
    }

    private void BeginMovement()
    {
        currentWaypointIndex = 0;
        nextWaypointIndex = 0;
        ChangeWaypoint();
    }

    protected virtual void ResetEnemy()
    {
        gameObject.layer = originalLayerIndex;

        visuals.MakeTransperent(false);

        currentHp = maxHp;
        isDead = false;

        agent.speed = originalSpeed;

        // Alinear al NavMesh (reuso desde pool puede dejar el transform fuera de malla).
        UnityEngine.AI.NavMeshHit hit;
        bool positionFound = UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 50.0f, UnityEngine.AI.NavMesh.AllAreas);

        if (positionFound)
        {
            transform.position = hit.position;
        }

        if (agent.gameObject.activeInHierarchy && this.GetType() != typeof(Enemy_BossUnit))
        {
            agent.enabled = true;
            if (positionFound)
                agent.Warp(hit.position);
        }
    }


    protected virtual void Update()
    {
        FaceTarget(agent.steeringTarget);

        if (ShouldChangeWaypoint())
        {
            ChangeWaypoint();
        }
    }
                                    
    public void SlowEnemy(float slowMultiplier,float duration) => StartCoroutine(SlowEnemyCo(slowMultiplier, duration));
    private IEnumerator SlowEnemyCo(float slowMultiplier, float duration)
    {
        agent.speed = originalSpeed;
        agent.speed = agent.speed * slowMultiplier;

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
    }
    public void DisableHide(float duration)
    {
        if(disableHideCo != null)
            StopCoroutine(disableHideCo);

        disableHideCo = StartCoroutine(DisableHideCo(duration));
    }
    protected virtual IEnumerator DisableHideCo(float duration)
    {
        canBeHidden = false;

        yield return new WaitForSeconds(duration);
        canBeHidden = true;
    }
    public void HideEnemy(float duration)
    {
        if (canBeHidden == false)
            return;

        if(hideCo != null)
            StopCoroutine(hideCo);

        hideCo = StartCoroutine(HideEnemyCo(duration));
    }
    private IEnumerator HideEnemyCo(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Untargetable");
        visuals.MakeTransperent(true);
        isHidden = true;

        yield return new WaitForSeconds(duration);

        gameObject.layer = originalLayerIndex;
        visuals.MakeTransperent(false);
        isHidden = false;
    }

    protected virtual void ChangeWaypoint()
    {
        agent.SetDestination(GetNextWaypoint());
    }
    protected virtual bool ShouldChangeWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Length)
            return false;

        if (agent.remainingDistance < .5f)
            return true;

        Vector3 currentWaypoint = myWaypoints[currentWaypointIndex];
        Vector3 nextWaypoint = myWaypoints[nextWaypointIndex];

        float distanceToNextWaypoint = Vector3.Distance(transform.position, nextWaypoint);
        float distanceBetweenPoints = Vector3.Distance(currentWaypoint, nextWaypoint);

        return distanceBetweenPoints > distanceToNextWaypoint;
    }

    public virtual float DistanceToFinishLine() => totalDistance + agent.remainingDistance;

    // Suma tramos spawn → meta (debe reiniciarse en cada Setup por object pooling).
    private void CollectTotalDistance()
    {
        totalDistance = 0f;
        for (int i = 0; i < myWaypoints.Length - 1; i++)
        {
            float distance = Vector3.Distance(myWaypoints[i], myWaypoints[i + 1]);
            totalDistance += distance;
        }
    }
    private void FaceTarget(Vector3 newTarget)
    {
        Vector3 directionToTarget = newTarget - transform.position;
        directionToTarget.y = 0;

        if (directionToTarget == Vector3.zero || directionToTarget.sqrMagnitude < 0.001f)
            return;

        Quaternion newRotation = Quaternion.LookRotation(directionToTarget);

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, turnSpeed * Time.deltaTime);
    }

    protected Vector3 GetFinalWaypoint()
    {
        if(myWaypoints.Length == 0)
            return transform.position;

        return myWaypoints[myWaypoints.Length - 1];
    }

    // Avanza al siguiente punto de la ruta y actualiza totalDistance (usado p. ej. para priorizar torres).
    private Vector3 GetNextWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Length)
        {
            return transform.position;
        }

        Vector3 targetPoint = myWaypoints[nextWaypointIndex];

        if (nextWaypointIndex > 0)
        {
            float distance = Vector3.Distance(myWaypoints[nextWaypointIndex], myWaypoints[nextWaypointIndex - 1]);
            totalDistance -= distance;
        }

        nextWaypointIndex = nextWaypointIndex + 1;
        currentWaypointIndex = nextWaypointIndex - 1;

        return targetPoint;
    }

    public Vector3 CenterPoint() => centerPoint != null ? centerPoint.position : transform.position;
    public EnemyType GetEnemyType() => enemyType;
    
    public virtual void TakeDamage(float damage)
    {
        currentHp = currentHp - damage;

        if (currentHp <= 0 && isDead == false)
        {
            isDead = true;
            Die();
        }
    }

    public virtual void Die()
    {
        gameManager?.UpdateCurrency(1);
        RemoveEnemy();
    }

    public virtual void RemoveEnemy()
    {
        visuals.CreateOnDeathVFX();
        objectPool.Remove(gameObject);
        agent.enabled = false;

        if(myPortal != null)
            myPortal.RemoveActiveEnemy(gameObject);
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        CancelInvoke();
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class Enemy_BossUnit : Enemy
{
    private Vector3 savedDestination;
    private Vector3 lastKnownBossPosition;
    private Enemy_Flying_Boss myBoss;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();

        if (myBoss != null)
            lastKnownBossPosition = myBoss.transform.position;
    }

    public void SetupEnemy(Vector3 destination, Enemy_Flying_Boss myNewBoss, EnemyPortal myNewPortal)
    {
        ResetEnemy();
        ResetMovement();

        myBoss = myNewBoss;
        myPortal = myNewPortal;
        myPortal.GetActiveEnemies().Add(gameObject);

        savedDestination = destination;

        InvokeRepeating(nameof(SnapToBossIfNeeded), .1f, .5f);
    }

    private void ResetMovement()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        agent.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Enemy")
            return;

        rb.useGravity = false;
        rb.isKinematic = true;

        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 50.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.enabled = true; // Habilitar ANTES de hacer Warp
            agent.Warp(hit.position);

            StartCoroutine(SetDestinationWhenReady(savedDestination));
        }
        else
        {
            // Destruirlo si cayó fuera por bug físico, evitamos el error
            Die();
        }
    }

    private IEnumerator SetDestinationWhenReady(Vector3 destination)
    {
        yield return null; // Esperar un frame para que Unity registre el agente en el NavMesh
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }

    private void SnapToBossIfNeeded()
    {
        if (agent.enabled && agent.isOnNavMesh == false)
        {
            if (Vector3.Distance(transform.position, lastKnownBossPosition) > 3f)
            {
                transform.position = lastKnownBossPosition + new Vector3(0, -1, 0);
                ResetMovement();
            }
        }
    }

    public override float DistanceToFinishLine()
    {
        return Vector3.Distance(transform.position, GetFinalWaypoint());
    }
}

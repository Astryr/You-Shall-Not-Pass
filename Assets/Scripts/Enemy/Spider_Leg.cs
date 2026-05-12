using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_Leg : MonoBehaviour
{
    private Enemy_Spider_Visuals spiderVisuals;
    private ObjectPoolManager objectPool;

    [SerializeField] private float legSpeed = 2.5f;
    [SerializeField] private float moveThreshold = .45f;
    private bool shouldMove;
    private bool canMove = true;
    private Coroutine moveCo;



    [Header("Leg Setup")]
    [SerializeField] private Spider_Leg opppositeLeg;
    [SerializeField] private Spider_LegReference legRef;
    [SerializeField] private Transform actualTarget;
    [SerializeField] private Transform bottomLeg;
    [SerializeField] private Vector3 placementOffset;
    [SerializeField] private Transform worldTargetReference;

    private void Awake()
    {
        objectPool = ObjectPoolManager.instance;
        spiderVisuals = GetComponentInParent<Enemy_Spider_Visuals>();

        worldTargetReference = Instantiate(worldTargetReference, actualTarget.position, Quaternion.identity).transform;
        worldTargetReference.gameObject.name = legRef.gameObject.name + "_world";

        legSpeed = spiderVisuals.legSpeed;
    }

    public void UpdateLeg()
    {

        actualTarget.position = worldTargetReference.position;// + new Vector3(0, placementOffset.y, 0);// + placementOffset;
        shouldMove = Vector3.Distance(worldTargetReference.position, legRef.ContactPoint()) > moveThreshold;

        if (bottomLeg != null)
            bottomLeg.forward = Vector3.down;


        if (shouldMove && canMove)
        {
            if (moveCo != null)
                StopCoroutine(moveCo);

            StartCoroutine(LegMoveCo());
        }
    }

    private IEnumerator LegMoveCo()
    {
        opppositeLeg.CanMove(false);

        while (Vector3.Distance(worldTargetReference.position, legRef.ContactPoint()) > .01f)
        {
            worldTargetReference.position = 
                Vector3.MoveTowards(worldTargetReference.position, legRef.ContactPoint(), legSpeed * Time.deltaTime);

            yield return null;
        }

        opppositeLeg.CanMove(true);
    }

    public void SpeedUpLeg() => StartCoroutine(SpeedUpLegCo());

    private IEnumerator SpeedUpLegCo()
    {
        legSpeed = spiderVisuals.increasedLegSpeed;

        yield return new WaitForSeconds(1f);

        legSpeed = spiderVisuals.legSpeed;
    }

    public void CanMove(bool enableMovement) => canMove = enableMovement;

    private void OnEnable()
    {
        ParentLegRefrence(false);
    }

    private void OnDisable()
    {
        ParentLegRefrence(true);
    }

    private void ParentLegRefrence(bool parent)
    {
        if (worldTargetReference == null)
            return;

        worldTargetReference.transform.parent = parent ? objectPool.transform : null;
    }

}

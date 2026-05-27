using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Visuals : MonoBehaviour
{
    private ObjectPoolManager objectPool;
     
    [SerializeField] private GameObject onDeathFx;
    [SerializeField] private float onDeathFxScale = .5f;
    [Space]
    [SerializeField] protected Transform visuals;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float verticalRotationSpeed;

    [Header("Transperency Details")]
    [SerializeField] private Material transperentMat;
    private List<Material> originalMat;
    private MeshRenderer[] myRenderers;

    protected virtual void Awake()
    {
        CollectDefaultMaterials();
        // Escalonar el offset por instancia para que los enemigos no raycaseen todos en el mismo frame.
        frameOffset = GetInstanceID() % 3;
        if (frameOffset < 0) frameOffset += 3;
    }

    protected virtual void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    private int frameOffset;

    protected virtual void Update()
    {
        // Raycast cada 3 frames en lugar de cada frame; reduce la carga de física en móvil.
        if (Time.frameCount % 3 == frameOffset)
            AlignWithSlope();
    }

    public void CreateOnDeathVFX()
    {
        GameObject newDeathVFX = objectPool.Get(onDeathFx,transform.position + new Vector3(0,.15f,0),Quaternion.identity);
        newDeathVFX.transform.localScale = new Vector3(onDeathFxScale,onDeathFxScale,onDeathFxScale);
    }

    public void MakeTransperent(bool transperent)
    {
        for (int i = 0; i < myRenderers.Length; i++)
        {
            Material materialToApply = transperent ? transperentMat : originalMat[i];
            myRenderers[i].material = materialToApply;
        }
    }


    protected void CollectDefaultMaterials()
    {
        myRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMat = new List<Material>();

        foreach (var renderer in myRenderers)
        {
            originalMat.Add(renderer.material);
        }

    }

    private void AlignWithSlope()
    {
        if (visuals == null)
            return;

        if (Physics.Raycast(visuals.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, whatIsGround))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            visuals.rotation = Quaternion.Slerp(visuals.rotation, targetRotation, Time.deltaTime * verticalRotationSpeed);
        }
    }
}

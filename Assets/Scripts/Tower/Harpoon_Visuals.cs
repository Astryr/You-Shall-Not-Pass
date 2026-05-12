using System.Collections.Generic;
using UnityEngine;

public class Harpoon_Visuals : MonoBehaviour
{
    private ObjectPoolManager objectPool;

    [SerializeField] private Transform startPoint; // gun point
    [SerializeField] private Transform endPoint; // harpoon back point
    [Space]
    [SerializeField] private GameObject linkPrefab;
    [SerializeField] private Transform linksParent;
    [SerializeField] private float linkDistance = .2f;
    [SerializeField] private int maxLinks = 100;

    private List<Projectile_HarpoonLink> links = new List<Projectile_HarpoonLink>();

    [Space]
    [SerializeField] private GameObject onElectrifyVfx;
    [SerializeField] private Vector3 vfxOffset;
    private GameObject currentVfx;

    private void Start()
    {
        InitializeLinks();
        objectPool = ObjectPoolManager.instance;
    }

    private void Update()
    {
        if (endPoint == null)
            return;

        ActivateLinksIfNeeded();
    }

    public void CreateElectrifyVFX(Transform targetTransform)
    {
        currentVfx = objectPool.Get(onElectrifyVfx, targetTransform.position + vfxOffset, Quaternion.identity, targetTransform);
    }

    private void DestroyElectrifyVFX()
    {
        if (currentVfx != null)
            objectPool.Remove(currentVfx);
    }

    public void EnableChainVisuals(bool enable, Transform newEndPoint = null)
    {
        if (enable)
        {
            endPoint = newEndPoint;
        }

        if (enable == false)
        {
            endPoint = startPoint;
            DestroyElectrifyVFX();
        }
    }

    private void ActivateLinksIfNeeded()
    {
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        int activeLinksAmount = Mathf.Min(maxLinks, Mathf.CeilToInt(distance / linkDistance));

        for (int i = 0; i < links.Count; i++)
        {
            if (i < activeLinksAmount)
            {
                Vector3 newPosition = startPoint.position + direction * linkDistance * (i + 1);
                links[i].EnableLink(true, newPosition);
            }
            else
                links[i].EnableLink(false, Vector3.zero);


            if (i != links.Count - 1)
                links[i].UpdateLineRenderer(links[i], links[i + 1]);
        }

    }

    private void InitializeLinks()
    {
        for (int i = 0; i < maxLinks; i++)
        {
            Projectile_HarpoonLink newLink =
                Instantiate(linkPrefab, startPoint.position, Quaternion.identity, linksParent).GetComponent<Projectile_HarpoonLink>();

            links.Add(newLink);
        }
    }
}

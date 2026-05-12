using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerPreview : MonoBehaviour
{
    private List<System.Type> compToKeep = new List<System.Type>();

    private MeshRenderer[] meshRenderers;
    private RadiusDisplay attackRadiusDisplay;
    private ForwardAttackDisplay forwardDisplay;

    private float attackRange;
    private bool towerAttacksForward;


    public void SetupTowerPreview(GameObject towerToBuild)
    {
        Tower tower = towerToBuild.GetComponent<Tower>();

        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        attackRadiusDisplay = transform.AddComponent<RadiusDisplay>();
        forwardDisplay = tower.GetComponent<ForwardAttackDisplay>();
        attackRange = tower.GetAttackRange();
        towerAttacksForward = tower.towerAttacksForward;

        SecureComponents();
        MakeAllMeshTransperent();
        DestroyExtraComponents();

        gameObject.SetActive(false);
    }

    public void ShowPreview(bool showPreview, Vector3 previewPosition)
    {
        transform.position = previewPosition;

        if (towerAttacksForward == false)
            attackRadiusDisplay.CreateCircle(showPreview, attackRange);
        else
            forwardDisplay.CreateLines(showPreview, attackRange);
    }

    private void SecureComponents()
    {
        compToKeep.Add(typeof(Transform));
        compToKeep.Add(typeof(TowerPreview));
        compToKeep.Add(typeof(RadiusDisplay));
        compToKeep.Add(typeof(ForwardAttackDisplay));
        compToKeep.Add(typeof(LineRenderer));
    }

    private bool ComponentSecured(Component compToCheck)
    {
        return compToKeep.Contains(compToCheck.GetType());
    }

    private void DestroyExtraComponents()
    {
        Component[] components = GetComponents<Component>();
        
        foreach (var componentToCheck in components)
        {
            if(ComponentSecured(componentToCheck) == false)
                Destroy(componentToCheck);
        }
    }
    private void MakeAllMeshTransperent()
    {
        Material previewMat = FindFirstObjectByType<BuildManager>().GetBuildPreviewMat();

        foreach (var mesh in meshRenderers)
        {
            mesh.material = previewMat;
        }
    }
}

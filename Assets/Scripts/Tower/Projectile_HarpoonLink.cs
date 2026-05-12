using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_HarpoonLink : MonoBehaviour
{
    private LineRenderer lr;
    private MeshRenderer mesh;
    private ParticleSystem vfx;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;

        mesh = GetComponentInChildren<MeshRenderer>();
        vfx = GetComponentInChildren<ParticleSystem>();

        EnableLink(false, transform.position);
        EnableVFX(false);
    }

    public void EnableLink(bool enable, Vector3 newPosition)
    {
        mesh.enabled = enable;
        transform.position = newPosition;
        lr.enabled = enable;
    }

    public void UpdateLineRenderer(Projectile_HarpoonLink startPoint, Projectile_HarpoonLink endPoint)
    { 
        lr.enabled = startPoint.CurrentlyActive() && endPoint.CurrentlyActive();
        EnableVFX(lr.enabled);


        if (lr.enabled == false)
            return;

        lr.SetPosition(0, startPoint.transform.position);
        lr.SetPosition(1, endPoint.transform.position);
    }

    private void EnableVFX(bool enable)
    {
        if (enable && vfx.isPlaying == false)
            vfx.Play();
        else
            vfx.Stop();
    }

    public bool CurrentlyActive() => mesh.enabled;
}

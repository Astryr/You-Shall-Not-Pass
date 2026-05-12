using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Stealth : Enemy
{
    [Header("Stealth Enemy Details")]
    [SerializeField] private List<Enemy> enemiesToHide;
    [SerializeField] private float hideDuration = .5f;
    [SerializeField] private ParticleSystem smokeFx;
    private bool canHideEnemies = true;

    private void HideItself() => HideEnemy(hideDuration);

    private void HideEnemies()
    {
        if (canHideEnemies == false)
            return;

        foreach (Enemy enemy in enemiesToHide)
        {
            enemy.HideEnemy(hideDuration);
        }
    }

    public List<Enemy> GetEnemiesToHide() => enemiesToHide;

    public void EnableSmoke(bool enable)
    {
        if (smokeFx.isPlaying == false && enable)
            smokeFx.Play();
        else if (smokeFx.isPlaying == true && enable == false)
            smokeFx.Stop();
    }

    protected override IEnumerator DisableHideCo(float duration)
    {
        EnableSmoke(false);
        canBeHidden = false;
        canHideEnemies = false;

        yield return new WaitForSeconds(duration);

        EnableSmoke(true);
        canBeHidden = true;
        canHideEnemies = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InvokeRepeating(nameof(HideItself), .1f, hideDuration);
        InvokeRepeating(nameof(HideEnemies), .1f, hideDuration);
    }
}

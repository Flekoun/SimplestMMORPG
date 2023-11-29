using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using simplestmmorpg.data;
using UnityEngine;

public class CombatFlowEffectSpawner : MonoBehaviour
{
    public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public PrefabFactory PrefabFactory;
    //public GameObject HitEffectPrefab;
    //public GameObject ProjectileEffectPrefab;
    public Transform Parent;
    private int LastCombatFlowEntryIndex = -1;
    private UIEncounterEntry Data;

    // Start is called before the first frame update
    public void SpawnEffect(UIEncounterEntry _encounterUI)
    {
        if (!this.isActiveAndEnabled)
            return;
        Data = _encounterUI;

        if (Data.Data.combatFlow.Length == 0)
            return;

        StartCoroutine(WaitForAWhile());
    }

    private IEnumerator WaitForAWhile()
    {

        for (int i = LastCombatFlowEntryIndex + 1; i <= Data.Data.combatFlow.Length - 1; i++)
        {

            //    Debug.Log("Spawning combat flow effect : " + i);
            CombatFlowEntry nextFlowToShow = Data.Data.combatFlow[i];
            UICombatEntity target = Data.GetUICombatEntityByUid(nextFlowToShow.target);
            UICombatEntity caster = Data.GetUICombatEntityByUid(nextFlowToShow.caster);
            Vector3 targetPos = new Vector3(target.transform.position.x, target.transform.position.y, 0);
            Vector3 casterPos = new Vector3(caster.transform.position.x, caster.transform.position.y, 0);

            if (target != null && caster != null)
            {
                Debug.Log("nextFlowToShow.effectId: " + nextFlowToShow.effectId);
                var projectile = PrefabFactory.CreateGameObject<Transform>(AllImageIdDefinitionSOSet.GetDefinitionById(nextFlowToShow.effectId).ProjectileEffectPrefab, Parent.transform, casterPos);

                var tween = projectile.transform.DOMove(targetPos, 1f).SetEase(Ease.InOutExpo);
                tween.SetAutoKill(true);
                tween.Restart();  //tohle je super dulezite imo, jinak mi to pak nefachalo kdyz sem chtel spawnovat effekt znova. Jakob se vytvroil ale auomaticky nespustil!

                tween.OnComplete(() =>
            {

                PrefabFactory.CreateGameObject<Transform>(AllImageIdDefinitionSOSet.GetDefinitionById(nextFlowToShow.effectId).ImpactEffectPrefab, Parent.transform, targetPos);
                target.SpawnFloatingTexts(nextFlowToShow);
            //    caster.SpawnFloatingTexts();
                target.ShowHitEffect();

            });
            }


            // PrefabFactory.CreateGameObject(HitEffectPrefab, target.transform);
            // PrefabFactory.CreateGameObject(HitEffectPrefab, caster.transform);

            LastCombatFlowEntryIndex = i;

            yield return new WaitForSecondsRealtime(0.5f);

        }

    }


    // Update is called once per frame
    public void ResetCombatFlow(EncounterData _data)
    {
        LastCombatFlowEntryIndex = _data.combatFlow.Length - 1;
    }
}

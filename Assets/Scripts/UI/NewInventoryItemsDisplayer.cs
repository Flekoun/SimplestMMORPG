using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using simplestmmorpg.data;
using UnityEngine;
public class NewInventoryItemsDisplayer : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public Transform PickupEffectTarget;
    public Transform Parent;
    public GameObject UIContentItemPrefab;
    public bool IsFirstCall = true;
    //public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    // Start is called before the first frame update
    void Awake()
    {
        // AccountDataSO.OnPlayerDataLoadedFirstTime += OnPlayerLoadedFirstTimeChanged;
        AccountDataSO.OnCharacterDataChanged_OldData += OnCharacterDataChanged_OldData;
    }



    // Update is called once per frame
    void OnCharacterDataChanged_OldData(CharacterData _oldData)
    {


        if (_oldData == null) return;

        if (IsFirstCall)
        {
            IsFirstCall = false;
            return;
        }




        //            List<MyObject> list1 = // ... your first list
        //List < MyObject > list2 = // ... your second list
        List<ContentContainer> newItems = new List<ContentContainer>();

        foreach (var item2 in AccountDataSO.CharacterData.inventory.content)
        {
            bool exists = false;

            foreach (var item1 in _oldData.inventory.content)
            {
                if (item1.GetContent().uid == item2.GetContent().uid)
                {
                    if (item1.GetContent().amount >= item2.GetContent().amount)
                    {
                        exists = true;
                        break;
                    }
                    else if (item1.GetContent().amount < item2.GetContent().amount)
                    {
                        item1.GetContent().amount = item2.GetContent().amount - item1.GetContent().amount;
                        newItems.Add(item1);
                        exists = true;
                    }


                }
            }
            if (!exists)
            {
                newItems.Add(item2);
            }

        }

        //jeste pridam goldy at jdou videt jestli si dostal

        if (_oldData.currency.gold < AccountDataSO.CharacterData.currency.gold)
        {
            newItems.Add(MakeContentContainer(AccountDataSO.CharacterData.currency.gold - _oldData.currency.gold, Utils.CURRENCY_ID.GOLD));
        }
        if (_oldData.currency.monsterEssence < AccountDataSO.CharacterData.currency.monsterEssence)
        {
            newItems.Add(MakeContentContainer(AccountDataSO.CharacterData.currency.monsterEssence - _oldData.currency.monsterEssence, Utils.CURRENCY_ID.MONSTER_ESSENCE));
        }
        if (_oldData.currency.scavengePoints < AccountDataSO.CharacterData.currency.scavengePoints)
        {
            newItems.Add(MakeContentContainer(AccountDataSO.CharacterData.currency.scavengePoints - _oldData.currency.scavengePoints, Utils.CURRENCY_ID.SCAVENGE_POINTS));
        }
        if (_oldData.currency.travelPoints < AccountDataSO.CharacterData.currency.travelPoints)
        {
            newItems.Add(MakeContentContainer(Utils.RoundToInt(AccountDataSO.CharacterData.currency.travelPoints - _oldData.currency.travelPoints), Utils.CURRENCY_ID.TRAVEL_POINTS));
        }
        if (_oldData.currency.time < AccountDataSO.CharacterData.currency.time)
        {
            newItems.Add(MakeContentContainer(AccountDataSO.CharacterData.currency.time - _oldData.currency.time, Utils.CURRENCY_ID.TIME));
        }
        //if (_oldData.currency.fatigue < AccountDataSO.CharacterData.currency.fatigue)
        //{
        //    newItems.Add(MakeContentContainer(AccountDataSO.CharacterData.currency.time - _oldData.currency.time, Utils.CURRENCY_ID.TIME));
        //}



        foreach (var item in newItems)
        {
            Debug.Log("You gained new item: " + item.GetContent().amount + "x " + item.GetContent().itemId);
        }

        StartCoroutine(SpawnPickupEffect(newItems, PickupEffectTarget));
        //if (_oldData.stats.level < AccountDataSO.CharacterData.stats.level)
        //{
        //    var window = UIManager.instance.SpawnPromptPanel("Level " + AccountDataSO.CharacterData.stats.level + " reached!\n +12 Time \n", null, null);
        //    window.HideDeclineButton();
        //    window.SetAcceptButtonText("Great!");

        //}

    }


    private IEnumerator SpawnPickupEffect(List<ContentContainer> _items, Transform _target)
    {
        for (int i = 0; i < _items.Count; i++)
        {

            //    Debug.Log("Spawning combat flow effect : " + i);

            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            Vector3 startingPosition = new Vector3(x, y, 0);

            if (_target == null) continue;

            Vector3 targetPos = new Vector3(_target.position.x, _target.position.y, 0);
            Vector3 casterPos = new Vector3(startingPosition.x, startingPosition.y, 0);

            if (_target != null)
            {
                //                Debug.Log("nextFlowToShow.effectId: " + nextFlowToShow.effectId);
                var uIItem = PrefabFactory.CreateGameObject<UIContentItem>(UIContentItemPrefab, Parent, casterPos);
                uIItem.SetData(_items[i].GetContent());

                // var tween = uIItem.transform.DOMove(targetPos, 1f).SetEase(Ease.InOutExpo);

                Tween tweenPopUp = uIItem.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 1f).SetRelative(true).SetEase(Ease.InOutExpo);
                Tween tweenMoveToTarget = uIItem.transform.DOMove(targetPos, 1f).SetEase(Ease.InOutExpo);


                Sequence mySequence = DOTween.Sequence();
                // mySequence.SetLoops(1);
                mySequence.Append(tweenPopUp);
                mySequence.Append(tweenMoveToTarget);

                mySequence.SetId(uIItem.gameObject);
                mySequence.Play().SetUpdate(true);



                mySequence.SetAutoKill(true);
                //  mySequence.Restart();  //tohle je super dulezite imo, jinak mi to pak nefachalo kdyz sem chtel spawnovat effekt znova. Jakob se vytvroil ale auomaticky nespustil!

                mySequence.OnComplete(() =>
                {
                    Debug.Log("Sem tu!");
                    Destroy(uIItem.gameObject);
                    //   PrefabFactory.CreateGameObject<Transform>(AllImageIdDefinitionSOSet.GetDefinitionById(nextFlowToShow.effectId).ImpactEffectPrefab, Parent.transform, targetPos);
                    //    caster.SpawnFloatingTexts();

                });
            }


            yield return new WaitForSecondsRealtime(0.25f);

        }

    }

    private ContentContainer MakeContentContainer(int _amount, string _currencyType)
    {
        var contentContainer = new ContentContainer();
        contentContainer.content = new Content();
        contentContainer.content.amount = _amount;
        contentContainer.content.contentType = Utils.CONTENT_TYPE.CURRENCY;
        contentContainer.content.currencyType = _currencyType;
        contentContainer.content.itemId = _currencyType;
        return contentContainer;
    }
}

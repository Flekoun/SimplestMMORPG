using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.UI;

public class UITooltip : MonoBehaviour
{
    //   public List<DOTweenAnimation> FadeAnims;
    public TextMeshProUGUI HeaderText;
    public TextMeshProUGUI BodyText;
    public UIContentContainerDetail ContentContainerDetail;
    public UICombatSkillVisuals UICombatSkillVisuals;
    public UICombatBuffDescription UICombatBuffDescription;

    public VerticalLayoutGroup VerticalLayout;
    public GameObject Model;
    public string StringId;
    private Transform Parent;
    private IContentDisplayable ContentDisplayable;
    private float OffsetY;
    private CombatSkill CombatSkill;

    private RectTransform canvasRectTransform;
    private Camera uiCamera;
    // Start is called before the first frame update

    //private void Reset()
    //{
    //    ContentContainerDetail.Hide();
    //    UICombatSkillVisuals.gameObject.SetActive(false);
    //    UICombatBuffDescription.gameObject.SetActive(false);
    //    HeaderText.gameObject.SetActive(false);
    //    BodyText.gameObject.SetActive(false);
    //    HeaderText.SetText("");
    //    BodyText.SetText("");

    //    StringId = string.Empty;
    //    ContentDisplayable = null;
    //    CombatSkill = null;
    //    OffsetY = 0;

    //}
    public void Setup(string _def, IContentDisplayable _contentDisplayable, CombatSkill _combatSkill, CombatBuff _combatBuff, int _manaLeft, Transform _parent, float _offSetY, int[] _values = null)
    {



        Parent = _parent;
        StringId = _def;
        ContentDisplayable = _contentDisplayable;
        CombatSkill = _combatSkill;
        //   OffsetY = _offSetY;

        if (!string.IsNullOrEmpty(StringId))
        {
            VerticalLayout.padding.bottom = 28;
            VerticalLayout.padding.top = 10;
            VerticalLayout.padding.left = 33;
            VerticalLayout.padding.right = 33;

            HeaderText.gameObject.SetActive(true);
            BodyText.gameObject.SetActive(true);
            if (Utils.DescriptionsMetadata.DoesDescriptionMetadataForIdExist(StringId))
            {

                HeaderText.SetText(Utils.ReplaceValuePlaceholderInStringWithValues(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.DescriptionsMetadata.GetDescriptionMetadataForId(StringId).title.EN), _values));
                BodyText.SetText(Utils.ReplaceValuePlaceholderInStringWithValues(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.DescriptionsMetadata.GetDescriptionMetadataForId(StringId).description.EN), _values));
                HeaderText.gameObject.SetActive(!string.IsNullOrEmpty(Utils.DescriptionsMetadata.GetDescriptionMetadataForId(StringId).title.EN));
                BodyText.gameObject.SetActive(!string.IsNullOrEmpty(Utils.DescriptionsMetadata.GetDescriptionMetadataForId(StringId).description.EN));
            }
            else
            {
                HeaderText.SetText("Ooops!");
                BodyText.SetText("Could not find localization for stringId : " + StringId);
            }
        }
        else if (ContentDisplayable != null)
        {

            VerticalLayout.padding.bottom = 50;
            VerticalLayout.padding.top = 40;
            VerticalLayout.padding.left = 33;
            VerticalLayout.padding.right = 33;

            ContentContainerDetail.Show(ContentDisplayable);

        }
        else if (CombatSkill != null)
        {
            UICombatSkillVisuals.gameObject.SetActive(true);
            UICombatSkillVisuals.SetData(CombatSkill, _manaLeft);
        }
        else if (_combatBuff != null)
        {
            UICombatBuffDescription.gameObject.SetActive(true);
            UICombatBuffDescription.SetData(_combatBuff);
        }




        Show();

    }

    //private void CheckForBoundaries()
    //{


    //    this.transform.position = Parent.position;

    //    float x = 1;
    //    float y = 1;


    //    if (this.transform.position.x > 0)
    //        x = -1;
    //    else
    //        x = 1;

    //    if (this.transform.position.y < 0)
    //    {
    //        HeaderText.transform.SetAsFirstSibling();
    //        y = 1;
    //    }
    //    else
    //    {
    //        HeaderText.transform.SetAsLastSibling();
    //        y = -1;
    //    }


    //    //if (!string.IsNullOrEmpty(StringId))
    //    //{
    //    this.transform.localScale = new Vector3(x, y, this.transform.localScale.z);
    //    HeaderText.transform.localScale = new Vector3(x, y, HeaderText.transform.localScale.z);
    //    BodyText.transform.localScale = new Vector3(x, y, BodyText.transform.localScale.z);
    //    ContentContainerDetail.transform.localScale = new Vector3(x, y, BodyText.transform.localScale.z);
    //    //}
    //}





    public void EnsureFullyVisible(RectTransform tooltipRect)
    {
        // this.transform.position = Parent.position;

        if (uiCamera == null)
            return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, tooltipRect.position);
        //screenPoint.y = screenPoint.y + OffsetY;
        Vector2 adjustedPosition = screenPoint;

        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;

        // Check left boundary
        if (screenPoint.x - tooltipWidth * tooltipRect.pivot.x < 0)
        {
            adjustedPosition.x = tooltipWidth * tooltipRect.pivot.x;
        }

        // Check right boundary
        if (screenPoint.x + tooltipWidth * (1 - tooltipRect.pivot.x) > Screen.width)
        {
            adjustedPosition.x = Screen.width - tooltipWidth * (1 - tooltipRect.pivot.x);
        }

        // Check bottom boundary
        if (screenPoint.y - tooltipHeight * tooltipRect.pivot.y < 0)
        {
            adjustedPosition.y = tooltipHeight * tooltipRect.pivot.y;
        }

        // Check top boundary
        if (screenPoint.y + tooltipHeight * (1 - tooltipRect.pivot.y) > Screen.height)
        {
            adjustedPosition.y = Screen.height - tooltipHeight * (1 - tooltipRect.pivot.y);
        }

        // Print out the adjusted position
        //Debug.Log("Adjusted Y Position: " + adjustedPosition.y);


        tooltipRect.position = uiCamera.ScreenToWorldPoint(new Vector3(adjustedPosition.x, adjustedPosition.y, tooltipRect.position.z));
    }




    public void Show()
    {

        //Canvas canvas = GetComponentInParent<Canvas>();
        //uiCamera = canvas.worldCamera;
        //if (canvas != null)
        //    canvasRectTransform = canvas.GetComponent<RectTransform>();

        Model.gameObject.SetActive(true);
        // Debug.Log("Initial Tooltip Position: " + this.transform.position);
      //  EnsureFullyVisible(this.transform as RectTransform);
        //    Debug.Log("Adjusted Tooltip Position: " + this.transform.position);


    }

    public void Hide()
    {
        Model.gameObject.SetActive(false);
        //Reset();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Firebase.Firestore;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public static class Utils
{
    //TODO :toto je proste divne, ma to byt v AccountData, ale account data mam asi zbytecne SO, a nechci ho tahat vsude do prefab kdyz chci pristoupit k nejake metode, jako je tato, tak to mam v utils staticke tride
    //podle me by to melo byt v Acoount data a account data by mel byt singleton
    public static DescriptionsMetadata DescriptionsMetadata;
    //    private static OtherMetadata OtherMetadata;
    public static string ActiveLanguage = "EN";


    public struct POI_SPECIALS
    {
        public const string AUCTION_HOUSE = "AUCTION_HOUSE";
        public const string MAILBOX = "MAILBOX";
    }

    public struct POI_TYPE
    {
        public const string ENCOUNTER = "ENCOUNTER";
        public const string DUNGEON = "DUNGEON";
        public const string TOWN = "TOWN";
    }

    public struct LOCATION_TYPE
    {
        public const string ENCOUNTERS = "ENCOUNTERS";
        public const string DUNGEON = "DUNGEON";
        public const string TOWN = "TOWN";
    }

    public struct RARITY
    {
        public const string POOR = "POOR";
        public const string COMMON = "COMMON";
        public const string UNCOMMON = "UNCOMMON";
        public const string RARE = "RARE";
        public const string EPIC = "EPIC";
        public const string LEGENDARY = "LEGENDARY";
        public const string ARTIFACT = "ARTIFACT";
    }

    public struct CONTENT_TYPE
    {
        public const string ITEM = "ITEM";
        public const string EQUIP = "EQUIP";
        public const string CURRENCY = "CURRENCY";
        public const string FOOD = "FOOD";
    }

    public struct EQUIP_SLOT_ID
    {
        public const string ANY = "ANY";
        public const string HEAD = "HEAD";
        public const string BODY = "BODY";
        public const string LEGS = "LEGS";
        public const string FINGER_1 = "FINGER_1";
        public const string HANDS = "HANDS";
        public const string FEET = "FEET";
        public const string AMULET = "AMULET";
        public const string TRINKET = "TRINKET";
        public const string WAIST = "WAIST";
        public const string BACK = "BACK";
        public const string WRIST = "WRIST";
        public const string NECK = "NECK";
        public const string SHOULDER = "SHOULDER";
        public const string OFF_HAND = "OFF_HAND";
        public const string MAIN_HAND = "MAIN_HAND";
    }

    public struct CHARACTER_CLASS
    {
        public const string ANY = "ANY";
        public const string WARRIOR = "WARRIOR";
        public const string WARLOCK = "WARLOCK";
        public const string SHAMAN = "SHAMAN";
    }



    public enum EQUIP_ATTRIBUTES
    {
        STRENGTH,
        STAMINA,
        INTELLECT,
        AGILITY,
        SPIRIT
    }


    public static Color GetClassColor(string _class)
    {

        switch (_class)

        {
            case CHARACTER_CLASS.WARRIOR: return new Color32(198, 155, 109, 255);
            case CHARACTER_CLASS.SHAMAN: return new Color32(0, 112, 221, 255);
            case CHARACTER_CLASS.WARLOCK: return new Color32(135, 136, 238, 255);

            default: return new Color(1, 1, 1);

        }

    }



    public static Color GetRarityColor(string _rarity)
    {

        switch (_rarity)

        {
            case RARITY.POOR: return new Color(0.62f, 0.62f, 0.62f);
            case RARITY.COMMON: return new Color(1f, 1f, 1f);
            case RARITY.UNCOMMON: return new Color(0.12f, 1f, 0f);
            case RARITY.RARE: return new Color(0f, 0.44f, 0.87f);
            case RARITY.EPIC: return new Color(0.64f, 0.21f, 0.93f);
            case RARITY.LEGENDARY: return new Color(1f, 0.5f, 0f);
            case RARITY.ARTIFACT: return new Color(0.9f, 0.8f, 0.5f);

            default: return new Color(1, 1, 1);

        }

    }

    public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
    {
        var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
        var destProps = typeof(TU).GetProperties()
                .Where(x => x.CanWrite)
                .ToList();

        foreach (var sourceProp in sourceProps)
        {
            if (destProps.Any(x => x.Name == sourceProp.Name))
            {
                var p = destProps.First(x => x.Name == sourceProp.Name);
                if (p.CanWrite)
                { // check if the property can be set or no.
                    p.SetValue(dest, sourceProp.GetValue(source, null), null);
                }
            }

        }

    }

    public static void DestroyAllChildren(Transform _transform, int ignoreEntries = 0)
    {
        int loopCount = 0;

        if (_transform == null)
            return;

        while (_transform.childCount > ignoreEntries)
        {
            if (!_transform.GetChild(0).name.StartsWith("@"))
                UnityEngine.Object.DestroyImmediate(_transform.GetChild(0).gameObject);
            else
                UnityEngine.Object.DestroyImmediate(_transform.GetChild(1).gameObject);

            loopCount++;
            if (loopCount > 500)
            {
                Debug.LogError("LOOP LOCK PREVENTION TRIGGERED");
                break;
            }
        }
    }

    public static void SetDescriptionMetadata(DescriptionsMetadata _metadata)
    {
        DescriptionsMetadata = _metadata;
    }

    //public static void SetOtherMetadata(OtherMetadata _metadata)
    //{
    //    OtherMetadata = _metadata;
    //}

    public static BaseDescriptionMetadata GetMetadataForPointOfInterest(string _id)
    {
        return DescriptionsMetadata.GetPointsOfInterestMetadata(_id);
    }

    public static BaseDescriptionMetadata GetMetadataForLocation(string _id)
    {
        return DescriptionsMetadata.GetLocationsMetadata(_id);
    }

    public static BaseDescriptionMetadata GetMetadataForSkill(string _skillId)
    {
        return DescriptionsMetadata.GetSkillMetadata(_skillId);
    }

    public static BaseDescriptionMetadata GetMetadataForEnemy(string _id)
    {
        return DescriptionsMetadata.GetEnemyMetadata(_id);
    }

    public static BaseDescriptionMetadata GetMetadataForItem(string _id)
    {
        return DescriptionsMetadata.GetItemsMetadata(_id);
    }

    public static BaseDescriptionMetadata GetMetadataForGatherable(string _id)
    {
        return DescriptionsMetadata.GetGatherablesMetadata(_id);
    }

    public static BaseDescriptionMetadata GetMetadataForQuest(string _id)
    {
        return DescriptionsMetadata.GetQuestMetadata(_id);
    }

    public static BaseDescriptionMetadata GetMetadataForVendors(string _id)
    {
        return DescriptionsMetadata.GetVendorsMetadata(_id);
    }

    public static BaseDescriptionMetadata GetMetadataForTrainers(string _id)
    {
        return DescriptionsMetadata.GetTrainersMetadata(_id);
    }


    public static string ReplacePlaceholdersInTextWithDescriptionFromMetadata(string _stringToModify)
    {
        var stringToModify = _stringToModify;

        foreach (var item in DescriptionsMetadata.enemies)
            stringToModify = stringToModify.Replace(item.id, item.title.GetText());

        foreach (var item in DescriptionsMetadata.items)
            stringToModify = stringToModify.Replace(item.id, item.title.GetText());

        foreach (var item in DescriptionsMetadata.locations)
            stringToModify = stringToModify.Replace(item.id, item.title.GetText());

        foreach (var item in DescriptionsMetadata.skills)
            stringToModify = stringToModify.Replace(item.id, item.title.GetText());

        foreach (var item in DescriptionsMetadata.pointsOfInterest)
            stringToModify = stringToModify.Replace(item.id, item.title.GetText());

        foreach (var item in DescriptionsMetadata.gatherables)
            stringToModify = stringToModify.Replace(item.id, item.title.GetText());

        foreach (var item in DescriptionsMetadata.professions)
            stringToModify = stringToModify.Replace(item.id, item.title.GetText());

        return stringToModify;

    }




    //public static DateTime ConvertFirebaseTimeToDateTime(string _firebaseString)
    //{

    //    return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(_firebaseString)).UtcDateTime;

    //    // Return the time converted into UTC
    //    //return DateTime.Parse(_firebaseString);
    //    // return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(expireDate.ToString())).UtcDateTime;
    //}

    public static double GetNowInMillis()
    {
        return DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public static void SetAlphaColorFromGivenImage(Graphic _image, float _aplhaValue)
    {
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _aplhaValue);
    }

    //public static void SetColorFromGivenImage(Graphic _image, Color _color)
    //{
    //    _image.color = _color;
    //}


    public static double GetTimeLeftToDateInSeconds(string _date)
    {
        double ExpireMilis = double.Parse(_date);
        double NowInMilis = Utils.GetNowInMillis();
        double durationLeft = ExpireMilis - NowInMilis;
        return durationLeft / 1000;
    }

    public static double GetTimePassedSinceDateInSeconds(string _date)
    {
        double sinceDate = double.Parse(_date);
        double nowInMilis = Utils.GetNowInMillis();
        double duration = nowInMilis - sinceDate;
        return duration / 1000;

    }

    public static double SecondsToMinutes(double _seconds)
    {
        return _seconds / 60;
    }

    public static double SecondsToHours(double _seconds)
    {
        return SecondsToMinutes(_seconds) / 60;

    }

    public static string ColorizeGivenTextWithClassColor(string _text, string _classId)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(Utils.GetClassColor(_classId)) + ">" + _text + "</color>";
    }

    public static string ColorizeGivenText(string _text, Color _color)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(_color) + ">" + _text + "</color>";
    }

    public static int RoundToInt(float _float)
    {
        return (int)System.Math.Round(_float, MidpointRounding.AwayFromZero);
    }



}

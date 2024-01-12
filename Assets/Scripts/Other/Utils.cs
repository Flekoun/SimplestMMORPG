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
    public static CharacterData CharacterData;
    //    private static OtherMetadata OtherMetadata;


    public static string ActiveLanguage = "EN";

    public struct CURRENCY_ID
    {

        public const string GOLD = "GOLD";
        public const string TIME = "TIME";
        public const string FATIGUE = "FATIGUE";
        public const string TRAVEL_POINTS = "TRAVEL_POINTS";
        public const string SCAVENGE_POINTS = "SCAVENGE_POINTS";
        public const string MONSTER_ESSENCE = "MONSTER_ESSENCE";

    }

    public struct BLESS
    {
        public const string UNWEARIED = "UNWEARIED";
    }

    public struct MONSTER_SKILL_TYPES
    {
        public const string SKILL_TYPE_ATTACK_NORMAL = "SKILL_TYPE_ATTACK_NORMAL";
        public const string SKILL_TYPE_HEAL = "SKILL_TYPE_HEAL";
        public const string SKILL_TYPE_INCREASE_STATS = "SKILL_TYPE_INCREASE_STATS";
        public const string SKILL_TYPE_BUFF = "SKILL_TYPE_BUFF";
        public const string SKILL_TYPE_BLOCK = "SKILL_TYPE_BLOCK";
        public const string SKILL_TYPE_ATTACK_AND_DEBUFF = "SKILL_TYPE_ATTACK_AND_DEBUFF";


    }

    public struct POI_SPECIALS
    {
        public const string AUCTION_HOUSE = "AUCTION_HOUSE";
        public const string MAILBOX = "MAILBOX";
        public const string BARBER = "BARBER";
        public const string DUNGEON_ENTRANCE = "DUNGEON_ENTRANCE";
        public const string DUNGEON_EXIT = "DUNGEON_EXIT";
        public const string FORGE = "FORGE";
        public const string INN = "INN";
        public const string VENDOR = "VENDOR";
        public const string QUESTGIVER = "QUESTGIVER";
        public const string CHAPEL = "CHAPEL";
        public const string TREASURE = "TREASURE";

    }

    public enum ROOM_TYPE
    {
        NONE = 0,
        MONSTER_SOLO = 1,
        MONSTER_COOP = 2, //nepouziva se
        REST = 3, //nepouziva se
        TREASURE = 4,
        TOWN = 5, //AH, Inn, Forge
        MERCHANT = 6, //je chapel,vendor, questgiver
        MONSTER_ELITE = 7, // nepouziva se
        ENDGAME = 8,
        START = 9,
        QUEST = 10,//nepouziva se
        CHAPEL = 11, //nepouziva se
        DUNGEON = 12
    }


    //public struct POI_TYPE
    //{
    //    public const string ENCOUNTER = "ENCOUNTER";
    //    public const string DUNGEON = "DUNGEON";
    //    public const string TOWN = "TOWN";
    //    //   public const string DUNGEON_ENTRANCE = "DUNGEON_ENTRANCE";
    //}

    public struct LOCATION_TYPE
    {
        public const string ENCOUNTERS = "ENCOUNTERS";
        public const string DUNGEON = "DUNGEON";
        public const string TOWN = "TOWN";
    }

    public struct PERK_SPECIAL_EFFECT
    {
        public const string ENEMY_ALL_ADD_HEALTH = "ENEMY_ALL_ADD_HEALTH";
        public const string ENEMY_RANDOM_ADD_HEALTH = "ENEMY_RANDOM_ADD_HEALTH";
        public const string INCREASE_MANA_OF_ALL_SKILLS = "INCREASE_MANA_OF_ALL_SKILLS";
        public const string DUPLICATE_ENEMY = "DUPLICATE_ENEMY";

    }


    public struct RARITY
    {
        public const string POOR = "POOR";
        public const string COMMON = "COMMON";
        public const string UNCOMMON = "UNCOMMON";
        public const string RARE = "RARE";
        public const string EPIC = "EPIC";
        public const string LEGENDARY = "LEGENDARY";
        public const string MYTHICAL = "MYTHICAL";
        public const string ARTIFACT = "ARTIFACT";
    }

    public struct CONTENT_TYPE
    {
        public const string ITEM = "ITEM";
        public const string EQUIP = "EQUIP";
        public const string CURRENCY = "CURRENCY";
        public const string FOOD = "FOOD";
        public const string RECIPE = "RECIPE";
        public const string GENERATED = "GENERATED";
        public const string FOOD_SUPPLY = "FOOD_SUPPLY";
        public const string RANDOM_EQUIP = "RANDOM_EQUIP";
        public const string CHEST = "CHEST";
    }

    public struct ENCOUNTER_CONTEXT
    {
        public const string PERSONAL = "PERSONAL";
        public const string DUNGEON = "DUNGEON";
        public const string WORLD_BOSS = "WORLD_BOSS";
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
        public const string EARRING = "EARRING";
        public const string CHARM = "CHARM";
        public const string BAG = "BAG";
    }

    public struct CHARACTER_CLASS
    {
        public const string ANY = "ANY";
        public const string WARRIOR = "WARRIOR";
        public const string WARLOCK = "WARLOCK";
        public const string SHAMAN = "SHAMAN";
    }

    public struct PROFESSIONS
    {
        public const string ALCHEMY = "ALCHEMY";
        public const string HERBALISM = "HERBALISM";
        public const string MINING = "MINING";
        public const string BLACKSMITHING = "BLACKSMITHING";
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
            case CHARACTER_CLASS.ANY: return new Color32(255, 255, 255, 255);
            default: return new Color(1, 0, 0);

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
            case RARITY.MYTHICAL: return new Color(1f, 0.0f, 0f);
            case RARITY.ARTIFACT: return new Color(0.9f, 0.8f, 0.5f);

            default: return new Color(1, 1, 1);

        }

    }


    public static int GetRarityIndex(string _rarity)
    {

        switch (_rarity)

        {
            case RARITY.POOR: return 0;
            case RARITY.COMMON: return 1;
            case RARITY.UNCOMMON: return 2;
            case RARITY.RARE: return 3;
            case RARITY.EPIC: return 4;
            case RARITY.LEGENDARY: return 5;
            case RARITY.MYTHICAL: return 6;
            case RARITY.ARTIFACT: return 7;

            default:
                return -100;

        }

    }

    public static string GetRarityByIndex(int _index)
    {

        switch (_index)

        {
            case 0: return RARITY.POOR;
            case 1: return RARITY.COMMON;
            case 2: return RARITY.UNCOMMON;
            case 3: return RARITY.RARE;
            case 4: return RARITY.EPIC;
            case 5: return RARITY.LEGENDARY;
            case 6: return RARITY.MYTHICAL;
            case 7: return RARITY.ARTIFACT;

            default:
                Debug.LogError("UNKNOWN RARITY");
                return "UNKNOWN RARITY";

        }

    }

    public static int GetCurrencyIdIndex(string _index)
    {

        switch (_index)

        {
            case CURRENCY_ID.GOLD: return 0;
            case CURRENCY_ID.MONSTER_ESSENCE: return 1;


            default:
                Debug.LogError("UNKNOWN CURRENCY ID");
                return -100;

        }

    }

    public static string GetCurrencyIdByIndex(int _index)
    {

        switch (_index)

        {
            case 0: return CURRENCY_ID.GOLD;
            case 1: return CURRENCY_ID.MONSTER_ESSENCE;


            default:
                Debug.LogError("UNKNOWN RARITY");
                return "UNKNOWN RARITY";

        }

    }

    public static string GetPerkSpecialEffectIdByIndex(int _index)
    {

        switch (_index)

        {
            case 0: return PERK_SPECIAL_EFFECT.ENEMY_ALL_ADD_HEALTH;
            case 1: return PERK_SPECIAL_EFFECT.ENEMY_RANDOM_ADD_HEALTH;
            case 2: return PERK_SPECIAL_EFFECT.INCREASE_MANA_OF_ALL_SKILLS;
            case 3: return PERK_SPECIAL_EFFECT.DUPLICATE_ENEMY;

            default:
                Debug.LogError("UNKNOWN PERK SPECIAL EFFECT");
                return "UNKNOWN PERK SPECIAL EFFECT";

        }

    }

    public static int GetIndexByPerkSpecialEffectIdBy(string _perkEffectId)
    {

        switch (_perkEffectId)

        {
            case PERK_SPECIAL_EFFECT.ENEMY_ALL_ADD_HEALTH: return 0;
            case PERK_SPECIAL_EFFECT.ENEMY_RANDOM_ADD_HEALTH: return 1;
            case PERK_SPECIAL_EFFECT.INCREASE_MANA_OF_ALL_SKILLS: return 2;
            case PERK_SPECIAL_EFFECT.DUPLICATE_ENEMY: return 3;

            default:
                Debug.LogError("UNKNOWN PERK SPECIAL EFFECT INDEX");
                return -1;

        }

    }

    public static string GetEquipSlotByIndex(int _index)
    {

        switch (_index)

        {
            case 0: return EQUIP_SLOT_ID.ANY;
            case 1: return EQUIP_SLOT_ID.AMULET;
            case 2: return EQUIP_SLOT_ID.BACK;
            case 3: return EQUIP_SLOT_ID.BODY;
            case 4: return EQUIP_SLOT_ID.CHARM;
            case 5: return EQUIP_SLOT_ID.EARRING;
            case 6: return EQUIP_SLOT_ID.FEET;
            case 7: return EQUIP_SLOT_ID.FINGER_1;
            case 8: return EQUIP_SLOT_ID.HANDS;
            case 9: return EQUIP_SLOT_ID.HEAD;
            case 10: return EQUIP_SLOT_ID.LEGS;
            case 11: return EQUIP_SLOT_ID.MAIN_HAND;
            case 12: return EQUIP_SLOT_ID.NECK;
            case 13: return EQUIP_SLOT_ID.OFF_HAND;
            case 14: return EQUIP_SLOT_ID.SHOULDER;
            case 15: return EQUIP_SLOT_ID.WAIST;
            case 16: return EQUIP_SLOT_ID.WRIST;
            case 17: return EQUIP_SLOT_ID.BAG;
            case 18: return EQUIP_SLOT_ID.TRINKET;


            default:
                return "UNKNOWN RARITY";

        }

    }

    public static int GetIndexByEquipSlot(string _index)
    {

        switch (_index)

        {
            case EQUIP_SLOT_ID.ANY: return 0;
            case EQUIP_SLOT_ID.AMULET: return 1;
            case EQUIP_SLOT_ID.BACK: return 2;
            case EQUIP_SLOT_ID.BODY: return 3;
            case EQUIP_SLOT_ID.CHARM: return 4;
            case EQUIP_SLOT_ID.EARRING: return 5;
            case EQUIP_SLOT_ID.FEET: return 6;
            case EQUIP_SLOT_ID.FINGER_1: return 7;
            case EQUIP_SLOT_ID.HANDS: return 8;
            case EQUIP_SLOT_ID.HEAD: return 9;
            case EQUIP_SLOT_ID.LEGS: return 10;
            case EQUIP_SLOT_ID.MAIN_HAND: return 11;
            case EQUIP_SLOT_ID.NECK: return 12;
            case EQUIP_SLOT_ID.OFF_HAND: return 13;
            case EQUIP_SLOT_ID.SHOULDER: return 14;
            case EQUIP_SLOT_ID.WAIST: return 15;
            case EQUIP_SLOT_ID.WRIST: return 16;
            case EQUIP_SLOT_ID.BAG: return 17;
            case EQUIP_SLOT_ID.TRINKET: return 18;
            default: return -1;

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

    public static void ShowAllChildren(Transform _transform, bool _show)
    {

        for (int i = 0; i < _transform.childCount; i++)
        {
            _transform.GetChild(i).gameObject.SetActive(_show);
        }

    }

    public static void SetDescriptionMetadata(DescriptionsMetadata _metadata)
    {
        DescriptionsMetadata = _metadata;

    }

    public static void SetCharacterData(CharacterData _character)
    {
        CharacterData = _character;
    }



    public static string ReplacePlaceholdersInTextWithDescriptionFromMetadata(string _stringToModify)
    {
        if (string.IsNullOrWhiteSpace(_stringToModify))
            return "";

        var stringToModify = _stringToModify;

        foreach (var item in DescriptionsMetadata.enemies)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.items)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.locations)
            stringToModify = stringToModify.Replace("{" + item.descriptionData.id + "}", "<b>" + item.descriptionData.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.skills)
        {
            foreach (var itemid in item.id)
            {
                stringToModify = stringToModify.Replace("{" + itemid + "}", "<b>" + item.title.GetText() + "</b>");

            }
        }


        foreach (var item in DescriptionsMetadata.monsterSkillTypes)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.pointsOfInterest)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.gatherables)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.professions)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.portraits)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.rareEffects)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.cratingRecipes)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.leaderboardScoreTypes)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.perkSpecialEffects)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.UI)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.pointsOfInterestRoomTypes)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.blesses)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.foodEffects)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.equipSlots)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        foreach (var item in DescriptionsMetadata.rarities)
            stringToModify = stringToModify.Replace("{" + item.id + "}", "<b>" + item.title.GetText() + "</b>");

        if (CharacterData != null)
            stringToModify = stringToModify.Replace("{CHARACTER_NAME}", "<b>" + CharacterData.characterName + "</b>");

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

    public static string ConvertTimestampToReadableString(string _timestampInMillis)
    {
        double expireTimeInMilis = double.Parse(_timestampInMillis);
        double nowInMilis = Utils.GetNowInMillis();
        double durationLeft = expireTimeInMilis - nowInMilis;
        //Debug.Log("nowInMilis: " + nowInMilis);
        //Debug.Log("expireTimeInMilis: " + expireTimeInMilis);
        //Debug.Log("durationLeft: " + durationLeft);


        double secondsLeft = durationLeft / 1000;
        double minutesLeft = durationLeft / 60000;
        double hoursLeft = durationLeft / 3600000;
        double daysLeft = durationLeft / (3600000 * 24);

        //return Math.Floor(daysLeft) + "d" + Math.Floor(hoursLeft) + "h : " + Math.Floor(minutesLeft) + "m :" + Math.Floor(secondsLeft) + "s";

        TimeSpan duration = TimeSpan.FromMilliseconds(durationLeft);

        string format = "";
        if (daysLeft > 1)
            format = "dd\\dhh\\hmm\\mss\\s";
        else if (hoursLeft > 1)
            format = "hh\\hmm\\mss\\s";
        else if (minutesLeft > 1)
            format = "mm\\mss\\s";
        else
            format = "ss\\s";

        return duration.ToString(format);
    }

    public static string ConvertTimestampToTimeLeft(string _timestampInMillis)
    {
        double ExpireMilis = double.Parse(_timestampInMillis);
        double NowInMilis = Utils.GetNowInMillis();

        double durationLeft = ExpireMilis - NowInMilis;

        double minutesLeft = durationLeft / 60000;
        double hoursLeft = durationLeft / 3600000;

        if (minutesLeft < 0)
            return "Expired";
        if (minutesLeft < 2)
            return "Less than 2 minutes";
        if (minutesLeft < 10)
            return "Less than 10 minutes";
        else if (minutesLeft < 30)
            return "Less than 30 minutes";
        else if (minutesLeft < 60)
            return "Less than hourleft";
        else
            return "Less than " + Mathf.Ceil((float)hoursLeft) + " hours";

    }

    public static string ColorizeGivenTextWithClassColor(string _text, string _classId)
    {
        if (string.IsNullOrEmpty(_text)) return string.Empty;
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(Utils.GetClassColor(_classId)) + ">" + _text + "</color>";
    }

    public static string ColorizeGivenText(string _text, Color _color)
    {
        if (string.IsNullOrEmpty(_text)) return string.Empty;
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(_color) + ">" + _text + "</color>";
    }

    public static int RoundToInt(float _float)
    {
        return (int)System.Math.Round(_float, MidpointRounding.AwayFromZero);
    }


    public static string ReplaceValuePlaceholderInStringWithValues(string _string, int[] _values)
    {
        if (_values == null)
            return _string;

        string result = _string;

        var replacements = new List<(string key, string value)> { };

        int i = 0;
        foreach (var value in _values)
        {
            replacements.Add((i.ToString(), value.ToString()));
            i++;
        }

        StringBuilder templateBuild = new StringBuilder(_string);
        //  result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", (int.Parse(t.value) * 100f).ToString())).ToString();

        result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}%", "<color=\"yellow\">" + (((float.Parse(t.value)) * 100f).ToString()) + "%</color>")).ToString();
        result = replacements.Aggregate(templateBuild, (s, t) => s.Replace($"{{{t.key}}}", "<color=\"yellow\">" + t.value + "</color>")).ToString();


        result = Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(result);
        return result;

    }

    public static bool ArePositionsSame(WorldPosition _pos1, WorldPosition _pos2)
    {
        return (_pos1.pointOfInterestId == _pos2.pointOfInterestId &&
               _pos1.locationId == _pos2.locationId &&
               _pos1.zoneId == _pos2.zoneId);


    }
}

//using UnityEngine;

using System.Collections.Generic;

using Corgi;
using Corgi.Spec;

namespace Corgi.Spec
{
    [System.Serializable, ProtoBuf.ProtoContract]
    public class SpecCharacterInfo : StaticSpecData
    {
        public SpecCharacterInfo()
        {
            Category = StaticDataCategory.charInfo;
            DataType = StaticDataType.characterInfo;
        }

        public override bool Parse(JSONObject obj)
        {
            name = CorgiJson.ParseString(obj, "name");
            modelName = CorgiJson.ParseString(obj, "modelName");
            combatModel = CorgiJson.ParseString(obj, "combatModel");
            portrait = CorgiJson.ParseString(obj, "portrait");

            type = (CharRoleType)CorgiSpecHelper.ParseEnum(typeof(CharRoleType), CorgiJson.ParseString(obj, "type"));

			level = (int)CorgiJson.ParseInt(obj, "level");
			normalAttack = CorgiJson.ParseLong(obj, "normalAttack");
			bitterAttack = CorgiJson.ParseLong(obj, "bitterAttack");
			counterAttack = CorgiJson.ParseLong(obj, "counterAttack");

            characterPassive = CorgiJson.ParseArrayLong(obj, "characterPassive");
            List<string> statArray = CorgiJson.ParseArrayString(obj, "passiveEnhanceStat");
            passiveEnhanceStat = new List<StatType>();
            foreach(string statStr in statArray)
            {
                passiveEnhanceStat.Add((StatType) CorgiSpecHelper.ParseEnum(typeof(StatType), statStr));
            }

            passiveEnhanceStatPercent = CorgiJson.ParseArrayInt(obj, "passiveEnhanceStatPercent");
            passiveOpenGrade = CorgiJson.ParseArrayInt(obj, "passiveOpenGrade");
            gradeReqLevel = CorgiJson.ParseArrayInt(obj, "gradeReqLevel");
            gradeReqSkill = CorgiJson.ParseArray(obj, "gradeReqSkill");
            upgradeGold = CorgiJson.ParseArrayInt(obj, "upgradeGold");
            upgradeItem = CorgiJson.ParseArray(obj, "upgradeItem");
			enhanceableStat = CorgiJson.ParseArrayString(obj, "enhanceableStat");


            skillSlots = CorgiJson.ParseArrayLong(obj, "skillSlots");
            equipmentSlots = CorgiJson.ParseArrayLong(obj, "equipmentSlots");

            charType = (CharacterType)CorgiSpecHelper.ParseEnum(typeof(CharacterType), name);

			if (CorgiJson.IsValidInt(obj, "modelIndex"))
			{
				modelIndex = (int)CorgiJson.ParseInt(obj, "modelIndex");				
			}

			if (CorgiJson.IsValid(obj, "characterGuildBuff"))
				characterGuildBuff = CorgiJson.ParseArrayLong (obj, "characterGuildBuff");
            return base.Parse(obj);
        }

        public string Name
        {
            get
            {
				return GetClassName(CharacterGrade.Grade_0);
            }
        }

		public string PortraitName 
		{ 
			get 
			{
				return (modelIndex > 0) ? portrait + modelIndex.ToString() : portrait;
			} 
		}	


		public string GetClassName(CharacterGrade charGrade)
		{
			string ret = stringTable.GetString(LangStringType.characterInfo, "name", (int)charType, (int)charGrade);
			if (ret == null)
			{
				return "N/A class";
			}
			else
			{
				return ret;
			}
		}

        public string Story
        {
            get
            {
                string ret = stringTable.GetString(LangStringType.characterInfo, "lore", code);
                if (ret == null)
                {
                    return "Invalid Story";
                }
                else
                {
                    return ret;
                }
            }
        }

        [ProtoBuf.ProtoMember(2)]
        public string name;
        [ProtoBuf.ProtoMember(3)]
        public string modelName;
        [ProtoBuf.ProtoMember(4)]
        public CharacterType charType;
        [ProtoBuf.ProtoMember(5)]
        public string combatModel;
        [ProtoBuf.ProtoMember(6)]
		public string portrait;
        [ProtoBuf.ProtoMember(7)]
        public CharRoleType type;

        [ProtoBuf.ProtoMember(8)]
        public List<long> characterPassive;
        [ProtoBuf.ProtoMember(9)]
        public List<StatType> passiveEnhanceStat;
        [ProtoBuf.ProtoMember(10)]
        public List<int> passiveEnhanceStatPercent;

        [ProtoBuf.ProtoMember(11)]
        public List<int> passiveOpenGrade;
        [ProtoBuf.ProtoMember(12)]
        public List<int> gradeReqLevel;
        [ProtoBuf.ProtoMember(13)]
        public List<JSONObject> gradeReqSkill;
        [ProtoBuf.ProtoMember(14)]
        public List<int> upgradeGold;
        [ProtoBuf.ProtoMember(15)]
        public List<JSONObject> upgradeItem;


        [ProtoBuf.ProtoMember(16)]
        public long normalAttack;
        [ProtoBuf.ProtoMember(17)]
        public List<long> skillSlots;
        [ProtoBuf.ProtoMember(18)]
        public List<long> equipmentSlots;
        [ProtoBuf.ProtoMember(19)]
		public int level;
        [ProtoBuf.ProtoMember(20)]
		public List<string> enhanceableStat;
		[ProtoBuf.ProtoMember(21)]
		public int modelIndex;

		[ProtoBuf.ProtoMember(22)]
		public List<long> characterGuildBuff;

		[ProtoBuf.ProtoMember(23)]
		public long bitterAttack;

		[ProtoBuf.ProtoMember(24)]
		public long counterAttack;
    }
}
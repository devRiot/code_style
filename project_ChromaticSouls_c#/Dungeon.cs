
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

using Corgi;
using Corgi.Spec;

namespace Corgi.GameLogic
{
	public interface IDungeon
	{
		long Code { get; }
        string DungeonId { get ; }
		string DungeonName { get; }
		string DungeonTypeString { get; }
		DungeonType DungeonType { get; }

		int CurStage { get; }

		int StageSize { get; }
		List<DungeonStage> StageList { get; }
		IDungeonStage GetCurStage();
		bool DoNextStage();

        void OnDestroy();
	}

    [Serializable]
	public class Dungeon : GameObject<SpecDungeonInfo>, IDungeon
	{
		protected List<DungeonStage> _stages = new List<DungeonStage>();

        Character _friendChar = null;
        protected string _dungeonId;
		protected string _dungeonTypeString;
		protected DungeonType _dungeonType;

		//DateTime _expireTime;

		long IDungeon.Code { get { return _spec.Code; } }
        public string DungeonId { get { return _dungeonId; } }
		public string DungeonName { get { return _spec.name; } }
		public string DungeonTypeString { get { return _dungeonTypeString; } }
		public DungeonType DungeonType { get { return _dungeonType; } }
        public Character FriendChar { get { return _friendChar; } }
        
        protected int _curStage = 0;

		public int CurStage { get { return _curStage; } }
		public int StageSize { get { return _stages.Count; } }
		public List<DungeonStage> StageList { get { return _stages; } }

		public Dungeon() {}

		public override bool Load(JSONObject json)
		{
			long code = CorgiJson.ParseLong(json, "dungeonCode");
			
			if(code <= 0)
			{
                DebugUtils.LogWarning("invalid dungeon code");
				return false;
			}
			
			_dungeonType = DungeonType.Normal;
			_dungeonTypeString = "dungeon";
			SpecDungeonInfo spec = (SpecDungeonInfo) CorgiStaticData.Instance.GetData("dungeonInfo", code);
			if(spec == null)
			{
				_dungeonType = DungeonType.Term;
				_dungeonTypeString = "termDungeon";
				spec = (SpecDungeonInfo) CorgiStaticData.Instance.GetData("termDungeonInfo", code);
			}
			if(spec == null)
			{
				_dungeonType = DungeonType.Periodic;
				_dungeonTypeString = "periodicDungeon";
				spec = (SpecDungeonInfo) CorgiStaticData.Instance.GetData("periodicDungeonInfo", code);
			}
			if(spec == null)
			{
				_dungeonType = DungeonType.Heroic;
				_dungeonTypeString = "dungeon";
				spec = (SpecDungeonInfo) CorgiStaticData.Instance.GetData("heroicDungeonInfo", code);
			}
            if (spec == null)
			{
				_dungeonType = DungeonType.SubStory;
				_dungeonTypeString = "subStoryDungeon";
                spec = (SpecDungeonInfo)CorgiStaticData.Instance.GetData("subStoryDungeonInfo", code);
			}

            if (spec == null)
            {
				_dungeonType = DungeonType.Tier;
                _dungeonTypeString = "tierDungeonInfo";
                spec = (SpecDungeonInfo)CorgiStaticData.Instance.GetData("tierDungeonInfo", code);
            }
	
			if(spec == null)
			{
                DebugUtils.LogWarning("invalid dungeon spec");
				return false;
			}

			_spec = spec;


            _dungeonId = CorgiJson.ParseString(json, "dungeonId");

           // long expireTime = CorgiJson.ParseLong(json, "expireTimestamp");
           // _expireTime = CorgiTime.ConvertToDatetime(expireTime);

            // todo: stamina result
            if (CorgiJson.IsValid(json, "staminaResult"))
            {
                List<JSONObject> staminaResult = CorgiJson.ParseArray(json, "staminaResult");

                foreach (JSONObject jsonItem in staminaResult)
                {
                    string charId = CorgiJson.ParseString(jsonItem, "characterId");
                    int curStamina = CorgiJson.ParseInt(jsonItem, "currentStamina");
                    long lastConsumeTimestamp = CorgiJson.ParseLong(jsonItem, "lastConsumeTimestamp");

                    CharacterItem charInst = Player.Instance.GetCharacter(charId);
                    if (charInst != null)
                    {
                        charInst.UpdateStamina(curStamina, lastConsumeTimestamp);
                    }
                }
            }

			int stageIndex = 0;
			foreach(JSONObject jsonItem in CorgiJson.ParseArray(json, "dungeonInfo"))
			{
				DungeonStage stage = new DungeonStage();

				if( stage.InitObject(jsonItem) == false)
				{
                    DebugUtils.LogWarning("failed : create stage");
					return false;
				}
				stage.Index = stageIndex;
				stageIndex++;

				_stages.Add(stage);
			}

			return true;
		}

		public override bool InitState()
		{
			return true;
		}

		public bool DoNextStage()
		{
			_curStage++;
			return _curStage < StageSize;
		}

		public IDungeonStage GetCurStage()
		{
			return (_stages.Count > _curStage) ? _stages[_curStage] : _stages[_stages.Count - 1];
		}

		public int MonsterCount { get { return GetCurStage().MonsterCount; }}

		public override void OnDestroy()
		{
			base.OnDestroy();

			if(null!=_stages)
			{
				_stages.Clear();
				_stages = null;
			}
			_spec = null;
		}


	}

    [Serializable]
    public class DungeonTest : Dungeon
    {
		public override bool Load(long code)
		{
			if(code <= 0)
			{
                DebugUtils.LogWarning("invalid dungeon code");
				return false;
			}

			SpecDungeonInfo spec = CorgiLogicHelper.GetSpecDungeonInfo(code);
			if(spec == null)
			{
                DebugUtils.LogWarning("invalid dungeon spec");
				return false;
			}

			_spec = spec;

            _dungeonId = "testDungeonId";

            DungeonStageTest stage = new DungeonStageTest();

            long stageCode = 11;

            if(_spec.stages.Count > 0 && _spec.stages[0] >= 0)
            {
                stageCode = _spec.stages[0];
            }

            if( stage.InitObject(stageCode) == false)
            {
                DebugUtils.LogWarning("failed : create stage");
                return false;
            }
            stage.Index = 0;
			
            _stages.Add(stage);
			return true;
		}
		
		public bool LoadForPlayMode(long code)
		{
			if(code <= 0)
			{
                DebugUtils.LogWarning("invalid dungeon code");
				return false;
			}
			
			//reset stage
			_curStage = 0;
			
			if( code > 70000000) //guild raid
			{
				SpecGuildRaidDungeon spec = (SpecGuildRaidDungeon)CorgiStaticData.Instance.GetData(CorgiSheet.GUILD_RAID_DUNGEON, code);
				if(spec == null)
				{
					return false;
				}
				
				_spec = new SpecDungeonInfo();
				
				DungeonStageTest stage = new DungeonStageTest();
				if(stage.InitObject(spec.stage) == false)
				{
					return false;
				}
				stage.Index = 0;
				
				_stages.Clear();
				_stages.Add(stage);
			}
			else
			{
				SpecDungeonInfo spec = CorgiLogicHelper.GetSpecDungeonInfo(code);
				if(spec == null)
				{
	                DebugUtils.LogWarning("invalid dungeon spec");
					return false;
				}
	
				_spec = spec;
	            _dungeonId = "testDungeonId";
	            
	            if(_spec.stages.Count > 0 && _spec.stages[0] >= 0)
	            {
					int stageIndex =0;
					_stages.Clear();
					for(int i=0; i<_spec.stages.Count; i++)
					{
						DungeonStageTest stage = new DungeonStageTest();
						if( stage.InitObject(_spec.stages[i]) == false)
			            {
			                DebugUtils.LogWarning("failed : create stage");
			                return false;
			            }
			            stage.Index = stageIndex++;
						
			            _stages.Add(stage);
					}
	            }
			}
			return true;
		}

        public bool ChangeStage(long code)
        {
            DungeonStageTest stage = new DungeonStageTest();

            if( stage.InitObject(code) == false)
            {
                DebugUtils.LogWarning("failed : create stage");
                return false;
            }

            stage.Index = 0;

            _stages.Clear();
            _stages.Add(stage);

            return true;
        }

        public bool ChangeMonsterModel(int unitIndex, long code)
        {
            DungeonStageTest testStage = _stages[0] as DungeonStageTest;
            if(testStage == null)
            {
                return false;
            }

            if( testStage.ChangeMonsterModel(unitIndex, code) == false)
            {
                return false;
            }
            return true;
        }
    }
}

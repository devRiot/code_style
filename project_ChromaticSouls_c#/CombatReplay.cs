using System;
using System.Collections;
using System.Collections.Generic;

using Corgi;
using Corgi.Spec;
using Corgi.GameLogic;

namespace CorgiCombatLogServer
{
	public class CombatReplay : ICombatUIInterface
	{
		public DungeonReplay _replayInfo = null;
		public CombatLogInfo _combatLog = null;
		public string _battleResult = "";

		private int 			activeActionIndex;
		private bool			isRunning = false;

		private DungeonManager	DM = null;

		int						_stageTurn = -1;
		int						_playerActionCount = 0;

		public bool IsRunning
		{
			get { return isRunning; }
		}

		public CombatReplay(CombatLogInfo combatLog)
		{
			_replayInfo = combatLog.dungeonReplay;
			_combatLog = combatLog;

			CorgiCombatRandom.SetSeed (_replayInfo.Seed);

			//add log header
			AddSummaryLog(string.Format("server revision : {0}\n", ServerConfig.staticDataRevision));
			AddSummaryLog(string.Format("data revision : {0}, appVersion : {1}\n", combatLog.dungeonReplay.Revision, combatLog.dungeonReplay.AppVersion));
			AddSummaryLog(string.Format("DungeonName : {0}, DungeonDone : {1}\n", DungeonManager.Instance.GetCurDungeon().DungeonName, combatLog.ct.ToString()));
		}

		public void Ex()
		{
			IDungeon dungeon = null;

			dungeon.GetCurStage ();
		}

		public bool WaitForCombat()
		{
			IDungeon curDungeon = null;
			DM = DungeonManager.Instance;
			if (DM == null)
				return false;

			isRunning = true;

			//long combat summary
			AddSummaryLog("[Enter Characters Info]\n");
			foreach (Character character in DM.CharList) {
				string strCharInfo = string.Format ("Name : {0}({1}), Lv : {2} CS : {3}\n", character.Name, character.UnitIndex, character.Level, character.CharacterItem.CharacterScore);

				//skills
				foreach (Skill skill in character.Skills) {
					if (skill.SkillBook == null)
						continue;
					string strSkillInfo = string.Format ("- Skill : {0}({1}), Lv : {2}\n", skill.SkillBook.Name, skill.SkillBook.GradeName, skill.SkillBook.Level);
					strCharInfo += strSkillInfo;
				}

				AddSummaryLog (strCharInfo);
			}

			curDungeon = DM.GetCurDungeon();
			while(isRunning)
			{
				switch(DM.State)
				{
				case DungeonState.EnterStage:
					if (DM.EnterStage () == false) {
					}	

					if (_combatLog.dungeonType == CombatLogDungeonType.Raid || _combatLog.dungeonType == CombatLogDungeonType.GuildRaid) {
						//adjust monster hp
						if (CorgiJson.IsValid (_combatLog.dungeonData["status"], "monsters")) {
							JSONObject statusJson = _combatLog.dungeonData.GetField("status");

							foreach (JSONObject monsterJson in CorgiJson.ParseArray(statusJson, "monsters")) {
								string id = CorgiJson.ParseString (monsterJson, "monsterId");
								int hp = CorgiJson.ParseInt (monsterJson, "hp");

								foreach (Monster monster in DM.MonsterList) {
									if (monster.Id == id && monster.CurHP != hp) {

										AddLog(string.Format("Raid MonsterHP Adjust : {0} {1}->{2}", monster.Name, monster.CurHP, hp));
										monster.ResetHP (hp);
										break;
									}
								}
							}
						}
					}

					WaitProcessCombatResult (DungeonState.EnterStage);
					break;
				case DungeonState.EnterTurn:
					if (DM.EnterTurn () == false) {
					}

					string strEnterTurnLog = "";
					foreach (IUnit unit in DM.TurnList) {
						strEnterTurnLog += string.Format ("EnterTurn : {0} {1} hp:{2}\n", unit.Name, unit.UnitIndex, unit.CurHP);
					}
					AddLog(strEnterTurnLog);
					break;
				case DungeonState.PreAction:
					ActionProgress preActionResult = DM.DoPreAction();

					SetTurnList(true);

					#if DEBUG
					strEnterTurnLog = "";
					foreach (IUnit unit in DM.TurnList) {
						strEnterTurnLog += string.Format ("UpdateTurn : {0} {1} hp:{2}\n", unit.Name, unit.UnitIndex, unit.CurHP);
					}
					AddLog(strEnterTurnLog);
					#endif 

					if(preActionResult != ActionProgress.Error)
					{							
						WaitProcessCombatResult (DungeonState.PreAction);
					}
					else {
						FileLogger.log (FileLogger.LogLevelType.Combat, "");
					}
					break;					
				case DungeonState.MonsterAction:
					activeActionIndex = -1;

					ActionProgress monsterActionResult = DM.DoMonsterAction (activeActionIndex);
					if (monsterActionResult != ActionProgress.Error) {							
						WaitProcessCombatResult (DungeonState.MonsterAction);
					} else {
						FileLogger.log (FileLogger.LogLevelType.Combat, "");
					}
					break;				
				case DungeonState.PlayerAction:
					WaitForPlayerAction ();

					ActionInput replayInput = _replayInfo.GetInput (_playerActionCount);

					if (_combatLog.dungeonType == CombatLogDungeonType.Abyss && replayInput == null) { //check end abyss
						if (CheckVaild () == false) {
							DungeonFailProcess ();
							DM.GameOverDungeon ();
							break;
						}

						DM.State = DungeonState.Finish;
						continue;
					}

					if (replayInput.ActionIndex == DungeonReplay.DUNGEON_CONTINUE_INDEX) {
						ExecuteDungeonContinue ();
						_playerActionCount++;
						continue;
					}

					if(replayInput.ActionIndex == DungeonReplay.GUILD_RAID_JOIN_INDEX) {
						GuildRaidActionJoin (replayInput);

						_playerActionCount++;
						continue;
					}

					if (replayInput.ActionIndex == DungeonReplay.RAID_DAMAGE_ACTION_INDEX) {

						foreach (Monster monster in DM.MonsterList) {
							if (monster.UnitIndex == replayInput.TargetIndex) {
								if (replayInput.Value > 0)
									AddLog (string.Format ("raid damage event : {0} damage : {1}", monster.Name, replayInput.Value));
								monster.ChangeHP (replayInput.Value);
								break;
							}
						}

						_playerActionCount++;
						continue;
					}

					if (replayInput.ActionIndex == DungeonReplay.RAID_END_ACTION_INDEX) {

						if (CheckVaild () == false) {
							DungeonFailProcess ();
							DM.GameOverDungeon ();
							break;
						}

						DM.State = DungeonState.Finish;
						continue;
					}

					//check target valid
					/*if (replayInput.TargetIndex != -1 && replayInput.TargetIndex > (int)UnitIndexType.CharacterEnd) {
						IUnit targetUnit = DM.GetUnit (replayInput.TargetIndex);
						if (targetUnit == null || targetUnit.UnitState == UnitState.Dead) {
							DM.State = DungeonState.FinishStage;
							DM.FinishStage ();
							continue;
						}
					}
					*/

					//check character switch (for guildraid)
					if (replayInput.ActionIndex >= DungeonReplay.CHARACTER_SWITCH_INDEX && 
						replayInput.ActionIndex < DungeonReplay.CHARACTER_SWITCH_INDEX+10) {
						GuildRaidCharacterSwitch (replayInput);
					} else {
						ActionInput actionInput = new ActionInput (DM.GetCurUnit (), replayInput.ActionIndex, replayInput.TargetIndex);

						ActionProgress ret = DM.DoPlayerAction (actionInput);
						if (ret != ActionProgress.Error) {
							WaitProcessCombatResult (DungeonState.PlayerAction);
						} else {
							FileLogger.log (FileLogger.LogLevelType.Combat, "");
						}
					}
					_playerActionCount++;
					break;
				case DungeonState.FinishTurn:
					DM.FinishTurn();
					break;
				case DungeonState.FinishStage:		
					AddLog ("FinishStage.\n");
						
					DM.FinishStage();
					SetTurnList(false);
					break;
				case DungeonState.Finish:
					DungeonFinishProcess();
					DM.FinishDungeon();
					break;
				case DungeonState.GameOver:
					ActionInput continueInput = _replayInfo.GetInput (_playerActionCount);
					if (continueInput != null) {
						if (continueInput.ActionIndex == DungeonReplay.DUNGEON_CONTINUE_INDEX) {
							ExecuteDungeonContinue ();
							_playerActionCount++;
							continue;
						}
					}
					DungeonGameOverProcess ();
					DM.GameOverDungeon ();
					break;
				default:
					break;
				}
			}

			return true;
		}

		private bool WaitProcessCombatResult(DungeonState dungeonState)
		{
			DungeonManager DM = DungeonManager.Instance;
			if (DM == null)
				return false;

			CombatLogNode resultNode = DM.GetLastCombatLog();

			if(resultNode != null)
			{
				DM.DungeonResult.ShowLog (this);
				DM.DungeonResult.FinishNode();
			}
			return true;
		}

		private void SetTurnList(bool updateCurrentActionUnit)
		{
			if (DM == null) { return; }

			activeActionIndex = -1;
		}

		int GetAliveMonsterCount()
		{
			if (DM == null) { return -1; }

			int count = 0;
			foreach (Monster monster in DM.MonsterList)
			{
				if (monster != null && monster.IsAlive())
				{
					count++;
				}
			}
			return count;
		}

		public int GetStageTurn()
		{	
			if (DM == null) { return -1; }

			return DM.CurTurn - _stageTurn + 1;
		}

		private void InitPlayerAction()
		{
			activeActionIndex = -1;
		}

		private bool WaitForPlayerAction()
		{
			InitPlayerAction();
			return true;
		}

		private bool GuildRaidCharacterSwitch(ActionInput actionInput)
		{
			DM.GuildRaidSwitchCombatCharacter (actionInput);

			CombatLogNode resultNode = DM.GetLastCombatLog();
			if(resultNode != null)
			{
				DM.DungeonResult.ShowLog (this);
				DM.DungeonResult.FinishNode();
			}
			return true;
		}

		private bool GuildRaidActionJoin(ActionInput actionInput)
		{
			if (actionInput.TargetIndex <= 0)
				return false;
			
			//event log
			RaidJoinCharacterLogNode raidJoinLogNode = new RaidJoinCharacterLogNode(DungeonLogType.RaidJoin, actionInput.TargetIndex, "", "");

			DM.OnGuildRaidJoin (actionInput, raidJoinLogNode);

			DM.DungeonResult.LastLog = raidJoinLogNode;
			DM.DungeonResult.AddNode (raidJoinLogNode);
			CombatLogNode resultNode = DM.GetLastCombatLog();
			if(resultNode != null)
			{
				DM.DungeonResult.ShowLog (this);
				DM.DungeonResult.FinishNode();
			}

			return true;
		}

		private bool CheckVaild()
		{
			int damage = 0;
			foreach (Character character in DM.OriginCharList) {
				damage += character.AccDamage;
			}
			foreach (Character character in DM.SubCharList) {
				damage += character.AccDamage;
			}

			if (_replayInfo.Damage != damage) {

				if (_combatLog.state == "done") {
					if (damage > _replayInfo.Damage)
						return true;
				}

				string strMsg = string.Format ("{0} != {1}", _replayInfo.Damage, damage);
				AddLog ("fail CheckVaild : " + strMsg);
				return false;
			}
			return true;
		}

		private void DungeonGameOverProcess()
		{
			if (_combatLog.dungeonType == CombatLogDungeonType.Dungeon)
				_battleResult = "gameover";
			else
				_battleResult = "valid";

			isRunning = false;

			AddLog (_battleResult);
		}
			
		private void DungeonFinishProcess()
		{
			_battleResult = "valid";

			isRunning = false;

			AddLog (_battleResult);
		}

		private void DungeonFailProcess()
		{
			_battleResult = "invalid";

			isRunning = false;

			AddLog (_battleResult);
		}

		private void ExecuteDungeonContinue()
		{
			if (DM != null)
			{
				AddLog ("ExecuteDungeonContinue");
				DM.ContinueDungeon(true);
			}
		}

		public void UpdateUnitHP(int targetIndex, DungeonLogType logType, int amount, float nodeDelay)
		{
		}
		public void OnUnitCombatLogEvent(int targetIndex, DungeonLogType logType, float nodeDelay)
		{
		}
		public void OnGuildRaidActionLogEvent(DungeonLogType logType, int characterCode, string userId, string nickname, float nodeDelay)
		{
		}
		public void ShowSCT(List<SpecSkillEffectObjectInfo> effectList, int casterIndex, int targetIndex, TextUIMessage msg, DungeonLogType logType, ref float nodeDelay)
		{
		}
		public float ShowSkillEffect(List<SpecSkillEffectObjectInfo> effectObjectInfo, int casterIndex, int targetIndex, ref float nodeDelay, EffectType[] filterTypes)
		{
			return 0f;
		}
		public float ShowSkillEffect(SpecSkillEffectObjectInfo effectObjectInfo, int casterIndex, int targetIndex, ref float nodeDelay, EffectType[] filterTypes)
		{
			return 0f;
		}
		public void OnCompleteTween(float nodeDelay, int unitIndex, StatusEffectCode effectCode, IUnit owner)
		{
		}

		public void SetOneLineExplanation(CombatLogNode logNode, float delay)
		{
		}

		public void AddLog(string logText)
		{
			//remove ngui tag
			logText = logText.Replace ("[D94E34]", "");
			logText = logText.Replace ("[71B627]", "");
			logText = logText.Replace ("[-]", "");

			_combatLog.replayLog += logText;
			FileLogger.log (FileLogger.LogLevelType.Combat, logText);
		}

		public void AddSummaryLog(string logText)
		{
			//remove ngui tag
			logText = logText.Replace ("[D94E34]", "");
			logText = logText.Replace ("[71B627]", "");
			logText = logText.Replace ("[-]", "");

			_combatLog.replaySummaryLog += logText;
			FileLogger.log (FileLogger.LogLevelType.Combat, logText);
		}
	}
}


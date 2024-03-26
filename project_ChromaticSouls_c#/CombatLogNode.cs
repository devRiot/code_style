using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using Corgi;
using Corgi.Spec;

namespace Corgi.GameLogic
{
    [Serializable]
	public class CombatLogNode 
	{
		protected List<CombatLogNode> _childs = new List<CombatLogNode>();

		protected DungeonLogType _logType;
		CombatLogNode _parent;
		bool _isLogged = false;
		bool _isRaidLogged = false;

		public string KeyName { get { return _logType.ToString(); } }
		public CombatLogNode Parent { get { return _parent; } set { _parent = value; } }
		public virtual ActionInput ActionInput
		{
			get
			{
				if (Parent != null)
				{
					ActionInput input = Parent.ActionInput;
					if(input != null)
					{
						return input;
					}
				}

				return null;
			}
		}

		public List<CombatLogNode> Childs { get { return _childs; } }
		public CombatLogNode LastChild 
		{ 
			get 
			{ 
				if(Childs.Count > 0)
				{
					return _childs[_childs.Count-1]; 
				}else
				{
					return null;
				}
			} 
		}
		public int ChildCount { get { return _childs.Count; } }

		public virtual bool IsCombatMessage { get { return true; } }

		public CombatLogNode(DungeonLogType logType)
		{
			_logType = logType;
		}

		public void AddResult(CombatLogNode child)
		{
			_childs.Add(child);
			child.Parent = this;
		}

		public virtual string GetDesc()
		{
			string baseDesc = CorgiStaticStringData.Instance.GetString(LangStringType.combat, CorgiSpecHelper.ConvertToString(_logType.ToString()));

			if(baseDesc == null)
			{
				return null;
			}
			return baseDesc;
		}

		public void ShowLog(ICombatUIInterface manager,  ref string logText)
		{
			if(_isLogged == true)
			{
				return;
			}

            //CombatSceneManager sceneManager = GamePhaseManager.Instance.GetSceneManager() as CombatSceneManager;
            if (manager != null)
            {
                if(this.IsCombatMessage == true)
                {
                    //sceneManager.AddOneLineExplanation(desc);
                    manager.SetOneLineExplanation(this, 5f);
                }
            }

			string desc = GetDesc();
			if(desc != null)
			{
				logText += desc + "\n";
			}

 			foreach(CombatLogNode child in _childs)
 			{
 				child.ShowLog(manager, ref logText);
 			}

			_isLogged = true;
		}

		public virtual float ProcessLogNodeEffect(ICombatUIInterface manager, ref float delay)
		{
			return delay;
		}

		public virtual void ProcessLogNodeHPUpdate(ICombatUIInterface manager, int amount, DungeonLogType logType, float nodeDelay)
		{
		}

        public virtual void ProcessLogNodeText(ICombatUIInterface manager, TextUIMessage msg, DungeonLogType logType, ref float nodeDelay, int amount = 0) 
		{
			DebugUtils.LogWarning("      ++ ProcessLogNodeText - nodeDelay : " + nodeDelay + " / type : " + logType + " / text : " + msg.strFrom+msg.strMessage+msg.strAddition);
		}

		public virtual float ProcessEffect(ICombatUIInterface manager, ref float nodeDelay, int depthCount)
		{
			//float curNodeDelay = nodeDelay;
			float thisRetDelay = ProcessLogNodeEffect(manager, ref nodeDelay);

			//Debug.LogWarning(depthCount + ".    ++ ProcessEffect - retDelay : " + thisRetDelay + " / nodeDelay : " + nodeDelay + " / type : " + _logType);
            CombatLogNode preNode = null;
            float childNodeDelay = nodeDelay;

			foreach (CombatLogNode node in _childs)
			{
				if (node != null)
				{
                    float curRetDelay = 0.0f;

                    SkillCompLogNode preSkillNode = preNode as SkillCompLogNode;
                    SkillCompLogNode curSkillNode = node as SkillCompLogNode;
                    
                    if(preSkillNode != null && curSkillNode != null && System.Object.ReferenceEquals(preSkillNode.SkillComp, curSkillNode.SkillComp) == false)
                    {
                        nodeDelay = childNodeDelay;
                    }

                    childNodeDelay = nodeDelay;

                    curRetDelay = node.ProcessEffect(manager, ref childNodeDelay, depthCount + 1);

					thisRetDelay = Math.Max(thisRetDelay, curRetDelay);

                    preNode = node;
				}
			}

			return thisRetDelay;
		}

		public JSONObject Serialize()
		{
			JSONObject thisObject = new JSONObject();

			thisObject.AddField("keyName", KeyName);
			//thisObject.AddField("desc", this.GetDesc()); // debugging 용도로 추가.

			SerializeNode(thisObject);

			JSONObject childs = null;
			foreach (CombatLogNode node in _childs)
			{
				if (childs == null)
				{
					childs = new JSONObject(JSONObject.Type.ARRAY);
				}

				JSONObject childObject = node.Serialize();

				childs.Add(childObject);
			}

			if (childs != null)
			{
				thisObject.AddField("childs", childs);
			}

			return thisObject;
		}

		public void SerializeRaidLog(ref JSONObject rootObject)
		{
			JSONObject thisObject = null;
			
			if(_isRaidLogged == false)
			{
				if((_logType == DungeonLogType.Damage 
	                || _logType == DungeonLogType.CriticalDamage
	                || _logType == DungeonLogType.DamageByContinuous
	                || _logType == DungeonLogType.CriticalDamageByContinuous))
				{
					thisObject = new JSONObject();
	
					thisObject.AddField("keyName", "Damage");
					thisObject.AddField("turn", DungeonManager.Instance.CurTurn);
					
					SerializeNode(thisObject);
	
					int targetIndex = CorgiJson.ParseInt(thisObject, "targetIndex");
					if(targetIndex >= (int)UnitIndexType.Monster && targetIndex <= (int)UnitIndexType.MonsterEnd)
					{
						rootObject.Add(thisObject);
					}
	                _isRaidLogged = true;
				}
			}
			
			foreach (CombatLogNode node in _childs)
			{
				node.SerializeRaidLog(ref rootObject);
			}
		}

		protected virtual void SerializeNode(JSONObject jsonObject)
		{
		}

		public virtual void DeSerialize(JSONObject json)
		{
		}

		public bool IsValid()
		{
			return Childs.Count > 0;
		}

		public bool FindLogTypeInChildren(DungeonLogType logType)
		{
			if (_logType == logType)
			{
				return true;
			}
			else
			{
				foreach (CombatLogNode logNode in _childs)
				{
					if (logNode != null)
					{
						if (logNode.FindLogTypeInChildren(logType)) { return true; }
					}
				}
			}
			return false;
		}

		public void OnDestroy()
		{
			if(null!=_parent)
			{
				_parent = null;
			}
			if(null!=_childs)
			{
				foreach(CombatLogNode n in _childs)
				{
					n.OnDestroy();
				}
				_childs.Clear();
				_childs = null;
			}

		}
	}
}
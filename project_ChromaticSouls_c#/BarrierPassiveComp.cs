
using System;
using System.Collections.Generic;
using Corgi;
using Corgi.Spec;


namespace Corgi.GameLogic
{
    [System.Serializable]
	public class BarrierPassiveComp : PassiveSkillComp 
	{
		int _amount = 0;
		int _maxAmount = 0;
		ActiveAmountType _amountType;
		StatType _statType;

		public int Amount { get{return _amount; }}
		public int MaxAmount { get { return _maxAmount; } }
        public float AmountAbsolute 
        { 
            get 
            { 
                if(_skillInst == null)
                {
                    return (float)(_spec.baseAbsolute);
                }

                return (float)(_spec.baseAbsolute + _spec.levelModifier * _skillInst.Level); 
            }
 
        } 
		public float AmountFactor { get { return (float)_spec.param2; }}
		public float AbsorbRate { get { return (float)_spec.param3; } }

		public BarrierPassiveComp(IUnit caster, SkillInst skillInst)
			: base(caster, skillInst)
		{
	
		}

		protected override void InitGameObject ()
		{
			base.InitGameObject ();

			_eventManager.Register(CombatEventType.OnEnter, this.OnEnter);
			_eventManager.Register(CombatEventType.OnBeingHitAlways, this.OnDamaged);
		}

		public override bool Load(long code)
		{
			if(base.Load(code) == false)
			{
				return false;
			}

			_amountType = (ActiveAmountType)CorgiSpecHelper.ParseEnum(typeof(ActiveAmountType), _spec.param0);
			_statType = (StatType)CorgiSpecHelper.ParseEnum(typeof(StatType), _spec.param1);

			return true;
		}

		public override bool InitState()
		{
			if(base.InitState() == false)
			{
				return false;
			}

			float factor = 0;
			if(_statType == StatType.AttackPower)
			{
				factor = AmountAbsolute + Caster.AttackPower * AmountFactor;

			}else if(_statType == StatType.SpellPower)
            {
				factor = AmountAbsolute + Caster.SpellPower * AmountFactor;
            }
            else
			{
                factor = AmountAbsolute + AmountFactor;
			}

            float newFactor = _owner.GetAmountValue(this, factor);

			_amount = (int)(newFactor);
			_maxAmount = _amount;

			return true;
		}
		bool OnEnter(IEventParam eventParam, CombatLogNode logNode)
        {
			float factor = 0;

            if(_statType == StatType.HigherPower)
            {
                if(_owner.AttackPower >= _owner.SpellPower)
                {
                    _statType = StatType.AttackPower;
                }else
                {
                    _statType = StatType.SpellPower;
                }
            }

			if(_statType == StatType.AttackPower)
			{
				factor = AmountAbsolute + Caster.AttackPower * AmountFactor;

			}else if(_statType == StatType.SpellPower)
            {
				factor = AmountAbsolute + Caster.SpellPower * AmountFactor;
            }
            else
			{
                factor = AmountAbsolute + AmountFactor;
			}

            float newFactor = _owner.GetAmountValue(this, factor);

			_amount = (int)(newFactor);

            return false;
        }

		bool OnDamaged(IEventParam eventParam, CombatLogNode logNode)
		{
			int absorbAmount = 0;

			EventParamSkillOutput skillEventParam = eventParam as EventParamSkillOutput;

			if(skillEventParam == null)
			{
				return false;
			}

			SkillOutputActive skillOutput = skillEventParam.Output as SkillOutputActive;
			
			if(skillOutput == null)
			{
				return false;
			}

			if(_amountType == ActiveAmountType.None || _amountType == skillOutput.AmountType)
			{
				absorbAmount = skillOutput.Absorb(_amount, AbsorbRate);

				_amount -= absorbAmount;

				if(_amount <= 0)
				{
					SkillInst.State = SkillInstState.Dead;
				}
 
				AbsorbDamageLogNode newNode = new AbsorbDamageLogNode(DungeonLogType.AbsorbDamage, SkillInst, this, absorbAmount);
				logNode.AddResult(newNode);
			}

			return false;
		} 

        protected override string GetPassiveSCT(string key)
        {
            if(SCTVisible == false || key == null)
            {
                return null;
            }

			string sctStr = CorgiStaticStringData.Instance.GetString(LangStringType.SCT, key);

            if(sctStr == null)
            {
                return null;
            }

            sctStr = CorgiString.Format("{0}{1}({2}){3}", CorgiColor.BBC_COLOR_GREEN, sctStr, _amount, CorgiColor.BBC_COLOR_END);

            //sctStr += "(" + _amount + ")";
            //sctStr = CorgiColor.BBC_COLOR_GREEN + sctStr + CorgiColor.BBC_COLOR_END;

            return sctStr;
        }

        public override string GetSCTKey()
        {
            return "Barrier";
        }
	}
}

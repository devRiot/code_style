#include "PlayerSkillInvoker.h"

#include "SkillAction.h"
#include "ParticleManager.h"
#include "Animation.h"

//common include
#include "SVLog.h"
#include "SVInternal.h"

PlayerSkillInvoker::PlayerSkillInvoker(void)
{
	_bStateCharging = false;
	_bChared = false;
}

PlayerSkillInvoker::~PlayerSkillInvoker(void)
{
}

bool PlayerSkillInvoker::JustTest()
{
	//저스트판정은 플레이어가 액티브형태의 스킬을 발동중일때만 가능하다.
	//하지만 Invoke상태=스킬발동 중 이라는 보장이 없기때문에 좀더 개선이 필요함.
	if(_Owner->HasAction(ActionTypes::Invoke))
	{
		//현재 발동중인 스킬이 저스트타임인가?
		if(_PrevInvokedSkillAction->IsJustTime())
		{
			//ParticleProxy()->createEffect(L"저스트_성공", _Owner->GetPosition());

			OnCancelSkillAction(_PrevInvokedSkillAction);

			System::log << L"저스트 성공!" << sv::endl;
			return true;
		}

		_PrevInvokedSkillAction->SetJustFailed();
		System::log << L"저스트 실패!" << sv::endl;
		return false;
	}

	return true;
}

bool PlayerSkillInvoker::InvokeSkill(Skill* skill)
{
	//인보커상태가 현재 차징중인가?
	if(_bStateCharging)
	{
		if(_bChared && skill->GetSkillId() == _ChargedSkill->GetSkillId())
		{
			_bStateCharging = false;
			_bChared = false;

			OnInvokeSkillAction(skill);
			return true;
		}
		else
		{
			//차징실패
			_bChared = false;
			_bStateCharging = false;
			_Owner->ChangeState(L"Idle");
			return false;
		}
	}

	//저스트 검사를 수행한다.
	if(!JustTest())
		return false;

	if(skill->GetSkillParam()._InvokeType == eInvokeType_Immediately)
	{
		OnInvokeSkillAction(skill);
		return true;
	}

	if(skill->GetSkillParam()._InvokeType == eInvokeType_Charge)
	{
		_bStateCharging = true;
		_ChargeTime		= 0.25f;
		_ChargeTimer	= 0.f;
		_ChargedSkill	= skill;

		//해당 스킬액션의 차징모션처리.
		OnChargeSkillAction(skill);
		_Owner->ChangeState(L"Charge");
		return false;
	}

	return true;
}

void PlayerSkillInvoker::Update(float elapsedTime)
{
	SkillInvoker::Update(elapsedTime);

	if(_bStateCharging)
	{
		_ChargeTimer += elapsedTime;

		if(!_bChared && _ChargeTimer >= _ChargeTime)
		{
			_bChared = true;
			ParticleProxy()->createEffect(L"저스트_성공", _Owner->GetPosition());
		}
	}
}
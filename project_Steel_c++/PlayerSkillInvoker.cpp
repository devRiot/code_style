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
	//����Ʈ������ �÷��̾ ��Ƽ�������� ��ų�� �ߵ����϶��� �����ϴ�.
	//������ Invoke����=��ų�ߵ� �� �̶�� ������ ���⶧���� ���� ������ �ʿ���.
	if(_Owner->HasAction(ActionTypes::Invoke))
	{
		//���� �ߵ����� ��ų�� ����ƮŸ���ΰ�?
		if(_PrevInvokedSkillAction->IsJustTime())
		{
			//ParticleProxy()->createEffect(L"����Ʈ_����", _Owner->GetPosition());

			OnCancelSkillAction(_PrevInvokedSkillAction);

			System::log << L"����Ʈ ����!" << sv::endl;
			return true;
		}

		_PrevInvokedSkillAction->SetJustFailed();
		System::log << L"����Ʈ ����!" << sv::endl;
		return false;
	}

	return true;
}

bool PlayerSkillInvoker::InvokeSkill(Skill* skill)
{
	//�κ�Ŀ���°� ���� ��¡���ΰ�?
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
			//��¡����
			_bChared = false;
			_bStateCharging = false;
			_Owner->ChangeState(L"Idle");
			return false;
		}
	}

	//����Ʈ �˻縦 �����Ѵ�.
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

		//�ش� ��ų�׼��� ��¡���ó��.
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
			ParticleProxy()->createEffect(L"����Ʈ_����", _Owner->GetPosition());
		}
	}
}
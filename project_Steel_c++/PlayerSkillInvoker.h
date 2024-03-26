#pragma once
#include "SkillInvoker.h"
#include "BasicSkillActionFactory.h"

class PlayerSkillInvoker : public SkillInvoker
{
public:
	PlayerSkillInvoker(void);
	virtual ~PlayerSkillInvoker(void);

private:
	bool	_bStateCharging;
	bool	_bChared;
	float	_ChargeTime, _ChargeTimer;
	Skill*	_ChargedSkill;

public:
	virtual void Update(float elapsedTime);
	virtual bool InvokeSkill(Skill* skill);
	virtual bool JustTest();

	bool IsSkillCharging() { return _bStateCharging; }
};

#ifndef __PLAYERMAGAZINE_H__
#define __PLAYERMAGAZINE_H__

#include "FireControlSystem.h"

class Animation;

class NeedleTrapMag : public Magazine
{
    DECLARE_SVOBJECT(NeedleTrapMag)

public:
    virtual FireType GetFireType() const { return FireTypes::Ground; }
    virtual AfterEffectType GetAfterEffect() const { return AfterEffectTypes::Damage | AfterEffectTypes::Upper; }
    virtual RemoveTimingType GetRemoveTiming() const { return RemoveTimingTypes::TimeOver | RemoveTimingTypes::Hit; }

    NeedleTrapMag() {}
    virtual ~NeedleTrapMag() {}

    virtual void Update(float elapsedTime);    
};

class NormalSlashSingle : public Magazine
{
    DECLARE_SVOBJECT(NormalSlashSingle)

public:
    virtual FireType GetFireType() const { return FireTypes::Ground; }
    virtual AfterEffectType GetAfterEffect() const { return AfterEffectTypes::Damage; }
    virtual RemoveTimingType GetRemoveTiming() const { return RemoveTimingTypes::TimeOver | RemoveTimingTypes::Hit; }

    NormalSlashSingle() {}
    virtual ~NormalSlashSingle() {}

    virtual void Update(float elapsedTime);    
};

class NormalSlashMulti : public Magazine
{
    DECLARE_SVOBJECT(NormalSlashMulti)

public:
    virtual FireType GetFireType() const { return FireTypes::Ground; }
    virtual AfterEffectType GetAfterEffect() const { return AfterEffectTypes::Damage; }
    virtual RemoveTimingType GetRemoveTiming() const { return RemoveTimingTypes::TimeOver | RemoveTimingTypes::Hit; }

    NormalSlashMulti() {}
    virtual ~NormalSlashMulti() {}

    virtual void Update(float elapsedTime);    
};

class UpperSlash : public NormalSlashMulti
{
    DECLARE_SVOBJECT(UpperSlash)

public:
    virtual FireType GetFireType() const { return FireTypes::Ground; }
    virtual AfterEffectType GetAfterEffect() const { return AfterEffectTypes::Damage | AfterEffectTypes::Upper; }
    virtual RemoveTimingType GetRemoveTiming() const { return RemoveTimingTypes::TimeOver | RemoveTimingTypes::Hit; }

    UpperSlash() {}
    virtual ~UpperSlash() {}

    virtual void Update(float elapsedTime);
};

class SlashWave : public Magazine
{
    DECLARE_SVOBJECT(SlashWave)

public:
    virtual FireType GetFireType() const { return FireTypes::Projectile; }
    virtual AfterEffectType GetAfterEffect() const { return AfterEffectTypes::Damage | AfterEffectTypes::Upper; }
    virtual RemoveTimingType GetRemoveTiming() const { return RemoveTimingTypes::OutOfArea | RemoveTimingTypes::TimeOver | RemoveTimingTypes::Hit; }

    SlashWave();
    virtual ~SlashWave();

    virtual void Serialize(Archive &ar);
    virtual void Update(float elapsedTime);

    DirectionType   _direction;
    Animation      *_animation;
    bool            _delayPlay;
};

class BombItemMag : public Magazine
{
	DECLARE_SVOBJECT(BombItemMag)

public:
	virtual FireType GetFireType() const { return FireTypes::Ground; }
	virtual AfterEffectType GetAfterEffect() const { return AfterEffectTypes::Damage | AfterEffectTypes::Upper; }
	virtual RemoveTimingType GetRemoveTiming() const { return RemoveTimingTypes::TimeOver | RemoveTimingTypes::Hit; }

	BombItemMag() {}
	virtual ~BombItemMag() {}

	virtual void Update(float elapsedTime);    
};

#endif //__PLAYERMAGAZINE_H__
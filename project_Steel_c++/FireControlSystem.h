#ifndef __FIRECONTROLSYSTEM_H__
#define __FIRECONTROLSYSTEM_H__

#include "SVTypeDef.h"
#include "SVObject.h"
#include "TypeDef.h"
#include <Windows.h>
#include <d3dx9.h>
#include <map>
#include <vector>
#include <string>
#include "Ref.h"
#include "Unit.h"

USING_SVFRAMEWORK
using namespace UnitTypes;
using namespace FCSTypes;

class Unit;
class Magazine : public sv::Object
{
    DECLARE_SVOBJECT(Magazine)

public:
    virtual void Serialize(Archive &ar);
    virtual Unit * GetUnit() const { return _unit; }

    virtual FireType GetFireType() const { return FireTypes::Ground; } // 발생타입: 고정형, 발사형...
    virtual AfterEffectType GetAfterEffect() const { return AfterEffectTypes::None; } // 타격시 타입: 데미지, 띄우기, 경직 등등
    virtual RemoveTimingType GetRemoveTiming() const { return RemoveTimingTypes::TimeOver; } // 삭제 조건

    Magazine(){}
    virtual ~Magazine() {}

    virtual void Update(float elapsedTime) = 0;
    
    virtual bool IsEmpty() { return _isEmpty; }

protected: // 사용자 지정 데이터 영역
    uint32_t        _fireTime; // 지속시간
    uint32_t        _fireIdleTime;
    RECT            _targetArea; // 충돌영역
    D3DXVECTOR3     _position; // 출력위치(중점을 중앙으로?)

protected: // 고유 데이터 영역
    bool            _isEmpty; // 탄창이 비었으면 FCS에서 제거
    //Unit            *_unit;
	RefT<Unit>		_unit;
    float           _accumDelta;
};

class FireControlSystem
{
public:
	FireControlSystem();
	virtual ~FireControlSystem();

    virtual void Update(float elapsedTime);
    virtual bool Fire(const std::wstring &name, Archive &ar);

private:
    typedef std::vector<Magazine *> Magazines;
    Magazines   _fired;
    Magazines   _empty;

};

#endif //__FIRECONTROLSYSTEM_H__
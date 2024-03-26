#ifndef __EVENTTRIGGER_H__
#define __EVENTTRIGGER_H__

#include <d3dx9.h>
#include <sigc++/sigc++.h>
#include <vector>
#include "Movable.h"
#include "TypeDef.h"
#include "SVTypeDef.h"

#define ON_TRIGGER_ENTERED(func) triggerEntered.connect(sigc::mem_fun(*this, func))
#define ON_TRIGGER_MOVED(func) triggerMoved.connect(sigc::mem_fun(*this, func))
#define ON_TRIGGER_LEAVED(func) triggerLeaved.connect(sigc::mem_fun(*this, func))

USING_SVFRAMEWORK

class Unit;
class TriggerObject
{
public:
    TriggerObject(const EventTriggerTimingType &type);
    virtual ~TriggerObject() {}

    void Serialize(Archive &ar);

    void Update(float elapsedTime);
    bool HitTest(int x, int y);
    bool IsPulled() const { return _pulled; }

public:
    sigc::signal<bool, const std::wstring &, Unit *, const D3DXVECTOR3 &> triggerEntered;
    sigc::signal<bool, const std::wstring &, Unit *, const D3DXVECTOR3 &> triggerMoved;
    sigc::signal<bool, const std::wstring &, Unit *, const D3DXVECTOR3 &> triggerLeaved;

private:
    EventTriggerTimingType _type;
    std::wstring    _name;
    Movable         _position; //트리거 다각형 영역

    bool            _pulled; //트리거가 당겨졌나?
    bool            _entered;
};

class EventTrigger
{
public:
	EventTrigger();
	virtual ~EventTrigger();

    void AddTrigger(TriggerObject * trigger);
    void Update(float elapsedTime);

private:
    typedef std::vector<TriggerObject *> Triggers;
    Triggers _triggers;
    Triggers _pulled;
};

#endif //__EVENTTRIGGER_H__
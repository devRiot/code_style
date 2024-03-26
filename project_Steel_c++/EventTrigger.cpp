#include "EventTrigger.h"

#include "SVLog.h"
#include "SVInternal.h"
#include "Unit.h"
#include "Map.h"
#include "World.h"
#include <algorithm>


TriggerObject::TriggerObject(const EventTriggerTimingType &type) :
                _type(type), _position(MovableTypes::Trigger),
               _pulled(false), _entered(false)
{

}

void TriggerObject::Serialize(Archive &ar)
{
    if (ar.IsLoading())
    {
        ar >> _name;
        _position.Serialize(ar);
    }
}

void TriggerObject::Update(float elapsedTime)
{
    if (!_pulled)
    {
        Unit * player = WorldProxy()->GetPlayer();
        if (player)
        {
            D3DXVECTOR3 pos = player->GetPosition();

            bool entered = HitTest(int(pos.x), int(pos.y));

            if (entered)
            {
                if (!_entered) // 처음 들어왔다.
                {
                    _entered = true;
                    // 엔터 이벤트 발생
                    if (_type & EventTriggerTimingTypes::Entered)
                        _pulled = triggerEntered.emit(_name, player, pos);
                }
                else // 이미 들어와 있음
                {
                    // 무브 이벤트 발생
                    if (_type & EventTriggerTimingTypes::Moved)
                        _pulled = triggerMoved.emit(_name, player, pos);
                }
            }
            else
            {
                if (_entered) // 밖으로 나갔다
                {
                    _entered = false;
                    // 리브 이벤트 발생
                    if (_type & EventTriggerTimingTypes::Leaved)
                        _pulled = triggerLeaved.emit(_name, player, pos);
                }
            }
        }
    }
}

bool TriggerObject::HitTest(int x, int y)
{
    if (_position.PtInSide(Movable::PointI(x, y)))
        return true;

    return false;
}

//__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/__/

EventTrigger::EventTrigger()
{
    System::log << L"EventTrigger Start" << endl;
}

EventTrigger::~EventTrigger()
{
    for (std::vector<TriggerObject *>::iterator i = _triggers.begin(); i != _triggers.end(); ++i)
        delete (*i);
    _triggers.clear();
}

void EventTrigger::AddTrigger(TriggerObject * trigger)
{
    System::log << L"EventTrigger Added" << endl;
    _triggers.push_back(trigger);
}

void EventTrigger::Update(float elapsedTime)
{
    for (Triggers::iterator i = _triggers.begin(); i != _triggers.end(); ++i)
    {
        if ((*i)->IsPulled())
        {
            _pulled.push_back((*i));
        }
        else
        {
            (*i)->Update(elapsedTime);
        }
    }

    for (Triggers::iterator i = _pulled.begin(); i != _pulled.end(); ++i)
    {
        Triggers::iterator it = std::find(_triggers.begin(), _triggers.end(), (*i));
        if (it != _triggers.end())
        {
            SV_DELETE((*it));
            _triggers.erase(it);
        }
    }
    _pulled.clear();
}
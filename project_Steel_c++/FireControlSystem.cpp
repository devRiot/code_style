#include "FireControlSystem.h"

#include "SVLog.h"
#include "SVInternal.h"
#include <algorithm>
#include "Unit.h"

FireControlSystem::FireControlSystem()
{
}

FireControlSystem::~FireControlSystem()
{
    for (Magazines::iterator i = _fired.begin(); i != _fired.end(); ++i)
    {
        SV_DELETE((*i));
    }

    _fired.clear();
}

void FireControlSystem::Update(float elapsedTime)
{
    for (Magazines::iterator i = _fired.begin(); i != _fired.end(); ++i)
    {
		Unit * unit = (*i)->GetUnit();
        
		if (unit)
        {
            if ((*i)->IsEmpty() || (*i)->GetUnit()->HasAction(ActionTypes::Damaged))
                _empty.push_back((*i));
            else
                (*i)->Update(elapsedTime);
        }
        else
        {
            if ((*i)->IsEmpty())
                _empty.push_back((*i));
            else
                (*i)->Update(elapsedTime);
        }
    }

    for (Magazines::iterator i = _empty.begin(); i != _empty.end(); ++i)
    {
        Magazines::iterator it = std::find(_fired.begin(), _fired.end(), (*i));
        if (it != _fired.end())
        {
            SV_DELETE((*it));
            _fired.erase(it);
        }
    }
    _empty.clear();
}

bool FireControlSystem::Fire(const std::wstring &name, Archive &ar)
{
    //System::log << L"FCS: "<< name.c_str() << L" fired" << sv::endl;

    std::vector<uint8_t> stream;
    Archive arStore(stream, Archive::store);
    Archive arLoad(stream, Archive::load);

    arStore << name;
    arStore << ar;

    Magazine * magazine = static_cast<Magazine *>(System::object->BuildSerializableObject(arLoad));
    
    _fired.push_back(magazine);

    return true;
}

void Magazine::Serialize(Archive &ar)
{
    if (ar.IsLoading())
    {
        _isEmpty = false;
        _accumDelta = 0.0f;

        uint64_t p;
        ar >> p;
        _unit = (Unit *)p;
        ar >> _fireTime;
        ar >> _fireIdleTime;
        ar >> _targetArea.left >> _targetArea.top >> _targetArea.right >> _targetArea.bottom;
        ar >> _position.x >> _position.y >> _position.z;
    }
}
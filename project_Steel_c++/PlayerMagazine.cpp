#include "PlayerMagazine.h"

#include "Camera.h"
#include "Unit.h"
#include "World.h"
#include "SVArchive.h"
#include "SVLog.h"
#include "Renderer.h"
#include "SVInternal.h"
#include "Animation.h"
#include "ResourceManager.h"
#include "MessageSender.h"
#include "DrawTool.h"


void NeedleTrapMag::Update(float elapsedTime)
{
    uint32_t fc = (uint32_t)(_accumDelta * 1000.0f + 0.5f);
    if (_fireIdleTime > fc)
    {
        _accumDelta += elapsedTime;
        return;
    }

    if (_fireTime + _fireIdleTime >= fc)
    {
        _accumDelta += elapsedTime;

        {
            RECT difference;
            DirectionType repulsive;
            Unit * target = NULL;

            g_DrawTool.drawBox(_position, _targetArea);
            g_DrawTool.drawPoint(_position);

            std::vector<Unit *> dest;
            if (WorldProxy()->BoundCheck(CharacterTypes::Player, dest, _position, _targetArea, repulsive, difference))
            {
                std::vector<uint8_t> stream;
                Archive arStore(stream, Archive::store);
                Archive ar(stream, Archive::load);

                for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
                {
                    arStore.Flush();
                    arStore << repulsive;
                    arStore << GetFireType();
                    arStore << GetAfterEffect();
                    (*i)->ChangeState(L"Damaged", ar);
                }

                WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Horizontal);
                _isEmpty = true;
            }
        }
    }
    else
    {
        _isEmpty = true;
    }
}

void NormalSlashSingle::Update(float elapsedTime)
{
	if(_unit.IsNull())
	{
		_isEmpty = true;
		return;
	}

    uint32_t fc = (uint32_t)(_accumDelta * 1000.0f + 0.5f);
    if (_fireIdleTime > fc)
    {
        _accumDelta += elapsedTime;
        return;
    }

    if (_fireTime + _fireIdleTime >= fc)
    {
        switch (_unit->GetCharacterType())
        {
        case CharacterTypes::None:
            break;
        case CharacterTypes::Player:
            {
                RECT difference;
                DirectionType repulsive;
                Unit * target = NULL;

                g_DrawTool.drawBox(_position, _targetArea);
                g_DrawTool.drawPoint(_position);

                std::vector<Unit *> dest;
                if (WorldProxy()->BoundCheck(_unit, dest, _position, _targetArea, repulsive, difference))
                {
                    std::vector<uint8_t> stream;
                    Archive arStore(stream, Archive::store);
                    Archive ar(stream, Archive::load);

                    for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
                    {
                        arStore.Flush();
                        arStore << repulsive;
                        arStore << GetFireType();
                        arStore << GetAfterEffect();
                        (*i)->ChangeState(L"Damaged", ar);

						//데미지정보를 타겟에 직접 전달시킨다. [Riot]
						MessageProxy()->Send(eCMD_BATTLE_DAMAGED, _unit, (*i), &ar);

                        WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Horizontal);
                        _isEmpty = true;
                        return;
                    }
                }
            }
            break;
        case CharacterTypes::Monster:
            {
                RECT difference;
                DirectionType repulsive;
                Unit * target = NULL;

                g_DrawTool.drawBox(_position, _targetArea);
                g_DrawTool.drawPoint(_position);

                std::vector<Unit *> dest;
                if (WorldProxy()->BoundCheck(_unit, dest, _position, _targetArea, repulsive, difference))
                {
                    std::vector<uint8_t> stream;
                    Archive arStore(stream, Archive::store);
                    Archive ar(stream, Archive::load);

                    for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
                    {
                        arStore.Flush();
                        arStore << repulsive;
                        arStore << GetFireType();
                        arStore << GetAfterEffect();
                        (*i)->ChangeState(L"Damaged", ar);

						MessageProxy()->Send(eCMD_BATTLE_DAMAGED, _unit, (*i), &ar);

                        WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Horizontal);
                        _isEmpty = true;
                        return;
                    }
                }
            }
            break;
		case CharacterTypes::ActionItem:
			{
				RECT difference;
				DirectionType repulsive;
				Unit * target = NULL;

				g_DrawTool.drawBox(_position, _targetArea);
				g_DrawTool.drawPoint(_position);

				std::vector<Unit *> dest;
				if (WorldProxy()->BoundCheck(_unit, dest, _position, _targetArea, repulsive, difference))
				{
					std::vector<uint8_t> stream;
					Archive arStore(stream, Archive::store);
					Archive ar(stream, Archive::load);

					for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
					{
						arStore.Flush();
						arStore << repulsive;
						arStore << GetFireType();
						arStore << GetAfterEffect();
						(*i)->ChangeState(L"Damaged", ar);

						MessageProxy()->Send(eCMD_BATTLE_DAMAGED, _unit, (*i), &ar);

						WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Horizontal);
						_isEmpty = true;
						return;
					}
				}
			}
			break;
        }

        _accumDelta += elapsedTime;
    }
    else
    {
        //if ((GetRemoveTiming() & RemoveTimingTypes::TimeOver) != 0)
        _isEmpty = true;
    }
}


void NormalSlashMulti::Update(float elapsedTime)
{
	if(_unit.IsNull())
	{
		_isEmpty = true;
		return;
	}

    uint32_t fc = (uint32_t)(_accumDelta * 1000.0f + 0.5f);
    if (_fireIdleTime > fc)
    {
        _accumDelta += elapsedTime;
        return;
    }

    if (_fireTime + _fireIdleTime >= fc)
    {
        switch (_unit->GetCharacterType())
        {
        case CharacterTypes::None:
            break;
        case CharacterTypes::Player:
            {
                RECT difference;
                DirectionType repulsive;
                Unit * target = NULL;

                g_DrawTool.drawBox(_position, _targetArea);
                g_DrawTool.drawPoint(_position);

                std::vector<Unit *> dest;
                if (WorldProxy()->BoundCheck(_unit, dest, _position, _targetArea, repulsive, difference))
                {
                    std::vector<uint8_t> stream;
                    Archive arStore(stream, Archive::store);
                    Archive ar(stream, Archive::load);

                    for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
                    {
                        arStore.Flush();
                        arStore << repulsive;
                        arStore << GetFireType();
                        arStore << GetAfterEffect();
                        (*i)->ChangeState(L"Damaged", ar);

						MessageProxy()->Send(eCMD_BATTLE_DAMAGED, _unit, (*i), &ar);
                    }
                    WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Horizontal);
                    //if ((GetRemoveTiming() & RemoveTimingTypes::Hit) != 0)
                        _isEmpty = true;
                }
            }
            break;
        case CharacterTypes::Monster:
            break;
        }

        _accumDelta += elapsedTime;
    }
    else
    {
        //if ((GetRemoveTiming() & RemoveTimingTypes::TimeOver) != 0)
            _isEmpty = true;
    }
}

void UpperSlash::Update(float elapsedTime)
{
	if(_unit.IsNull())
	{
		_isEmpty = true;
		return;
	}

    uint32_t fc = (uint32_t)(_accumDelta * 1000.0f + 0.5f);

    if (_fireIdleTime > fc)
    {
       _accumDelta += elapsedTime;
       return;
    }

    if (_fireTime + _fireIdleTime >= fc)
    {
        switch (_unit->GetCharacterType())
        {
        case CharacterTypes::None:
            break;
        case CharacterTypes::Player:
            {
                RECT difference;
                DirectionType repulsive;
                Unit * target = NULL;

                g_DrawTool.drawBox(_position, _targetArea);
                g_DrawTool.drawPoint(_position);

                std::vector<Unit *> dest;
                if (WorldProxy()->BoundCheck(_unit, dest, _position, _targetArea, repulsive, difference))
                {
                    std::vector<uint8_t> stream;
                    Archive arStore(stream, Archive::store);
                    Archive ar(stream, Archive::load);

                    for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
                    {
                        arStore.Flush();
                        arStore << repulsive;
                        arStore << GetFireType();
                        arStore << GetAfterEffect();
                        (*i)->ChangeState(L"Damaged", ar);

						MessageProxy()->Send(eCMD_BATTLE_DAMAGED, _unit, (*i), &ar);
                    }
                    WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Both);
                    //if ((GetRemoveTiming() & RemoveTimingTypes::Hit) != 0)
                    _isEmpty = true;
                }
            }
            break;
        case CharacterTypes::Monster:
            break;
        }

        _accumDelta += elapsedTime;
    }
    else
    {
        //if ((GetRemoveTiming() & RemoveTimingTypes::TimeOver) != 0)
        _isEmpty = true;
    }
}

SlashWave::SlashWave()
{
    _animation = NULL;
    std::vector<uint8_t> stream;
    Archive arStore(stream, Archive::store);

    FILE * fileStream = NULL;
    ResProxy()->SetRoot(L"data/");
    if (ResProxy()->FileOpen(&fileStream, L"swave.mtn", L"r") == 0)
    {
        Archive arFile(fileStream, Archive::load);

        arStore << L"Animation";
        arStore << L"swave";
        arStore << arFile;

        ResProxy()->FileClose(fileStream);
    }
    Archive ar(stream, Archive::load);
    _animation = static_cast<Animation *>(System::object->BuildSerializableObject(ar));
    _animation->Loop(true);
}

SlashWave::~SlashWave()
{
    SV_DELETE(_animation);
}

void SlashWave::Serialize(Archive &ar)
{
    Magazine::Serialize(ar);

    ar >> _direction;

    if (_direction == DirectionTypes::Right)
        _animation->HReverse(false);
    else
        _animation->HReverse(true);

    _animation->Stop();
    _delayPlay = true;
}

void SlashWave::Update(float elapsedTime)
{
	if(_unit.IsNull())
	{
		_isEmpty = true;
		return;
	}

    uint32_t fc = (uint32_t)(_accumDelta * 1000.0f + 0.5f);

    if (_fireIdleTime > fc)
    {
        _accumDelta += elapsedTime;
        return;
    }

    if (_delayPlay)
    {
        _animation->Play();
        _delayPlay = false;
    }
    
    if (_fireTime + _fireIdleTime >= fc)
    {
        float movingValue = 60.f * elapsedTime;
        movingValue *= 6.0f;

        switch (_unit->GetCharacterType())
        {
        case CharacterTypes::None:
            break;
        case CharacterTypes::Player:
            {
                RECT difference;
                DirectionType repulsive;
                Unit * target = NULL;

                RECT area;
                if (_direction == DirectionTypes::Right)
                {
                    area.left = _targetArea.left - 50;
                    area.right = _targetArea.right - 50;
                    area.top = _targetArea.top - 30;
                    area.bottom = _targetArea.bottom - 30;
                    WorldProxy()->CalcMovingPosition(_position, movingValue, _direction, true);
                    _animation->SetPosition(D3DXVECTOR3(_position.x, _position.y, _position.z));
                    //System::log << _position.x << endl;
                }
                else
                {
                    area.left = _targetArea.left - 10;
                    area.right = _targetArea.right - 10;
                    area.top = _targetArea.top - 30;
                    area.bottom = _targetArea.bottom - 30;
                    WorldProxy()->CalcMovingPosition(_position, movingValue, _direction, true);
                    _animation->SetPosition(D3DXVECTOR3(_position.x, _position.y, _position.z));
                }

                g_DrawTool.drawBox(_position, area);
                g_DrawTool.drawPoint(_position);

                std::vector<Unit *> dest;
                if (WorldProxy()->BoundCheck(_unit, dest, _position, area, repulsive, difference))
                {
                    std::vector<uint8_t> stream;
                    Archive arStore(stream, Archive::store);
                    Archive ar(stream, Archive::load);

                    for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
                    {
                        arStore.Flush();
                        arStore << repulsive;
                        arStore << GetFireType();
                        arStore << GetAfterEffect();
                        (*i)->ChangeState(L"Damaged", ar);

						MessageProxy()->Send(eCMD_BATTLE_DAMAGED, _unit, (*i), &ar);
                    }
                    WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Both);

                    //_isEmpty = true;
                    //_animation->Stop();
                }
            }
            break;
        case CharacterTypes::Monster:
            break;
        }

        _animation->FrameMove(elapsedTime);
        _accumDelta += elapsedTime;
    }
    else
    {
        //if ((GetRemoveTiming() & RemoveTimingTypes::TimeOver) != 0)
        _isEmpty = true;
        _animation->Stop();
    }
}

void BombItemMag::Update(float elapsedTime)
{
	uint32_t fc = (uint32_t)(_accumDelta * 1000.0f + 0.5f);
	if (_fireIdleTime > fc)
	{
		_accumDelta += elapsedTime;
		return;
	}

	if (_fireTime + _fireIdleTime >= fc)
	{
		_accumDelta += elapsedTime;

		{
			RECT difference;
			DirectionType repulsive;
			Unit * target = NULL;

			g_DrawTool.drawBox(_position, _targetArea);
			g_DrawTool.drawPoint(_position);

			std::vector<Unit *> dest;
			if (WorldProxy()->BoundCheck(CharacterTypes::Monster, dest, _position, _targetArea, repulsive, difference))
			{
				std::vector<uint8_t> stream;
				Archive arStore(stream, Archive::store);
				Archive ar(stream, Archive::load);

				for (std::vector<Unit *>::iterator i = dest.begin(); i != dest.end(); ++i)
				{
					arStore.Flush();
					arStore << repulsive;
					arStore << GetFireType();
					arStore << GetAfterEffect();
					(*i)->ChangeState(L"Damaged", ar);
				}

				WorldProxy()->GetCamera()->GenerateNoise(NoiseTypes::Horizontal);
				_isEmpty = true;
			}
		}
	}
	else
	{
		_isEmpty = true;
	}
}
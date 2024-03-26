#include "ActionKey.h"
#include "SVLog.h"
#include "SVInternal.h"

CORESTART

ActionKey::ActionKey(eKeyTestType keyTestType, unsigned int keyDelayedTime /* = 0 */)
{
	_keyTestType		= keyTestType;
	_bPressedKeyTest	= true;
	_bReleaseKeyTest	= false;
	_keyTimeParam		= keyDelayedTime;
}

ActionKey::~ActionKey(void)
{
}

bool ActionKey::isCheckedKey(const OIS::KeyEvent &arg)
{
	std::vector<OIS::KeyCode>::iterator iterKeyParam = _keyParams.begin();
	for(; iterKeyParam != _keyParams.end(); ++iterKeyParam)
	{
		if((*iterKeyParam) == arg.key)
		{
			return true;
		}
	}
	return false;
}

bool ActionKey::keyPressed(const OIS::KeyEvent &arg)
{
	if(_bPressedKeyTest == false)
		return false;

	//저장할 필요가 있는 입력키인지 검사한다.
	if(isCheckedKey(arg) == false)
		return false;

	//키버퍼는 최대 5개까지 저장 할 수 있다, 초과시 리셋한다.
	if(_bufferedKeys.size() > 5)
		_bufferedKeys.clear();

	//버퍼에 오랫동안 저장된 키는 제거한다.
	iterKeyEventBuffer iterKeyEvent = _bufferedKeys.begin();
	for(; iterKeyEvent!=_bufferedKeys.end(); ++iterKeyEvent)
	{
		unsigned int time_gap = arg.timeStamp - (*iterKeyEvent).time;
		if(time_gap > 1000)
		{
			iterKeyEvent = _bufferedKeys.erase(iterKeyEvent);
			if(iterKeyEvent==_bufferedKeys.end())
				break;
		}
	}

	//입력된 키정보를 저장해놓는다.
	sKeyEvent keyEvent;
	keyEvent.key	= arg.key;
	keyEvent.time	= arg.timeStamp;
	_bufferedKeys.push_back(keyEvent);
	return true;
}

bool ActionKey::keyReleased(const OIS::KeyEvent &arg)
{
	if(_bReleaseKeyTest == false)
		return false;

	//저장할 필요가 있는 입력키인지 검사한다.
	if(isCheckedKey(arg) == false)
		return false;

	if(_bufferedReleaseKeys.size() > 5)
		_bufferedReleaseKeys.clear();

	sKeyEvent keyEvent;
	keyEvent.key	= arg.key;
	keyEvent.time	= arg.timeStamp;
	_bufferedReleaseKeys.push_back(keyEvent);
	return true;
}

bool ActionKey::keyTest(float elapsedTime)
{
	switch(_keyTestType)
	{
	case eKeyTestType_Once: //단일키 테스트
		{
			int bufferedKeyIndex = 0;

			for(int j=0; j<(int)_keyParams.size(); j++)
			{
				for(int i=bufferedKeyIndex; i<(int)_bufferedKeys.size(); i++)
				{
					if(_bufferedKeys[bufferedKeyIndex].key == _keyParams[j])
					{
						_bufferedKeys.clear();
						_event.emit(false);
						return true;
					}
				}
			}
		}
		break;

	case eKeyTestType_Mix: //조합키 테스트
		{
			int mixKeyCnt = (int)_keyParams.size();
			int sameKeyCnt = 0;
		
			for(int j=0; j<(int)_keyParams.size(); j++)
			{
				for(int i=0; i<(int)_bufferedKeys.size(); i++)
				{
					if(_bufferedKeys[i].key == _keyParams[j])
					{
						sameKeyCnt++;
						break;
					}
				}
			}

			if(sameKeyCnt >= mixKeyCnt)
			{
				_bufferedKeys.clear();
				_event.emit(false);
				return true;
			}
		}
		break;

	case eKeyTestType_Command: //커맨드키 테스트
		{
			int mixKeyCnt = (int)_keyParams.size();
			int sameKeyCnt = 0;
			int bufferedKeyIndex = 0;
			unsigned int keyDelayedTimeSum = 0;

			for(int j=0; j<(int)_keyParams.size(); j++)
			{
				for(int i=bufferedKeyIndex; i<(int)_bufferedKeys.size(); i++)
				{
					if(_bufferedKeys[bufferedKeyIndex].key == _keyParams[j])
					{
						keyDelayedTimeSum += _bufferedKeys[bufferedKeyIndex].time - _bufferedKeys[0].time;

						sameKeyCnt++;
						bufferedKeyIndex++;
						break;
					}
				}
			}

			if(sameKeyCnt >= mixKeyCnt)
			{
				if(keyDelayedTimeSum > _keyTimeParam)
				{
					_bufferedKeys.erase(_bufferedKeys.begin());
					return false;
				}

				_bufferedKeys.clear();
				_event.emit(false);
				return true;
			}
		}
		break;
	}
	return false;
}

bool ActionKey::releaseKeyTest(float elapsedTime)
{
	switch(_keyTestType)
	{
	case eKeyTestType_Once: //단일키 테스트
		{
			int bufferedKeyIndex = 0;

			for(int j=0; j<(int)_keyParams.size(); j++)
			{
				for(int i=bufferedKeyIndex; i<(int)_bufferedReleaseKeys.size(); i++)
				{
					if(_bufferedReleaseKeys[bufferedKeyIndex].key == _keyParams[j])
					{
						_bufferedReleaseKeys.clear();
						_event.emit(true);
						return true;
					}
				}
			}
		}
		break;

	case eKeyTestType_Mix: //조합키 테스트
		{
		}
		break;

	case eKeyTestType_Command: //커맨드키 테스트
		{
		}
		break;
	}
	return false;
}

COREEND
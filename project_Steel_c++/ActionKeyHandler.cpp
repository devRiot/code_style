#include "ActionKeyHandler.h"

CORESTART

ActionKeyHandler::ActionKeyHandler() 
{
	_bufferedCheckDelay	= 25;
}

bool ActionKeyHandler::keyPressed(const OIS::KeyEvent &arg)
{
	if( _keyParams.size() == 1)
	{
		if( _keyParams[0] == arg.key)
		{
			_actionEvent.emit();
			return true;
		}
	}
	else if(_keyParams.size() == 2)
	{
		iterActionKey iter = _keyParams.begin();
		for(; iter!=_keyParams.end(); ++iter)
		{
			if((*iter) == arg.key)
			{
				if(_keyParams[0] == arg.key && _keyParams[1] == _bufferedKey)
				{
					if( (arg.timeStamp - _bufferedKeyTime) < _bufferedCheckDelay)
					{
						_actionEvent.emit();
						return true;
					}
				}

				if(_keyParams[1] == arg.key && _keyParams[0] == _bufferedKey)
				{
					if( (arg.timeStamp - _bufferedKeyTime) < _bufferedCheckDelay)
					{
						_actionEvent.emit();
						return true;
					}
				}

				_bufferedKey		= arg.key;
				_bufferedKeyTime	= arg.timeStamp;
			}
		}
	}
	
	return false;
}

COREEND
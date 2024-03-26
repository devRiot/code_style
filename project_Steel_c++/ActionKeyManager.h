#pragma once
#include <string>
#include <map>
#include <vector>
#include "ActionKey.h"


CORESTART

//액션키매니저
class ActionKeyManager
{
	typedef std::map<std::wstring, ActionKey*>	ActionKeyMap;
	typedef ActionKeyMap::iterator				iterActionKey;

public:
	ActionKeyManager();
	virtual ~ActionKeyManager();

private:
	ActionKeyMap		_actionKeyMap;

public:
	void RegisterActionKey(const std::wstring& action_name, ActionKey* actionKey);
	void keyPressed(const OIS::KeyEvent &arg);
	void keyReleased(const OIS::KeyEvent &arg);

public: //for getter
	ActionKey* getActionKey(const std::wstring& action_name);
};

COREEND
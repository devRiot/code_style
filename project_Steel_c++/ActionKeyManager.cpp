#include "ActionKeyManager.h"

CORESTART

ActionKeyManager::ActionKeyManager()
{
}

ActionKeyManager::~ActionKeyManager()
{
	iterActionKey iter = _actionKeyMap.begin();
	for(; iter!=_actionKeyMap.end(); ++iter)
	{
		SV_DELETE((*iter).second);
	}
}

void ActionKeyManager::RegisterActionKey(const std::wstring& action_name, ActionKey* actionKey)
{
	iterActionKey iter = _actionKeyMap.find(action_name);
	if(iter != _actionKeyMap.end())
		return;

	actionKey->setActionName(action_name);
	_actionKeyMap.insert(make_pair(action_name, actionKey));
}

void ActionKeyManager::keyPressed(const OIS::KeyEvent &arg)
{
	iterActionKey iter = _actionKeyMap.begin();
	for(; iter != _actionKeyMap.end(); ++iter)
	{
		ActionKey* actionKey = (*iter).second;
		actionKey->keyPressed(arg);
		actionKey->keyTest(0.f);
	}
}

void ActionKeyManager::keyReleased(const OIS::KeyEvent &arg)
{
	iterActionKey iter = _actionKeyMap.begin();
	for(; iter != _actionKeyMap.end(); ++iter)
	{
		ActionKey* actionKey = (*iter).second;
		actionKey->keyReleased(arg);
		actionKey->releaseKeyTest(0.f);
	}
}	

ActionKey* ActionKeyManager::getActionKey(const std::wstring& action_name)
{
	iterActionKey iter = _actionKeyMap.find(action_name);
	
	if(iter != _actionKeyMap.end())
		return (*iter).second;

	return NULL;
}

COREEND
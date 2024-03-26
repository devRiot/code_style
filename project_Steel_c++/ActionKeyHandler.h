#pragma once
#include <sigc++/sigc++.h>
#include "InputManager.h"

using namespace std;

CORESTART

typedef std::vector<OIS::KeyCode>	ActionKeyParams;
typedef ActionKeyParams::iterator	iterActionKey;

//액션키 핸들러
class ActionKeyHandler
{
public:
	ActionKeyHandler();
	virtual ~ActionKeyHandler() {}

private:
	OIS::KeyCode		_bufferedKey;
	unsigned int		_bufferedKeyTime;
	unsigned int		_bufferedCheckDelay;

public:
	sigc::signal<void>	_actionEvent;
	ActionKeyParams		_keyParams;

public:
	bool keyPressed(const OIS::KeyEvent &arg);
};
COREEND
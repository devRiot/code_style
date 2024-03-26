#pragma once 
#include <vector>
#include <OIS.h>
#include <sigc++/sigc++.h>
#include "TypeDef.h"
#include "InputManager.h"

using namespace OIS;

CORESTART

struct sKeyEvent
{
	KeyCode			key;
	unsigned int	time;
};

enum eKeyTestType {
	eKeyTestType_Once,		//키 한번입력
	eKeyTestType_Mix,		//키조합 동시입력
	eKeyTestType_Command,	//커맨드 입력
	eKeyTestType_Max
};

//액션키
class ActionKey : public InputHandler
{
public:
	ActionKey(eKeyTestType keyTestType, unsigned int keyDelayedTime = 0);
	virtual ~ActionKey();
	typedef std::vector<OIS::KeyCode>	KeyParams;
	typedef std::vector<sKeyEvent>		KeyEventBuffers;
	typedef KeyEventBuffers::iterator	iterKeyEventBuffer;

private:
	std::wstring	_actionName;
	eKeyTestType	_keyTestType;
	bool			_bPressedKeyTest;
	bool			_bReleaseKeyTest;
	unsigned int	_keyTimeParam;
	KeyEventBuffers	_bufferedKeys;
	KeyEventBuffers	_bufferedReleaseKeys;

public:
	KeyParams	_keyParams;
	sigc::signal<void, bool>	_event;	

protected:
	bool isCheckedKey(const OIS::KeyEvent &arg);	

public:
	virtual bool keyPressed(const OIS::KeyEvent &arg);
	virtual bool keyReleased(const OIS::KeyEvent &arg);

public:
	void	setActionName(const std::wstring& actionName) { _actionName = actionName; }
	void	setKeyCheckType(bool bKeyPressed, bool bKeyReleased) { 
		_bPressedKeyTest = bKeyPressed;
		_bReleaseKeyTest = bKeyReleased; 
	}
	bool	keyTest(float elapsedTime);
	bool	releaseKeyTest(float elapsedTime);
};

COREEND
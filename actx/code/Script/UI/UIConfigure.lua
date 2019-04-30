
UIStyle = {
	LOGIN = 0,
	YES = 1,
	YESNO = 2,
	PROMPT = 3,
	ACCOUNT = 4,
	LOADING = 5,
	HOME = 6,
	TASK = 7,
	BACKGROUND = 8,
	FIGHT = 9,
}

-- game ui base config table
UIConfigure = {
	[UIStyle.LOGIN] 	= {ID=UIStyle.LOGIN, 		prefab="UI/Window/UILogin", 		uiClass=UILogin, 		active=true, async=true, 	cache=true, back=false},
	[UIStyle.YES] 		= {ID=UIStyle.YES, 			prefab="UI/Window/UIYes", 			uiClass=UIYes, 			active=true, async=true, 	cache=true, back=false},
	[UIStyle.YESNO] 	= {ID=UIStyle.YESNO, 		prefab="UI/Window/UIYesNo", 		uiClass=UIYesNo, 		active=true, async=true, 	cache=true, back=false},
	[UIStyle.PROMPT] 	= {ID=UIStyle.PROMPT, 		prefab="UI/Window/UIPrompt", 		uiClass=UIPrompt, 		active=true, async=true, 	cache=false, back=false},
	[UIStyle.ACCOUNT] 	= {ID=UIStyle.ACCOUNT,		prefab="UI/Window/UIAccount", 		uiClass=UIAccount, 		active=true, async=true, 	cache=true, back=false},
	[UIStyle.LOADING] 	= {ID=UIStyle.LOADING,		prefab="UI/Window/UILoading", 		uiClass=UILoading, 		active=true, async=true, 	cache=true, back=false},
	[UIStyle.HOME] 		= {ID=UIStyle.HOME,			prefab="UI/Window/UIHome", 			uiClass=UIHome, 		active=true, async=true, 	cache=true, back=true},
	[UIStyle.TASK] 		= {ID=UIStyle.TASK,			prefab="UI/Window/UITask", 			uiClass=UITask, 		active=true, async=true, 	cache=true, back=false},
	[UIStyle.BACKGROUND]= {ID=UIStyle.BACKGROUND,	prefab="UI/Window/UIBackground",	uiClass=UIBackground, 	active=true, async=true, 	cache=true, back=false},
	[UIStyle.FIGHT] 	= {ID=UIStyle.FIGHT,		prefab="UI/Window/UIFight", 		uiClass=UIFight, 		active=false, async=true, 	cache=true, back=true},
}

GuiEvent = {
	EVT_LOGIN 			= 10000,
	EVT_UPDATE_CHARLIST = 10001,
	EVT_SELECT_CHARITEM = 10002,
	EVT_CREATE_CHARITEM = 10003,
	EVT_ACCOUNT			= 10004,
	EVT_HOMESELMODE		= 10005,
	EVT_WINDOWCLOSED	= 10006,
	EVT_WINDOWOPENED	= 10007,
	EVT_RETURN			= 10008,
	EVT_EXECUTE			= 10009,
	EVT_SCENESTARTUP 	= 10010,
	EVT_SCENECOMPLETE 	= 10011,
}
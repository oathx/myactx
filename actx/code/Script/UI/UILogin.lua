module("UILogin", package.seeall)
setmetatable(UILogin, {__index=UIWidget})

UL_LOGIN 	= "UL_LOGIN"
UL_ACCOUNT 	= "UL_ACCOUNT"

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UILogin})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	UIWidget.Init(self, go)
end

function Awake(self)
	self:Install({
			UL_LOGIN,
		})
end

function Start(self)
	self:RegisterClickEvent(UL_LOGIN, 	OnLoginClicked)
end

function Close(self, complete)
	--self:DOCanvasGroupFade(1, 0, 1, function() 
		if complete then
			complete()	
		end
	--end)
end

function OnLoginClicked(self, goSend, evtData)
    local evtArgs = {
    }
    evtArgs.UserName 	= PlayerPrefs.GetString("USERNAME")
    evtArgs.Password 	= PlayerPrefs.GetString("PASSWORD")

    if string.len(evtArgs.UserName) == 0 or string.len(evtArgs.Password) == 0 then
        evtArgs.UserName = SystemInfo.deviceUniqueIdentifier 
        evtArgs.Password = SystemInfo.deviceUniqueIdentifier
    end

    XPluginManager.GetSingleton():FireEvent(GuiEvent.EVT_LOGIN, evtArgs)
end
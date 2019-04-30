module("UITask", package.seeall)
setmetatable(UITask, {__index=UIWidget})

UT_EXECTASK 	= "UT_EXECTASK"
UT_GIVEUP 		= "UT_GIVEUP"

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UITask})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	UIWidget.Init(self, go)
end

function Awake(self)
	self:Install({
			UT_EXECTASK, UT_GIVEUP
		})
end

function Start(self)
	self:RegisterClickEvent(UT_EXECTASK, 	OnExecTaskClicked)
	self:RegisterClickEvent(UT_GIVEUP, 		OnGiveupClicked)
end

function Close(self, complete)
	--self:DOCanvasGroupFade(1, 0, 1, function() 
		if complete then
			complete()	
		end
	--end)
end

function OnExecTaskClicked(self, goSend, evtData)
	XPluginManager.GetSingleton():FireEvent(GuiEvent.EVT_EXECUTE, {
			widget = self
		})
end

function OnGiveupClicked(self, goSend, evtData)
	XPluginManager.GetSingleton():FireEvent(GuiEvent.EVT_RETURN, {
			widget = self
		})
end
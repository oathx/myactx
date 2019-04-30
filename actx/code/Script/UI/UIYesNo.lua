module("UIYesNo", package.seeall)
setmetatable(UIYesNo, {__index=UIWidget})

UB_YES 	= "UB_YES"
UB_NO 	= "UB_NO"
UB_TEXT	= "UB_TEXT"

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UIYesNo})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	UIWidget.Init(self, go)
	
	self.call = nil
	self.args = nil
	self.text = nil
end

function Awake(self)
	self:Install({
		UB_YES, UB_NO, UB_TEXT
	})
end

function Start(self)
	self:RegisterClickEvent(UB_YES, OnClicked)
	self:RegisterClickEvent(UB_NO,  OnClicked)
end

function Message(self, text, callback, args)
	self:SetText(UB_TEXT, text)

	if callback then
		self.call = callback
	end

	self.args = args
end

function OnClicked(self, goSend, evtData)
	if self.call then
		self.call(goSend.name == UB_YES, args)
	end
	
	UISystem.GetSingleton():CloseWidget(UIStyle.YESNO)
end



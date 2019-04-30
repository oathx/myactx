module("UIYes", package.seeall)
setmetatable(UIYes, {__index=UIWidget})

UB_YES  = "UB_YES"
UB_TEXT = "UB_TEXT"

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UIYes})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	UIWidget.Init(self, go)
	
	self.call = nil
	self.args = nil
end

function Awake(self)
	self:Install({
		UB_YES, UB_TEXT
	})
end

function Start(self)
    XStaticDOTween.DOScaleV(
        self.gameObject.transform, Vector3.one * 0.9, 0.5)

    self:RegisterClickEvent(UB_YES, OnYesClicked)
end

function Message(self, text, callback, args)
	self:SetText(UB_TEXT, text)

	if callback then
		self.call = callback
	end

	self.args = args
end

function OnYesClicked(self, goSend, evtData)
	if self.call then
		self.call(true, args)
	end
	
	UISystem.GetSingleton():CloseWidget(UIStyle.YES)
end


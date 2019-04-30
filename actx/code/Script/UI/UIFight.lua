module("UIFight", package.seeall)
setmetatable(UIFight, {__index=UIWidget})

UF_LEG 	= "UF_LEG"
UF_FIST = "UF_FIST"

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UIFight})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	UIWidget.Init(self, go)
end

function Awake(self)
	self:Install({
			UF_LEG, UF_FIST
		})
end

function Start(self)
	self:RegisterClickEvent(UF_LEG, OnLegClicked)
	self:RegisterClickEvent(UF_FIST, OnFistClicked)
end

function SetMaster(self, master)
	self.master = master
end

function GetMaster(self)
	return self.master
end

function OnLegClicked(self, goSend, evtData)
	if not self.input and self.master then
		self.input = self.master:GetComponent(XPlayerInput)
	end

	if self.input then
		self.input:PostEvent(resmng.PlayerAction.Dash)
	end
end

function OnFistClicked(self, goSend, evtData)
	if not self.input and self.master then
		self.input = self.master:GetComponent(XPlayerInput)
	end

	if self.input then
		self.input:PostEvent(resmng.PlayerAction.Attack)
	end
end
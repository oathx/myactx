module("XEventModule", package.seeall)

function new(szName)
	local obj = {
	}
	setmetatable(obj, {__index=XEventModule})
	obj:Init(szName)
	
	return obj
end

function Init(self, szName)
	self.name 	= szName
	self.events = {
	}
	self.active = false
end

function Name(self)
	return self.name
end

function Active(self)
	if not self.active then
		self.active = true

		self:OnActive()
	end
end

function GetActive(self)
	return self.active
end

function Detive(self)
	if self.active then
		self.active = false

		self:OnDetive()
	end
end

function Unload(self, destroyFlag)
	if self.dispatcher then
		self.dispatcher:Unload(self:Name(), destroyFlag)
	end

	if destroyFlag then
		self:ResetEvent()
	end	
end

function SetDispatcher(self, dispatcher)
	self.dispatcher = dispatcher
end

function GetDispatcher(self)
	return self.dispatcher
end

function SubscribeEvent(self, nID, evtCallback)
	if not self.events[nID] then
		self.events[nID] = evtCallback
	end
end

function FireEvent(self, nID, evtArgs)
	return self.events[nID](self, evtArgs)
end

function HasEvent(self, nID)
	return self.events[nID]
end

function RemoveEvent(self, nID)
	if self.events[nID] then
		self.events[nID] = nil
	end
end

function ResetEvent(self)
	self.events = {}
end

function OnActive(self)
end

function OnDetive(self)
end

function OnDestroy(self)
end




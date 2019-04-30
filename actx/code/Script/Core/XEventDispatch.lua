module("XEventDispatch", package.seeall)
setmetatable(XEventDispatch, {__index=XLuaBehaviour})

function new(szName)
	local obj = {
	}
	setmetatable(obj, {__index=XEventDispatch})
	obj:Init(szName)
	
	return obj
end

function Init(self, szName)
	self.gameObject = GameObject(szName)
	if not self.gameObject then
		error("Can't create GameObject " .. szName)
	end
	
	self:AddLuaBehaviour(self.gameObject)
	
	self.observers = {}
	self.postEvent = {}
end

function GetName(self)
	return self.gameObject.name
end

function Load(self, name, pluginClassType, active)
	local observer = self.observers[name]
	if not observer then
		observer = _G[pluginClassType].new(name)
		if observer then
			self:Register(observer, active)
			return observer
		end
	end
end

function Unload(self, name, destroyFlag)
	local observer = self.observers[name]
	if observer then
		self:Unregister(observer, destroyFlag)
		return name
	end
end

function Register(self, observer, active)
	if observer then
		local name = observer:Name()
		
		if not self.observers[name] then
			observer:SetDispatcher(self)

			TRACE("register observer name=%s", name)

			self.observers[name] = observer

			if self.transform then
				local obsTrans = observer.transform
				if obsTrans then
					obsTrans.transform.position = Vector3.zero
					obsTrans:SetParent(self.transform, false)
				end

				if active then
					observer:Active()
				end
			end
		end
	end
end

function Query(self, name)
	ELOG("Query %s", name)
	return self.observers[name]
end

function Unregister(self, observer, destroyFlag)
	if observer then
		local name = observer:Name()
		if self.observers[name] then
			if observer:GetActive() then
				observer:Detive()
			end

			if destroyFlag then
				if observer.gameObject then
					GameObject.Destroy(observer.gameObject)
				else
					observer:OnDestroy()
				end

				self.observers[name] = nil
			end
		end
	end
end

function GetObservers(self)
	return self.observers
end

function DispatchEvent(self, nID, evtArgs, szObserverName)
	if evtArgs and not evtArgs.dispatcher then
		evtArgs.dispatcher = self
	end
	
	if szObserverName then
		local observer = self:Query(szObserverName)
		if observer then
			return observer:FireEvent(nID, evtArgs)
		end
	else
		for name, observer in pairs(self.observers) do
			if observer:HasEvent(nID) then
				local result = observer:FireEvent(nID, evtArgs)
				if result then
					return true
				end
			end
		end
	end
end

function SendEvent(self, nID, evtArgs, szObserverName)
	return self:DispatchEvent(nID, evtArgs, szObserverName)
end

function PostEvent(self, nID, evtArgs, fDelayTime, szObserverName)
	local post = {
	}
	
	post.DelayTime 	= fDelayTime or 0
	post.ID 		= nID
	post.Args 		= evtArgs
	post.PostTime 	= Time.time
	post.Observer	= szObserverName

	table.insert(self.postEvent, post)
end

function Send(self, id, evtArgs)
	for idx, observer in pairs(self.observers) do
		local fireEvent = observer:HasEvent(id)
		if fireEvent then
			if fireEvent(observer, unpack(evtArgs)) then
				return true
			end
		end
	end

	return false
end

function FixedUpdate(self)
	local count = #self.postEvent
	for idx=1, count do
		local evt = self.postEvent[idx]
	
		if Time.time - evt.PostTime >= evt.DelayTime then
			self:DispatchEvent(evt.ID, evt.Args, evt.observer)
			table.remove(
				self.postEvent, idx
				)
		end
	end
end



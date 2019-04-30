module("XEntityManager", mkSingleton)

function Init(self)
	self.entitys = {
	}
end

function Startup(self, factory)
	for id, func in pairs(factory) do
		local entity = func(id)
		if entity then
			if self:AddEntity(entity) then
				TRACE("Startup install defualt entity id(%d)", id)
			end
		else
			ERROR("cant find factory id %d", id)
		end
	end
end

function Shutdown(self)
	for id, entity in pairs(self.entitys) do
		if entity then
			entity:Destroy()
		end
	end

	self.entitys = {}
end

function AddEntity(self, entity)
	if entity then
		local id = entity:cid()
		if not self.entitys[id] then
			self.entitys[id] = entity
			return true
		end
	end
end

function GetEntity(self, id)
	return self.entitys[id]
end

function Query(self, id)
	return self.entitys[id]
end

function RemoveEntity(self, id)
	local entity = self.entitys[id]
	if entity then
		entity:Destroy()
	end
end

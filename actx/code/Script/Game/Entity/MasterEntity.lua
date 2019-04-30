module("MasterEntity", package.seeall)
setmetatable(MasterEntity, {__index=PlayerEntity})

function new(cid, propID)
	local entity = {
	}
	setmetatable(entity, {__index=MasterEntity})
	entity:Init(cid, propID)

	return entity
end

function Init(self, cid, propID)
	PlayerEntity.Init(self, cid, propID)
	self.type = self.type + resmng.XActorType.Master
end

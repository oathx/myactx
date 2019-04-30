module("PlayerEntity", package.seeall)
setmetatable(PlayerEntity, {__index=HeroEntity})

function new(cid, propID)
	local entity = {
	}
	setmetatable(entity, {__index=PlayerEntity})
	entity:Init(cid, propID)

	return entity
end

function Init(self, cid, propID)
	HeroEntity.Init(self, cid, propID)
	self.type = self.type + resmng.XActorType.Player
end


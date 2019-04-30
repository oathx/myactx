module("EnemyEntity", package.seeall)
setmetatable(EnemyEntity, {__index=PlayerEntity})

function new(cid, propID)
	local entity = {
	}
	setmetatable(entity, {__index=EnemyEntity})
	entity:Init(cid, propID)

	return entity
end

function Init(self, cid, propID)
	PlayerEntity.Init(self, cid, propID)
	self.type = self.type + resmng.XActorType.Enemy
end
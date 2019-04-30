module("HomeEntity", package.seeall)
setmetatable(HomeEntity, {__index=XEntity})

function new(id)
	local entity = {
	}
	setmetatable(entity, {__index=HomeEntity})
	entity:Init(id)

	return entity
end

function Init(self, id)
	XEntity.Init(self, id)

	self:InitRecommended()
end

function InitRecommended(self, ids)
	self.recommendeds = {}

	local propIds = {
		resmng.CHAR_GRIL
	}

	for idx, id in ipairs(propIds) do
		table.insert(self.recommendeds, HeroEntity.new(idx, id, resmng.XActorType.Hero))
	end	
end

function GetRecommended(self)
	return self.recommendeds[math.random(1, #self.recommendeds)]
end

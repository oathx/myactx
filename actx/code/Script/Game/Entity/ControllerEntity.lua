module("ControllerEntity", package.seeall)
setmetatable(ControllerEntity, {__index=XEntity})

function new(id)
	local entity = {
	}
	setmetatable(entity, {__index=ControllerEntity})
	entity:Init(id)

	return entity
end

function Init(self, id)
	XEntity.Init(self, id)
end

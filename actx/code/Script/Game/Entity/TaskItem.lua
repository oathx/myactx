module("TaskItem", package.seeall)
setmetatable(TaskItem, {__index=XEntity})

function new(id)
	local item = {
	}
	setmetatable(item, {__index=TaskItem})
	item:Init(id)

	return item
end

function Init(self, id)
	XEntity.Init(self, id)

	self.prop = resmng.propTaskById(id)
	if not self.prop then
		ELOG("Can't find task config(%d)", id)
		return
	end	
end

function GetSceneID(self)
	return self.prop.Scene or 0
end
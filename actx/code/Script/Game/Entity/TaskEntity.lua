module("TaskEntity", package.seeall)
setmetatable(TaskEntity, {__index=XEntity})

function new(id)
	local entity = {
	}
	setmetatable(entity, {__index=TaskEntity})
	entity:Init(id)

	return entity
end

function Init(self, id)
	XEntity.Init(self, id)

	self.tasks 	 = {}
	self.current = self:Create(resmng.TASK_DEMO)	
end

function Create(self, id)
	local task = self:Query(id)
	if task then
		ELOG("Can't add task id(%s)", tostring(id))
	else
		task = TaskItem.new(id)
		table.insert(self.tasks, 
			task)
	end

	return task
end

function AddTask(self, id)
	local task = self:Query(id)
	if task then
		return task
	end

	return self:Create(id)
end

function Query(self, id)
	for idx, task in ipairs(self.tasks) do
		if task:ID() == id then
			return task
		end
	end
end

function GetCurrent(self)
	return self.current
end

function Remove(self, id)
	for idx, task in ipairs(self.tasks) do
		if task:ID() == id then
			table.remove(self.tasks, idx)
			break	
		end
	end
end

function Reset(self)
	self.tasks 	 = {}
	self.current = nil
end


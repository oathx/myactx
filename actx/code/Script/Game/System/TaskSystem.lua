module("TaskSystem", package.seeall)
setmetatable(TaskSystem, {__index=XEventBehaviour})

function new(szName)
	local system = {
	}
	setmetatable(system, {__index=TaskSystem})
	system:Init(szName)

	return system
end

function Init(self, szName)
	XEventBehaviour.Init(self, szName)

	self.entity = XEntityManager.GetSingleton():GetEntity(resmng.SYS_TASK)
	if not self.entity then
		ELOG("Can't find SYS_TASK module")
	end

	self.task = self.entity:GetCurrent()
	if not self.task then
		ERROR("Please initialize the current task")
	end
end

function OnActive(self)
	UISystem.GetSingleton():OpenWidget(UIStyle.TASK, function(widget)
		self.widget = widget 
	end)

	self:SubscribeEvent(GuiEvent.EVT_EXECUTE, OnExecTask)
	self:SubscribeEvent(GuiEvent.EVT_RETURN,  OnReturn)
end

function OnDetive(self)
	UISystem.GetSingleton():CloseWidget(UIStyle.TASK)
end

function OnReturn(self, args)
	local disptacher = self:GetDispatcher()
	if disptacher then
		disptacher:Back()
	end

	return true
end

function OnExecTask(self, args)
	if self.task then
		self:Unload(true)

		XRootContext.GetSingleton():LoadSystem(resmng.SYS_FIGHT)
	end

	return true
end
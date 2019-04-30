module("ControllerSystem", package.seeall)
setmetatable(ControllerSystem, {__index=XEventModule})

-- game controller system, Monitor and manage the game logic flow
function new(szName)
	local controller = {
	}
	setmetatable(controller, {__index=ControllerSystem})
	controller:Init(szName)

	return controller
end

function Init(self, szName)
	XEventModule.Init(self, szName)

	-- get controller entity
	self.entity = XEntityManager.GetSingleton():Query(resmng.SYS_CONTROLLER)
	if not self.entity then
		ERROR("Can't find entity %d", resmng.SYS_CONTROLLER)
	end

	self.machine = XStateMachine.new()
end

function OnActive(self)
	self:SubscribeEvent(GuiEvent.EVT_SCENESTARTUP, 	OnSceneStartup)
	self:SubscribeEvent(GuiEvent.EVT_SCENECOMPLETE,	OnSceneComplete)

	self:SubscribeEvent(GuiEvent.EVT_WINDOWOPENED, 	OnWindowOpened)
	self:SubscribeEvent(GuiEvent.EVT_WINDOWCLOSED, 	OnWindowClosed)
end

function OnDetive(self)
	self:ResetEvent()
end

function OnSceneStartup(self, evtArgs)
	local curSceneID = evtArgs.curSceneID
	local newSceneID = evtArgs.newSceneID

	if curSceneID ~= newSceneID then
		XAvatarSystem.ResetCacheAvatar()
	end
end

function OnSceneComplete(self, evtArgs)
	ELOG("OnSceneComplete [%d] %s %s %s", 
		evtArgs.scene.ID, evtArgs.scene.Path, evtArgs.scene.Script, evtArgs.showLoading)
end

function OnWindowOpened(self, evtArgs)
	local conf = evtArgs.widget:GetConfigure()
	TRACE("OnWindowOpened id=%d classname=%s", conf.ID, conf.prefab)
end

function OnWindowClosed(self, evtArgs)
	local conf = evtArgs.widget:GetConfigure()
	TRACE("OnWindowClosed id=%d classname=%s", conf.ID, conf.prefab)


	return true	
end

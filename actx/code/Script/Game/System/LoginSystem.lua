module("LoginSystem", package.seeall)
setmetatable(LoginSystem, {__index=XEventModule})

function new(szName)
	local login = {
	}
	setmetatable(login, {__index=LoginSystem})
	login:Init(szName)

	return login
end

function Init(self, szName)
	XEventModule.Init(self, szName)

	-- init system data entity object
	self.entity = XEntityManager.GetSingleton():Query(resmng.SYS_LOGIN)
	if not self.entity then
		ERROR("Can't find entity %d", resmng.SYS_LOGIN)
	end

	self.loaded = false
end

function OnActive(self)
	if not self.loaded then
		XRootContext.GetSingleton():LoadAsyncScene(resmng.SCENE_LOGIN, function(scene)
			self:OnSceneCompleted(scene)
		end)

		self.loaded = true
	end
end

function OnDetive(self)
	UISystem.GetSingleton():Clearup()
end

function OnSceneCompleted(self, scene)
	UISystem.GetSingleton():OpenWidget(UIStyle.LOGIN, function(widget)
		self.widget = widget 
	end)

	self:SubscribeEvent(GuiEvent.EVT_LOGIN, OnLoginEvent)
end

function OnLoginEvent(self, evtArgs)
	if string.len(evtArgs.UserName) == 0 or string.len(evtArgs.Password) == 0 then
		local text = resmng.LangText(resmng.LG_USERNAME_ERROR)
		if text then
			UISystem.GetSingleton():MessageBox(UIStyle.PROMPT, text)
		end
	else
		self:Unload(true)

		XRootContext.GetSingleton():LoadSystem(resmng.SYS_HOME)
	end

	return true
end
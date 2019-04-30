module("XRootContext", mkSingleton)

function Init(self)
	XEntityManager.GetSingleton():Startup({
			[resmng.SYS_LOGIN] = function(id) 
				return LoginEntity.new(id) 
			end,

			[resmng.SYS_CONTROLLER] = function(id)
				return ControllerEntity.new(id)
			end,

			[resmng.SYS_HOME] = function(id)
				return HomeEntity.new(id)
			end,

			[resmng.SYS_TASK] = function(id)
				return TaskEntity.new(id)
			end,

			[resmng.SYS_FIGHT] = function(id)
				return FightEntity.new(id)
			end,
		})

	XPluginManager.GetSingleton():Startup()
end

function Startup(self)
	XRpc:Init(Protocol.new())

	local cfgPlugin = resmng.propPluginById(resmng.PLUGIN_GAME)
	if cfgPlugin then
		self.gamePlugin = XPluginManager.GetSingleton():LoadPlugin(cfgPlugin.Name, cfgPlugin.ClassType)
		if not self.gamePlugin then
			ERROR("Can't load XGamePlugin")
		else
			UISystem.GetSingleton():Startup()

			local initSys = {
				resmng.SYS_CONTROLLER, resmng.SYS_LOGIN
			}

			for idx, sysID in ipairs(initSys) do
				self:LoadSystem(sysID)
			end
		end
	end
end

function Shutdown(self)
	XEntityManager.GetSingleton():Shutdown()
	UISystem.GetSingleton():Shutdown()
	XPluginManager.GetSingleton():Shutdown()
end

function LoadAsyncScene(self, nSceneID, complete)
	local prop = resmng.propSceneById(nSceneID)
	if prop then
		local loaded = function() 
			if complete then
				complete(prop)
			end
		end

		local curSceneID = self.curSceneID or 0
		if curSceneID ~= nSceneID then
			--XAvatarSystem.ResetCacheAvatar()
			local evtArgs = {
			}
			evtArgs.curSceneID = curSceneID
			evtArgs.newSceneID = newSceneID
			
			XPluginManager.GetSingleton():FireEvent(GuiEvent.EVT_SCENESTARTUP, evtArgs)

			-- load new scene
			XRes.LoadSceneAsync(prop.Path, function(name) 
				loaded() 
			end)

			self.curSceneID = nSceneID
		else
			loaded()
		end
	end
end

function LoadSystem(self, systemID)
	local sys = resmng.propSystemById(systemID)
	if sys then
		return self.gamePlugin:Load(sys.Name,
			sys.ClassType, sys.Active == 1 and true or false)
	end
end

function GetSystem(self, systemID)
	local sys = resmng.propSystemById(systemID)
	if sys then
		return self.gamePlugin:Query(sys.Name)
	end
end

function UnloadSystem(self, systemID)
	local sys = resmng.propSystemById(systemID)
	if sys then
		self.gamePlugin:Unload(sys.Name)
	end
end


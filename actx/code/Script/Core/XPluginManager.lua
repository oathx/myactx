module("XPluginManager", mkSingleton)
setmetatable(XPluginManager, {__index=XLuaBehaviour})

function Init(self)
	-- current plugin list
	self.plugins = {
	}
end

-- start plugin manager
function Startup(self)
	self.gameObject = GameObject("XPluginManager")
	if not self.gameObject then
		error("Can't create PluginManager")
	end

	GameObject.DontDestroyOnLoad(self.gameObject)
	
	self:AddLuaBehaviour(self.gameObject)

	return true
end

function LoadPlugin(self, name, classType)
	local plugin = self.plugins[name]
	if not plugin then
		plugin = classType.new(name)
		if plugin then
			if plugin.transform then
				plugin.transform.parent = self.transform
			end

			plugin:Install()
		end

		TRACE("Load plugin %s", name)

		self.plugins[name] = plugin
	end

	return plugin
end

function QueryPlugin(self, name)
	return self.plugins[name]
end

function UnloadPlugin(self, name)
	local plugin = self.plugins[name]
	if not plugin then
		plugin:Uninstall()

		TRACE("Unload plugin %s", name)

		-- shutdown the plugin
		plugin:Shutdown()

		if plugin.gameObject then
			GameObject.Destroy(plugin.gameObject)
		end
	end

	self.plugins[name] = nil
end

function Shutdown(self)
	for name, plugin in pairs(self.plugins) do
		self:UnloadPlugin(name)
	end

	self.plugins = {}
end

-- send a plugin event
-- @param szPluginName
-- @param nID
-- @param evtArgs
-- @param szObserverName
function SendEvent(self, szPluginName, nID, evtArgs, szObserverName)
	local plugin = self:QueryPlugin(szPluginName)
	if plugin then
		plugin:SendEvent(nID, evtArgs, szObserverName)
	end
end

-- send a plugin event
-- @param nID
-- @param evtArgs
function FireEvent(self, nID, evtArgs)
	if _EDITOR then
		INFO("FireEvent(id=%s) evtArgs(%s)", tostring(nID), tostring(evtArgs))
	end

	for idx, plugin in pairs(self.plugins) do
		local bResult = plugin:SendEvent(nID, evtArgs)
		if bResult then
			break
		end
	end
end

function PostEvent(self, nID, evtArgs)
	if _EDITOR then
		INFO("FireEvent(id=%s) evtArgs(%s)", tostring(nID), tostring(evtArgs))
	end

	for idx, plugin in pairs(self.plugins) do
		plugin:PostEvent(nID, evtArgs)
	end
end

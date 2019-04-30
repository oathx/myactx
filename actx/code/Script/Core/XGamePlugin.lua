module("XGamePlugin", package.seeall)
setmetatable(XGamePlugin, {__index=XPlugin})

function new(szPluginName)
	local obj = {
	}
	setmetatable(obj, {__index=XGamePlugin})
	obj:Init(szPluginName)
	
	return obj
end

function Init(self, szPluginName)
	XPlugin.Init(self, szPluginName)

	self.cacheSystem = {
	}
end

function Install(self)
	XPlugin.Install(self)
end

function Uninstall(self)
	XPlugin.Uninstall(self)
end

function Startup(self)
	XPlugin.Startup(self)
end

function Shutdown(self)
	XPlugin.Shutdown(self)
end

function Load(self, name, classType, active)
	local sys = XPlugin.Load(self, name, classType, active)
	if sys then
		table.insert(self.cacheSystem, {
				name = sys:Name(), active = active
			})
		
		TRACE("Load system(%s) count(%d) [%s]", 
			sys:Name(), #self.cacheSystem, name)

		return sys
	end
end

function Unload(self, name, destroyFlag)
	local unloadName = XPlugin.Unload(self, name, destroyFlag and true or false)
	
	for idx, cache in ipairs(self.cacheSystem) do
		if cache.name == unloadName then
			table.remove(self.cacheSystem, idx)
			break
		end
	end
	
	TRACE("unload system(%s) count(%d)", 
		unloadName, #self.cacheSystem)

	return unloadName
end

function Back(self)
	local count = #self.cacheSystem

	-- unload current system
	local source = self.cacheSystem[count]
	if source then
		self:Unload(source.name, true)
	end

	-- load cache system
	count = #self.cacheSystem
	if count > 0 then
		local target = self.cacheSystem[count]
		if target then
			local sys = self:Query(target.name)
			if sys then
				if not sys:GetActive() then
					sys:Active()
				end
			else
				table.remove(self.cacheSystem, count)

				-- reload the system
				sys = self:Load(target.name, 
					target.active)
			end

			return sys
		end
	end
end



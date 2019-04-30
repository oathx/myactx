module("XPlugin", package.seeall)
setmetatable(XPlugin, {__index=XEventDispatch})

function new(szPluginName)
	local obj = {
	}
	setmetatable(obj, {__index=XPlugin})
	obj:Init(szPluginName)
	
	return obj
end

function Init(self, szPluginName)
	XEventDispatch.Init(self, szPluginName)
end

-- Plugin installation is invoked this function
-- When the plug-in need before start the initialization, should be implemented here 
function Install(self)
end

-- Plugin uninstall is invoked this function
-- When the plug-in need after stop the uninstall, should be implemented here
function Uninstall(self)
end

-- Plugin startup, calls this function start the plugin
function Startup(self)
end

-- Plugin shutdown, call this function close the plugin
function Shutdown(self)
end

-- imp lua class module

function mkcall( module )
    local mt = {}
    mt.__index = _G
    mt.__call = function(func, ...) return func.new(...) end
    setmetatable( module, mt )
end

function mkSingleton( module )
	module.mt = module.mt or {__index=module}
	module.new = function()
		local ins = {}
		setmetatable(ins, module.mt)
		if ins.Init then ins:Init() end
		return ins
	end
	module.GetSingleton = function()
		local ins = rawget(module, "mkinstance_")
		if not ins then
			ins = module.new()
			rawset(module, "mkinstance_", ins)
		end

		return ins
	end

	local mt = {}
	mt.__index = _G
	mt.__call = function(func)
		return func.GetSingleton()
	end
	setmetatable( module, mt )
end

function mkDeriveClass( child, parent )
	child.mt = child.mt or {__index=child}
	child.new = function(...)
		local ins = {}
		setmetatable(ins, child.mt)
		if ins.Init then ins:Init(...) end
		return ins
	end

	local mt = {}
	mt.__index = parent or _G
	mt.__call = function(func, ...) return func.new(...) end
	setmetatable( child, mt )
end

function mkSingletonClass( child, parent )
	child.mt = child.mt or {__index=child}
	child.new = function()
		local ins = {}
		setmetatable(ins, child.mt)
		if ins.Init then ins:Init() end
		return ins
	end
	child.GetSingleton = function()
		local ins = rawget(child, "instance_")
		if not ins then
			ins = child.new()
			rawset(child, "instance_", ins)
		end
		return ins
	end

	local mt = {}
	mt.__index = parent or _G
	mt.__call = function(func)
		return func.GetSingleton()
	end
	setmetatable( child, mt )
end

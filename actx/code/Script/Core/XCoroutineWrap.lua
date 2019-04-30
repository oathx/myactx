
local resume = coroutine.resume
local unpack = unpack or table.unpack
coroutine.resume=function(co,...)
	local ret={resume(co,...)}
	if not ret[1] then UnityEngine.Debug.LogError(debug.traceback(co,ret[2])) end
	return unpack(ret)
end

coroutine.wrap = function(func)
	local co = coroutine.create(func)
	return function(...)
		local ret={coroutine.resume(co,...)}
		return unpack(ret, 2)
	end
end

function StartCoroutine( func, ... )
	local co = coroutine.create(func)
    coroutine.resume(co, ...)
    return co
end



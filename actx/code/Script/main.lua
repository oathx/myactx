import "UnityEngine"
import "UnityEngine.UI"
import "UnityEngine.EventSystems"

local scriptInclude = {
}

function include(name)
    table.insert(scriptInclude, name)
end

function progress(value, text)
    XBootstrap.GetSingleton():SetProgress(value, text)

    if value >= 1.0 then
        XBootstrap.GetSingleton():Clearup()
    end
end

function DoRequire(done)
    for name, v in pairs(package.loaded) do
        package.loaded[name] = nil
    end

    local ret, msg = pcall(require, "requires")
    if not ret then
        Debug.LogError(
            string.format("critial error! requires failed: %s", msg)
            )
    end
    
    local totalInclude = #scriptInclude
    for idx, mod in ipairs(scriptInclude) do
        local ret, msg = pcall(require, mod)
        if not ret then
            error(string.format("critial error! %s requires failed: %s", mod, msg))
        end

        progress(0.1 +  idx / totalInclude * 0.7, mod)
        
        Yield()
    end

    Yield()

    progress(0.8, tostring(totalInclude))

    if done then
        done()
    end

    return ret
end

function main()
    math.randomseed(os.time())

    local co = coroutine.create(DoRequire)
    coroutine.resume(co, function()
        OneTimeInitScene()
    end)

	return true
end

function OneTimeInitScene()
    XRootContext.GetSingleton():Startup()
  
    -- load finished
    progress(1.0, 
        resmng.LangText(resmng.LG_VERSION_DOWNLOAD))
end

function exit()
    XRootContext.GetSingleton():Shutdown()
end


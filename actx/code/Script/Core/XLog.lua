function mkSpace(num)
    local fmt = string.format("%%%d.%ds", num, num)
    return string.format(fmt, " ")
end

function dumpTab(t, step)
    local str = ""
    step = step or 0
    if type(t) == "table" then
        for k, v in pairs(t) do
            if type(v) == "table" then
                str = str..string.format("%s%s = {\n", mkSpace(step*2+2), tostring(k))
                str = str..dumpTab(v, step+1)
                str = str..string.format("%s}\n", mkSpace(step*2+2))
            else
                str = str..string.format("%s%s = %s\n", mkSpace(step*2+2), tostring(k), tostring(v))
            end
        end
    else
        str = str..string.format("%s%s = %s\n", mkSpace(step*2), tostring(k), tostring(v))
    end

    return str
end


INFO = function(fmt, ...)
    print(string.format("[INFO](%s) frame=%s %s", 
        Time.time, Time.frameCount, string.format(fmt, ...)))
end

ERROR = function(fmt, ...)
    Debug.LogError(string.format("[ERROR](%s) frame=%s %s \n %s", 
        Time.time, Time.frameCount, string.format(fmt, ...), debug.traceback()))
end

ELOG = function(fmt, ...)
    if _DEBUG then
        Debug.LogError(string.format("(%s) frame=[%s] %s \n %s", 
            Time.time, Time.frameCount, string.format(fmt, ...), debug.traceback()))
    else
        Debug.Log(string.format("<color=red>[%d](%f) %s </color>", 
            Time.frameCount, Time.time, string.format(fmt, ...), debug.traceback()))
    end   
end

WARING = function(fmt, ...)
    Debug.Log(string.format("<color=yellow> %s </color>", 
        string.format(fmt, ...)))

    Debug.Log(string.format("<color=red> %s %s </color>", 
        string.format(fmt, ...), debug.traceback()))     
end

TRACE = function(fmt, ...)
    Debug.Log(string.format("<color=green>[%d](%f) %s </color>", 
        Time.frameCount, Time.time, string.format(fmt, ...), debug.traceback()))
end


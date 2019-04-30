module("resmng", package.seeall)

require("common/protocol")
require("common/define")
require("common/define_fight")

-- require config define
require("client/define_plugin")
require("client/define_language")
require("client/define_local")
require("client/define_scene")
require("client/define_system")
require("client/define_task")
require("client/define_character")
require("client/define_shape")

-- require config prop
require("client/prop_local")
require("client/prop_plugin")
require("client/prop_scene")
require("client/prop_system")
require("client/prop_task")
require("client/prop_character")
require("client/prop_shape")

-- require current dev language, you must be define symbols _LANGUAGE_CN, use Chinese languare
if _LANGUAGE_CN then
require("client/prop_lang_cn")
else
require("client/prop_lang_en")
end

-- format language text
function LangText(id, ...)
    local arg = {...}

    if type(id) == "string" then
        id = resmng[id]
    end

    if #arg ~= 0 then
        return string.format(resmng.propLang[id], unpack(arg))
    end

    return resmng.propLang[id]
end


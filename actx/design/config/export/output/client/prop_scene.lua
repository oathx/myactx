--
-- $Id$
--

module( "resmng" )
propSceneLANGKey = {

}

propSceneKey = {
ID = 1, Path = 2, 
}

propSceneData = {

	[SCENE_LOGIN] = {SCENE_LOGIN, "Scene/Login", },
	[SCENE_HOME] = {SCENE_HOME, "Scene/Home", },
	[SCENE_DEMO] = {SCENE_DEMO, "Scene/Demo", },
}



local propScene_mt = {}
propScene_mt.__index = function (_table, _key)
    local lang_idx = propSceneLANGKey[_key]
    if lang_idx then
		local lang_str = propLanguageById(_table[lang_idx])
		local idx_ex = propSceneKey[_key .. "ARG"]
		local lang_args = _table[idx_ex]
		if lang_args then
			if #lang_args > 0 then
				for k, v in ipairs(lang_args) do
					lang_args[k] = parse_language_id_arg(v)
				end
				return string.format(lang_str,unpack(lang_args))
			end
		end
		return lang_str
    end
    local idx = propSceneKey[_key]
    if not idx then
        return nil
    end
    return _table[idx]
end

function propSceneById(_key_id)
    local id_data = propSceneData[_key_id]
    if id_data == nil then
        return nil
    end
    if getmetatable(id_data) == nil then
        setmetatable(id_data, propScene_mt)
    end
    return id_data
end


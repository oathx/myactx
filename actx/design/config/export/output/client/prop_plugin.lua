--
-- $Id$
--

module( "resmng" )
propPluginLANGKey = {

}

propPluginKey = {
ID = 1, Name = 2, ClassType = 3, Active = 4, Version = 5, 
}

propPluginData = {

	[PLUGIN_GAME] = {PLUGIN_GAME, "Game", XGamePlugin, 1, 1, },
	[PLUGIN_SERVER] = {PLUGIN_SERVER, "Server", nil, nil, nil, },
}



local propPlugin_mt = {}
propPlugin_mt.__index = function (_table, _key)
    local lang_idx = propPluginLANGKey[_key]
    if lang_idx then
		local lang_str = propLanguageById(_table[lang_idx])
		local idx_ex = propPluginKey[_key .. "ARG"]
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
    local idx = propPluginKey[_key]
    if not idx then
        return nil
    end
    return _table[idx]
end

function propPluginById(_key_id)
    local id_data = propPluginData[_key_id]
    if id_data == nil then
        return nil
    end
    if getmetatable(id_data) == nil then
        setmetatable(id_data, propPlugin_mt)
    end
    return id_data
end


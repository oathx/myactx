--
-- $Id$
--

module( "resmng" )
propLocalLANGKey = {

}

propLocalKey = {
ID = 1, IPAddress = 2, Port = 3, MainLogicScriptPath = 4, Version = 5, 
}

propLocalData = {

	[GAME_LOCAL] = {GAME_LOCAL, "127.0.0.1", 9527, "Game/System/SystemPlugin", "0.0.1", },
}



local propLocal_mt = {}
propLocal_mt.__index = function (_table, _key)
    local lang_idx = propLocalLANGKey[_key]
    if lang_idx then
		local lang_str = propLanguageById(_table[lang_idx])
		local idx_ex = propLocalKey[_key .. "ARG"]
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
    local idx = propLocalKey[_key]
    if not idx then
        return nil
    end
    return _table[idx]
end

function propLocalById(_key_id)
    local id_data = propLocalData[_key_id]
    if id_data == nil then
        return nil
    end
    if getmetatable(id_data) == nil then
        setmetatable(id_data, propLocal_mt)
    end
    return id_data
end


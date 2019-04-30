--
-- $Id$
--

module( "resmng" )
propSystemLANGKey = {

}

propSystemKey = {
ID = 1, Name = 2, ClassType = 3, Active = 4, 
}

propSystemData = {

	[SYS_LOGIN] = {SYS_LOGIN, "Login", "LoginSystem", 1, },
	[SYS_HOME] = {SYS_HOME, "Home", "HomeSystem", 1, },
	[SYS_CONTROLLER] = {SYS_CONTROLLER, "Controller", "ControllerSystem", 1, },
	[SYS_TASK] = {SYS_TASK, "Task", "TaskSystem", 1, },
	[SYS_FIGHT] = {SYS_FIGHT, "Fight", "FightSystem", 1, },
	[SYS_MASTER] = {SYS_MASTER, "Master", "MasterSystem", 1, },
}



local propSystem_mt = {}
propSystem_mt.__index = function (_table, _key)
    local lang_idx = propSystemLANGKey[_key]
    if lang_idx then
		local lang_str = propLanguageById(_table[lang_idx])
		local idx_ex = propSystemKey[_key .. "ARG"]
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
    local idx = propSystemKey[_key]
    if not idx then
        return nil
    end
    return _table[idx]
end

function propSystemById(_key_id)
    local id_data = propSystemData[_key_id]
    if id_data == nil then
        return nil
    end
    if getmetatable(id_data) == nil then
        setmetatable(id_data, propSystem_mt)
    end
    return id_data
end


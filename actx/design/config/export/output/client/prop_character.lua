--
-- $Id$
--

module( "resmng" )
propCharacterLANGKey = {

}

propCharacterKey = {
ID = 1, Skeleton = 2, UISkeleton = 3, Shape = 4, Version = 5, 
}

propCharacterData = {

	[CHAR_GRIL] = {CHAR_GRIL, "Hero/kui/H_Kui_01/H_Kui_01", "Hero/kui/H_Kui_01/XSimpleAliveH_Kui_01", {10001, 10002, 10003, 10004, 10005, 10006, 10007, 10008}, 1, },
}



local propCharacter_mt = {}
propCharacter_mt.__index = function (_table, _key)
    local lang_idx = propCharacterLANGKey[_key]
    if lang_idx then
		local lang_str = propLanguageById(_table[lang_idx])
		local idx_ex = propCharacterKey[_key .. "ARG"]
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
    local idx = propCharacterKey[_key]
    if not idx then
        return nil
    end
    return _table[idx]
end

function propCharacterById(_key_id)
    local id_data = propCharacterData[_key_id]
    if id_data == nil then
        return nil
    end
    if getmetatable(id_data) == nil then
        setmetatable(id_data, propCharacter_mt)
    end
    return id_data
end


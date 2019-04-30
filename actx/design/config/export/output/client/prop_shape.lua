--
-- $Id$
--

module( "resmng" )
propShapeLANGKey = {

}

propShapeKey = {
ID = 1, Path = 2, 
}

propShapeData = {

	[GRIL_HAIR] = {GRIL_HAIR, "Hero/kui/H_Kui_01/hair", },
	[GRIL_HEAD] = {GRIL_HEAD, "Hero/kui/H_Kui_01/head", },
	[GRIL_BODYCLOTH] = {GRIL_BODYCLOTH, "Hero/kui/H_Kui_01/Kui_bodycloth", },
	[GRIL_BODYMETAL] = {GRIL_BODYMETAL, "Hero/kui/H_Kui_01/Kui_bodymetal", },
	[GRIL_BODYSKIN] = {GRIL_BODYSKIN, "Hero/kui/H_Kui_01/Kui_bodyskin", },
	[GRIL_HANDMETAL_2] = {GRIL_HANDMETAL_2, "Hero/kui/H_Kui_01/Kui_handmetal-2", },
	[GRIL_HANDSKIN_2] = {GRIL_HANDSKIN_2, "Hero/kui/H_Kui_01/Kui_handskin-2", },
	[GRIL_HANDCLOTH_2] = {GRIL_HANDCLOTH_2, "Hero/kui/H_Kui_01/Kui_handcloth-2", },
}



local propShape_mt = {}
propShape_mt.__index = function (_table, _key)
    local lang_idx = propShapeLANGKey[_key]
    if lang_idx then
		local lang_str = propLanguageById(_table[lang_idx])
		local idx_ex = propShapeKey[_key .. "ARG"]
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
    local idx = propShapeKey[_key]
    if not idx then
        return nil
    end
    return _table[idx]
end

function propShapeById(_key_id)
    local id_data = propShapeData[_key_id]
    if id_data == nil then
        return nil
    end
    if getmetatable(id_data) == nil then
        setmetatable(id_data, propShape_mt)
    end
    return id_data
end


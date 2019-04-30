
function gfRoundInt(value)
	return math.floor(value + 0.51)
end

function gfPrecisionFixedFloat(f)
    local flag = f >= 0 and 1 or -1
    return gfRoundInt(math.abs(f) * 1000) * 0.001 * flag
end

function gfPrecisionFixedVector3(v)
    return Vector3(gfPrecisionFixedFloat(v.x), gfPrecisionFixedFloat(v.y), gfPrecisionFixedFloat(v.z))
end
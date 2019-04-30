module("XProperty", mkcall)

XOR_RANDOM_UPPER 			= 100000
ADD_RANDOM_UPPER 			= 10000

FLOAT_PRECISION 			= 0.001
FLOAT_PRECISION_COMPRESS 	= 1000

-- value type
XValueType = {
	Int = 1, Float = 2
}

function new(value, vtype)
    value = value or 0
    vtype = vtype or XValueType.Int

    local property = {_type_ = vtype}
    setmetatable(property, {
	    __index = XProperty,
	    __call = function(obj, val)
	        if val then
	            obj:Set(val)
	        else
	            return obj:Get()
	        end
	    end,
	})
    property:Init(value)
    
    return property
end

function Init(self, value)
    -- if value changed, call this func    
    self:Set(value)
end

function Set(self, value)
    local xor = math.random(XOR_RANDOM_UPPER)
    local add = math.random(-ADD_RANDOM_UPPER, ADD_RANDOM_UPPER)

    self._xor_ = xor
    self._add_ = add
    
    if self._type_ == XValueType.Int then
        self._encoded_ = bit.bxor(value, xor) + add
    elseif self._type_ == XValueType.Float then
        self._encoded_ = bit.bxor(math.floor(value * FLOAT_PRECISION_COMPRESS), xor) + add
    else
        error("XProperty can't support type")
    end

end

function Get(self)
    local value = bit.bxor(self._encoded_ - self._add_, self._xor_)
    if self._type_ == XValueType.Float then
        value = value * FLOAT_PRECISION
    end

    return value
end
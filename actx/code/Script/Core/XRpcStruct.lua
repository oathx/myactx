
Struct = {}
mkcall(Struct)

local mt = {
    __index = Struct,
    __call = function(self,...)
        return self:get_data()
    end,
}

function Struct.new(typename)
    local struct = {}
    setmetatable(struct, mt)
    struct:init(typename)
    return struct
end

function Struct.init(self, typename)
    if typename then
        if not RpcType[typename] then
            error("[Struct:init] RpcType required, got unknown type, "..typename)
        end
    end
    typename = typename or ""

    self._type_ = typename
    self._data_ = {}
end

function Struct.check(self, struct)
    return type(struct)=="table" and getmetatable(struct)==mt
end

function Struct.write(self, ar)
    local rpc = RpcType[self._type_]
    assert(rpc)

    ar:writeString(self._type_)
    rpc:_write(ar, self._data_)
end

function Struct.read(self, ar)
    self._type_ = ar:readString()
    local rpc = RpcType[self._type_]
    assert(rpc)

    self._data_ = rpc:_read(ar)
end

function Struct.get_data(self)
    return self._data_
end

function Struct.set_data(self, data_array)
    self._data_ = data_array
end


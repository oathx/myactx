RpcType = {}

RpcType.int = {
    _write=function( packet, v )
        packet:WriteInt(v)
    end,
    _read=function( packet )
        return packet:ReadInt()
    end
}

RpcType.uint = {
    _write=function( packet, v )
        packet:WriteUint(v)
    end,
    _read=function( packet )
        return packet:ReadUint()
    end
}

RpcType.float = {
    _write=function( packet, v )
        packet:WriteFloat(v)
    end,
    _read=function( packet )
        return packet:ReadFloat()
    end
}

RpcType.string = {
    _write=function( packet, v )
        packet:WriteString(v)
    end,
    _read=function( packet )
        return packet:ReadString()
    end
}

RpcType.pack = {
    _write=function( packet, v )
        packet:WriteBlock(cmsgpack.pack(v))
    end,
    _read=function( packet )
        return cmsgpack.unpack(packet:ReadBlock())
    end,
    _check=function(v)
        return type(v) == "table"
    end,
}

RpcType.array = {
    _write=function( packet, v )
        v:write(packet)
    end,
    _read=function( packet )
        local array = Array()
        array:read(packet)
        return array
    end,
    _check=function( v )
        return Array:check(v)
    end,
}

RpcType.struct = {
    _write=function( packet, v )
        v:write(packet)
    end,
    _read=function( packet )
        local struct = Struct()
        struct:read(packet)
        return struct
    end,
    _check=function( v )
        return Struct:check(v)
    end,
}

RpcType.vector = {
    _write=function( packet, v )
        packet:WriteFloat(v.x)
        packet:WriteFloat(v.y)
        packet:WriteFloat(v.z)
    end,
    _read=function( packet )
        local vec = Vector3()
        vec.x = packet:ReadFloat()
        vec.y = packet:ReadFloat()
        vec.z = packet:ReadFloat()
        return vec
    end,
    _check=function( v )
        return getmetatable(v) == "Vector3"
    end,
}

RpcType.__struct = {
}



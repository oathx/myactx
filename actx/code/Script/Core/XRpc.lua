local hashString = XUtility.HashString

local NumberType={
    float=true,
    int=true,
    word=true,
    byte=true,
}

local function _is_typeof(i,s,v)
    -- such as string type
    if s=="string" then 
        if s==type(v) then return true else return false end
    end
    
    -- if it's a number type
    if NumberType[s] and type(v)=="number" then return true end

    if type(v) == "nil" then
        return false
    end
    
    -- if it's a user defined type
    local ut = RpcType[s]
    if ut then
        -- if has check function
        if ut._check then
            if ut._check(v) then
                return true
            else
                error(string.format("bad argument %d, expected %s, but got %s",i,s,type(v)))
            end
        else
            return true 
        end
    else
        error(string.format("can't find user defined type %s, argument %d",s,i))    
    end
    
    return false
end

local function _make(rpc, name, ...)
    local rf = rpc._remote_func_[name]
    if  not rf then
        error(string.format("can't find remote function named %s",key))
        return nil 
    end
    
    local arg={...}
    if #arg ~= #rf.args then
--        error(string.format("expected %d arguments, but passed in %d",#rf.args,#arg))
    end
    
    local packet = XNetLuaPacket(rf.id)

    for i, v in ipairs(arg) do
        local t = rf.args[i].t
        if t and _is_typeof(i,t,v) then
            RpcType[t]._write(packet, v)
        else
            error(string.format("bad argument %d, expected %s, but a %s",i,t,type(v)))
        end

    end

    return packet
end

local function _parse(rpc, packet)
    local rfid = packet:Type()
    local rf = rpc._local_func_[rfid]
    if not rf then
        --error(string.format("rfid(%d) not found in local func", rfid))
        return false
    end

    local args={}
    for i,v in ipairs(rf.args) do
        local tm = RpcType[v.t]
        args[i] = tm._read(packet)
    end
    
    local dealed = false
    local lf = NetImp[rf.name]
    if lf then
        lf(unpack(args))
        dealed = true
    end
    
    local handlers = rpc._dynamic_handler_[rf.name]
    if handlers then
        for owner, func in pairs(handlers) do
            func(owner, unpack(args))
            dealed = true
        end
    end

    return dealed, rf.name
end

local function _parse_function(funcimpl)
    local rf = {args={}}
    for t,n in string.gmatch(funcimpl,"(%w+)%s+(%w+)") do
        table.insert(rf.args, {t=t, n=n})
    end
    return rf
end

local function _parse_protocol(rpc, protocol)
    local rpc_server = {}
    local rpc_client= {}

    -- parse server
    for k, v in pairs(protocol.Server) do
        rpc_server[k] = _parse_function(v)
        rpc_server[k].id = hashString(k)
        print(rpc_server[k].name, rpc_server[k].id)
    end

    -- parse client
    for k, v in pairs(protocol.Client) do
        local rf = _parse_function(v)
        rf.id = hashString(k)
        rf.name = k
        rpc_client[rf.id] = rf
    end

    rpc._remote_func_ = rpc_server
    rpc._local_func_ = rpc_client
end

local function _parse_rpc_type()
    local parseStruct=function(k,v)
        local desc = _parse_function(v).args
        -- build rpc type
        RpcType[k] = {
            _write=function(packet, v)
                for i, arg in ipairs(desc) do
                    local rt = RpcType[arg.t]
                    rt._write(packet, v[arg.n])
                end
            end,
            _read=function(packet)
            local ret={}
            for i, arg in ipairs(desc) do
                local rt = RpcType[arg.t]
                local data = rt._read(packet)
                ret[arg.n] = data
            end
            return ret
            end,
            _check=function(v)
                for i, arg in ipairs(desc) do
                    local rt = RpcType[arg.t]
                    if rt._check and not rt._check(v[arg.n]) then
                        return false
                    end
                    return true
                end
            end,
        }
    end

    for k,v in pairs(RpcType.__struct) do
        parseStruct(k,v)
    end
end

local function _register(rpc, protocol, owner, func)
    local lf = NetImp[protocol]
    if not lf then
        NetImp[protocol] = function() end
    end

    local list = rpc._dynamic_handler_[protocol]
    if not list then
        list = {}
        setmetatable(list, { __mode="k" })
        rpc._dynamic_handler_[protocol] = list
    end

    list[owner] = func
end

local function _unregister(rpc, protocol, owner)
    local list = rpc._dynamic_handler_[protocol]
    if list then
        list[owner] = nil
    end
end

local function _parse_tcp_server(rpc)
    local prop = resmng.propLocalById(resmng.GAME_LOCAL)
    if prop then
        local InternalPacketType = {
            [PacketType.SOCKET_CONNECT_SUCCESS] = NetImp.NetConnectSuccess,
            [PacketType.SOCKET_CONNECT_FAILURE] = NetImp.NetConnectFailure,
            [PacketType.SOCKET_DISCONNECT]      = NetImp.NetDisconnect
        }

        local tcp = TcpServerConfig()
        tcp.ipAddress = prop.IPAddress
        tcp.ipPort    = prop.Port
        tcp.timeOut   = 0
        tcp.netFunc   = function(packet)
            local ptype = packet:Type()
            if ptype <= PacketType.SOCKET_DISCONNECT then
                local func = InternalPacketType[ptype]
                if func then
                    func()
                end
            else
                rpc:Parse(packet)
            end
        end

        XTcpServer.GetSingleton():Startup(tcp)
    else
        ELOG("_parse_tcp_server error, can't find local config")
    end
end

local function _init(rpc, protocol)
    _parse_protocol(rpc, protocol)
    _parse_rpc_type()
    _parse_tcp_server(rpc)

    rpc._dynamic_handler_ = {}
end

local mt = {
    __index = function( table, key )
        return function(rpc, ...)
            local packet = rpc:Make(key, ...)
            if packet then
                XTcpServer.GetSingleton():Send(packet)
            end
        end
    end
}

local function new()
    local instance = {
        Init = _init,
        Make = _make,
        Register = _register,
        Unregister = _unregister,
        Parse = _parse
    }
    setmetatable(instance, mt)

    return instance
end

XRpc = new()
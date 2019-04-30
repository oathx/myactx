module("XEventManager", package.seeall)

event_listeners_    = event_listeners_ or {}
listeners_          = listeners_ or {}
trigering_event_    = {}
pending_register_   = {}
pending_unregister_ = {}
listener_id_        = 1

function Trigger(event_name, ...)
    local listener = event_listeners_[event_name]
    if listener then
        trigering_event_[event_name] = true

        for _, data in ipairs(listener[2]) do
            local callback = data[2]
            if callback then
                callback(...)
            end
        end

        local pending = pending_register_[event_name]
        if pending and #pending > 0 then
            for _, data in ipairs(pending) do
                do_register(listener, data) 
            end
            pending_register_[event_name] = {}
        end

        pending = pending_unregister_[event_name]
        if pending and #pending > 0 then
            for _, id in ipairs(pending) do
                do_unregister(listener, id)
            end 
            pending_unregister_[event_name] = {}
        end

        trigering_event_[event_name] = false
    end
end

function Register(event_name, callback)
    local id = listener_id_
    listener_id_ = listener_id_ + 1

    local event_listener = event_listeners_[event_name]
    if not event_listener then
        event_listener = {event_name, {}}
        event_listeners_[event_name] = event_listener
    end

    if trigering_event_[event_name] then
        local pending = pending_register_[event_name] or {}
        table.insert(pending, {id, callback})
        pending_register_[event_name] = pending

    else
        DoUnregister(event_listener, {id, callback})
    end

    listeners_[id] = event_listener
    return id
end


function Unregister(id)

    local event_listener = listeners_[id]
    if not event_listener then return end

    local event_name = event_listener[1]
    if trigering_event_[event_name] then
        local pending = pending_unregister_[event_name] or {}
        table.insert(pending, id)
        pending_unregister_[event_name] = pending

    else
        DoUnregister(event_listener, id)
    end

    listeners_[id] = nil
end


function DoRegister(event_listener, data)
    table.insert(event_listener[2], data)
end

function DoUnregister(event_listener, id)
    local list = event_listener[2]
    for idx, data in ipairs(list) do
        if data[1] == id then
            table.remove(list, idx)
            break
        end
    end
end

function Reset()
    event_listeners_ = {}
    listeners_ = {}

    trigering_event_ = {}
    pending_register_ = {}
    pending_unregister_ = {}
end

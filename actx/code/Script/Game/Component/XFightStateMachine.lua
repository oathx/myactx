module("XFightStateMachine", package.seeall)
setmetatable(XFightStateMachine, {__index=XEntityComponent})

function new(entity)
    local machine = {
    }
    setmetatable(machine, {__index=XFightStateMachine})
    machine:Init(entity)

    return machine
end

function Init(self, entity)
    XEntityComponent.Init(self, entity)

    self.stateList = {}
    self.stateMap = {}

    -- must be add MonoBehaviour
    self:AddBehaviour(GameObject(tostring(entity:cid())))
    
    if self.gameObject then
        GameObject.DontDestroyOnLoad(self.gameObject)
    end   
end

function AddState(self, name, fnEnter, fnUpdate, fnExit)
    local state = {
    }

    state.name      = name
    state.fnEnter   = fnEnter
    state.fnExit    = fnExit
    state.fnUpdate  = fnUpdate
    state.enterFlag = false

    self.stateMap[name] = state

    return true
end

function GetState(self, name)
    return self.stateMap[name]
end

function SetGlobalState(self, state)
    self.globalState = state
end

function ClearToGlobalOrBlockState(self, name)
    local count = #self.stateList
    if count > 0 then
        for idx=count, 1, -1 do
            local state = self.stateList[idx]
            if state.name == name then
                return true  
            end

            if state.fnExit then
                state.fnExit(self)
            end

            table.remove(self.stateList, idx)
        end
    end

    return false
end

function PushState(self, name)
    local state = self:GetState(name)
    if not state then
        ERROR("Can't find state %s", name)
        return false
    end

    local count = #self.stateList
    if count > 0 then 
        if self.stateList[count].name == name then
            return false
        end
    end

    table.insert(self.stateList, state)

    state.enterFlag = false
    if state.fnEnter then
        if self.onStateBefore then
            self.onStateBefore(self)
        end

        state.fnEnter(self)
    end
    state.enterFlag = true

    if self.OnStateChanged then
        self.OnStateChanged(self, state)
    end

    return true
end

function PopState(self, name)
    local count = #self.stateList
    if count > 0 then
        local state = self.stateList[count]
        if state.fnExit then
            state.fnExit(self)
        end

        table.remove(self.stateList, count)
    end

    count = #self.stateList
    if count > 0 then
        if self.OnStateChanged then
            self.OnStateChanged(self, self.stateList[count])
        end
    end
end

function SetState(self, name)
    local newState = self:GetState(name)
    if not newState then
        ERROR("Can't find state %s", name)
        return false
    end

    local oldState = self:GetCurrentState()
    if not oldState or oldState.name ~= name then
        local stateName = "NullState"
        if self.globalState then
            stateName = self.globalState.name
        end

        self:ClearToGlobalOrBlockState(stateName)
        self:PushState(name)
    end
end

function IsCurrent(self, name)
    local state = self:GetCurrentState()
    if state then
        return state.name == name 
    end

    return false
end

function IsExist(self, name)
    return self:GetState(name) and true or false
end

function GetCurrentState(self)
    local count = #self.stateList
    if count > 0 then
        return self.stateList[count]
    end
end

function UpdateStateMachine(self, deltaTime)
    local max = #self.stateList
    if max > 0 then
        local state = self.stateList[max]
        if state.enterFlag and state.fnUpdate then
            self.stateList[max].fnUpdate(self, deltaTime)
        end
    end
end

function Awake(self)
end

function Start(self)
end

function Update(self)
    self:UpdateStateMachine(Time.deltaTime)
end


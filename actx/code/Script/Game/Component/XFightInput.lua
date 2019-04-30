module("XFightInput", package.seeall)
setmetatable(XFightInput, {__index=XEntityComponent})

PlayerAction = resmng.PlayerAction
XJoystCode = {
	JOYST_NONE = 0,
	JOYST_LEFT_DOWN = 1,
	JOYST_DOWN = 2,
	JOYST_RIGHT_DOWN = 3,
	JOYST_LEFT = 4,
	JOYST_CENTER = 5,
	JOYST_RIGHT = 6,
	JOYST_LEFT_UP = 7,
	JOYST_UP = 8,
	JOYST_RIGHT_UP = 9,
}

MAX_INPUT_DELAY = 0.05
MAX_JOYST_DELAY = 0.02
MAX_DODGE_DELAY = 0.5

function new(entity)
	local comp = {
	}
	setmetatable(comp, {__index=XFightInput})
	comp:Init(entity)

	return comp
end

function Init(self, entity)
	XEntityComponent.Init(self, entity)

	self.paused 			= false
	self.curJoystickStatus 	= XJoystCode.JOYST_CENTER
	self.curExecuteJoyCode	= XJoystCode.JOYST_NONE
	self.curPressTimestamp	= 0
    self.lastForwardTime 	= 0
    self.lastBackwardTime 	= 0
	self.joystDelayTime		= MAX_JOYST_DELAY
    self.inputDelayTime		= MAX_INPUT_DELAY
    self.execQueue			= {}
    self.queuedAction		= nil
    self.actionFlip 		= false   
end

function Reset(self)
	self.execQueue			= {}
	self.queuedAction		= nil
end

function SetjoystickHook(self, callback)
	self.joystickHook = callback
end

function GetTimestamp(self)
	return Time.realtimeSinceStartup
end

function PostEvent(self, act)
	if self.joystickHook then
		self.joystickHook(self.entity, act)
	else
		self:Enqueue(act)
	end
end

function Enqueue(self, act)
	local exec = self.execQueue[#self.execQueue]
	if exec then
		if exec.action == act then
			return false
		end
	end

	local queuedAction = self.queuedAction
	if queuedAction then
		if queuedAction and queuedAction.exec.action == act then
			return false
		end
	end

	table.insert(self.execQueue, {
		action = act, timestamp = self:GetTimestamp()
	})

	return true
end

function Paused(self, pause)
	self.paused = pause
end

function IsPause(self)
	return self.paused
end

function PostJoystickCode(self, code)
	if code == XJoystCode.JOYST_CENTER then
		self:PostEvent(PlayerAction.Stay)
	elseif code == XJoystCode.JOYST_LEFT then
		self:PostEvent(PlayerAction.WalkLeft)
	elseif code == XJoystCode.JOYST_RIGHT then
		self:PostEvent(PlayerAction.WalkRight)
	elseif code == XJoystCode.JOYST_UP then
		self:PostEvent(PlayerAction.Jump)
	elseif code == XJoystCode.JOYST_DOWN then
		self:PostEvent(PlayerAction.Crouch)
	elseif code == XJoystCode.JOYST_LEFT_UP then
		self:PostEvent(PlayerAction.JumpBackward)
	elseif code == XJoystCode.JOYST_RIGHT_UP then
		self:PostEvent(PlayerAction.JumpForward)
	elseif code == XJoystCode.JOYST_RIGHT_DOWN then
		self:PostEvent(PlayerAction.Crouch)
	elseif code == XJoystCode.JOYST_LEFT_DOWN then
		self:PostEvent(PlayerAction.Crouch)
	end

	if code ~= XJoystCode.JOYST_CENTER then
        if code ~= XJoystCode.JOYST_LEFT then
            self.lastBackwardTime = -1
        end

        if code ~= XJoystCode.JOYST_RIGHT then
            self.lastForwardTime = -1
        end
    end
end

function FixedFrameUpdate(self, deltaTime)
	if self.paused then
		return false
	end

	if _DEBUG then
		self:UpdateKeyEvent()
	end

	if self.curExecuteJoyCode ~= XJoystCode.JOYST_NONE then
		if self:GetTimestamp() - self.curPressTimestamp >= self.joystDelayTime then
			self:PostJoystickCode(self.curExecuteJoyCode)
			self.curExecuteJoyCode = XJoystCode.JOYST_NONE
		end
	end

	local queued = self.queuedAction
	if queued then
		local flag = true
		if queued.invalidTime >= self:GetTimestamp() then
			flag = self:ExecuteAction(queued.exec)
		end

		if flag then
			self.queuedAction = nil
		end
	end

	self:ProcessUpdateQueue()

	return true
end

JoyHandleCode = {
    PlayerAction.Jump,
    PlayerAction.WalkLeft,
    PlayerAction.WalkRight,
    PlayerAction.JumpForward,
    PlayerAction.JumpBackward,
    PlayerAction.Crouch,
}

function IsJoystCode(self, action)
	for idx, code in ipairs(JoyHandleCode) do
		if code == action then
			return true
		end
	end

	return false
end

function GetCacheActionTime( self , action )
    return resmng.NeedQueueCacheAction[action]
end

function ProcessUpdateQueue(self)
	local count = #self.execQueue
	if count <= 0 then
		return false
	end

	local execQueue = {
	}

	for idx, exec in ipairs(self.execQueue) do
		local curTime = self:GetTimestamp()
		local excTime = exec.timestamp + self.inputDelayTime

		if self:IsJoystCode(exec.action) then
			excTime = exec.timestamp + (self.inputDelayTime - self.joystDelayTime)
		end

		if curTime >= excTime or (excTime - curTime) < 0.015 then
			self:TryExecuteAction(exec)
		else
			table.insert(execQueue, exec)
		end
	end

	self.execQueue = execQueue

	return true
end

function IsActionBlocked(self, action)
	return false
end

function TranslateJoystickCodeFlip(self, joystCode)
    if self.actionFlip then
        if joystCode == XJoystCode.JOYST_LEFT then
            joystCode = XJoystCode.JOYST_RIGHT
        elseif joystCode == XJoystCode.JOYST_RIGHT then
            joystCode = XJoystCode.JOYST_LEFT
        elseif joystCode == XJoystCode.JOYST_LEFT_UP then
            joystCode = XJoystCode.JOYST_RIGHT_UP
        elseif joystCode == XJoystCode.JOYST_LEFT_DOWN then
            joystCode = XJoystCode.JOYST_RIGHT_DOWN
        elseif joystCode == XJoystCode.JOYST_RIGHT_UP then
            joystCode = XJoystCode.JOYST_LEFT_UP
        elseif joystCode == XJoystCode.JOYST_RIGHT_DOWN then
            joystCode = XJoystCode.JOYST_LEFT_DOWN
        end
    end

    return joystCode
end

function UpdateKeyEvent(self)
	local joystCode = XJoystCode.JOYST_NONE

	if Input.GetKey(KeyCode.W) then
		if Input.GetKey(KeyCode.A) then
			joystCode = XJoystCode.JOYST_LEFT_UP
		elseif Input.GetKey(KeyCode.D) then
			joystCode = XJoystCode.JOYST_RIGHT_UP
		else
			joystCode = XJoystCode.JOYST_UP
		end
	elseif Input.GetKey(KeyCode.S) then
        if Input.GetKey(KeyCode.A) then
            joystCode = XJoystCode.JOYST_LEFT_DOWN
        elseif Input.GetKey(KeyCode.D) then
            joystCode = XJoystCode.JOYST_RIGHT_DOWN
        else
            joystCode = XJoystCode.JOYST_DOWN
        end
	elseif Input.GetKey(KeyCode.A) then
        if Input.GetKey(KeyCode.W) then
            joystCode = XJoystCode.JOYST_LEFT_UP
        elseif Input.GetKey(KeyCode.S) then
            joystCode = XJoystCode.JOYST_LEFT_DOWN
        else
            joystCode = XJoystCode.JOYST_LEFT
        end
	elseif Input.GetKey(KeyCode.D) then
        if Input.GetKey(KeyCode.W) then
            joystCode = XJoystCode.JOYST_RIGHT_UP
        elseif Input.GetKey(KeyCode.S) then
            joystCode = XJoystCode.JOYST_RIGHT_DOWN
        else
            joystCode = XJoystCode.JOYST_RIGHT
        end
	else
		joystCode = XJoystCode.JOYST_CENTER
	end

	if joystCode ~= XJoystCode.JOYST_NONE and self.curJoystickStatus ~= joystCode then
		self.curJoystickStatus = joystCode

		local decisionCode = self:TranslateJoystickCodeFlip(joystCode)
		if decisionCode then
			self:ProcessJoystickCode(decisionCode)
		end
	end
end

function ProcessJoystickCode(self, code)
	if code == XJoystCode.JOYST_LEFT then
		local nowTimestamp = self:GetTimestamp()
		if nowTimestamp - self.lastBackwardTime < MAX_DODGE_DELAY then
			self:PostEvent(PlayerAction.Dodge)
			nowTimestamp = 0
		end

		self.lastBackwardTime = nowTimestamp
	elseif code == XJoystCode.JOYST_RIGHT then
		local nowTimestamp = self:GetTimestamp()
		if nowTimestamp - self.lastForwardTime < MAX_DODGE_DELAY then
			self:PostEvent(PlayerAction.Dash)
			nowTimestamp = 0
		end

		self.lastForwardTime = nowTimestamp
	end

	self.curPressTimestamp = self:GetTimestamp()
	self.curExecuteJoyCode = code
end

function TransFlipActionDirection(self, action)
	local target = action
	if action == PlayerAction.WalkLeft then
        return PlayerAction.WalkRight
    elseif action == PlayerAction.WalkRight then
        return PlayerAction.WalkLeft
    elseif action == PlayerAction.JumpForward then
        return PlayerAction.JumpBackward
    elseif action == PlayerAction.JumpBackward then
        return PlayerAction.JumpForward
    end

    return target
end

function ActionDirectionChanged(self, flip)
	self.actionFlip = flip

	for idx, exec in ipairs(self.execQueue) do
		exec.action = self:TransFlipActionDirection(exec.action)
	end

	local queuedAction = self.queuedAction
	if queuedAction then
		queuedAction.exec.action = self:TransFlipActionDirection(queuedAction.exec.action)
	end
end

function TryExecuteAction(self, execAction)
	local flag = self:ExecuteAction(execAction)
	if not flag then
		local cacheTime = self:GetCacheActionTime(execAction.action) or 0
		if cacheTime > 0 then
			self.queuedAction = {
				invalidTime = self:GetTimestamp() + cacheTime, exec = execAction
			}
		end
	end

	return flag
end

function ExecuteAction(self, execAction)
	return true
end



module("XPlayerInput", package.seeall)
setmetatable(XPlayerInput, {__index=XFightInput})

function new(entity)
	local comp = {
	}
	setmetatable(comp, {__index = XPlayerInput})
	comp:Init(entity)

	return comp
end

function Init(self, entity)
	XFightInput.Init(self, entity)

	-- get the hero shape
	self.shape = entity:GetShape(XHeroShape)
	if not self.shape then
		ERROR("Can't find render shape")
	end	
end

function ExecuteAction(self, execAction)
	local action = execAction.action
	local result = true
	local flag = false
	local reason

	if not self:IsActionBlocked(action) then
		if action == PlayerAction.Attack then
			result, reason = self:TryAttack(execAction)
		else
			result = self:InflictAction(action)
		end
	end

	return result
end

function TryAttack(self, execAction)
	local level = resmng.AttackLevel.Light
	if not self.shape:IsGround() then
		level = resmng.AttackLevel.Jump1
	end

	if self.shape:CanAttack(level) then
		return self.shape:DoAttack(level)
	end
end

function InflictAction(self, action)
	local result = true

	if action == PlayerAction.Stay then
		result = self.shape:DoStay()
	elseif action == PlayerAction.WalkLeft then
		result = self.shape:DoWalkBackward()
	elseif action == PlayerAction.WalkRight then
		result = self.shape:DoWalkForward()
	elseif action == PlayerAction.Crouch then
		result = self.shape:DoCrouch()
	elseif action == PlayerAction.Jump then
		result = self.shape:DoJump(0)
	elseif action == PlayerAction.JumpForward then	
		result = self.shape:DoJump(1)
	elseif action == PlayerAction.JumpBackward then	
		result = self.shape:DoJump(2)
	end

	return result
end

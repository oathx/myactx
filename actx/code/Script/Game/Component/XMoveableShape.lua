module("XMoveableShape", package.seeall)
setmetatable(XMoveableShape, {__index=XSimpleShape})

AnimatorStateValue = resmng.AnimatorStateValue
AnimationEventType = resmng.AnimationEventType

WalkState = {
	Stay 			= 0,
	Forward 		= 1,
	Backward 		= 2
}

MAX_DISTANCE_BETWEEN_HERO = 3

function new(entity, go)
	local shape = {
	}
	setmetatable(shape, {__index=XMoveableShape})
	shape:Init(entity, go)

	return shape
end

function Init(self, entity, go)
	XSimpleShape.Init(self, entity, go)

	self.ignorePos = false
	self.isOnStage = true
	self.moveSpeed = 0
	self.isBodyMeeting = false
	self.isTransMeeting = false
	self.isBreakBodyPosition = false
	self.isBreakTransPosition = false
	self.forceMoveX = 0
	self.renderForceMoveX = 0
	self.lastX = 0
	self.radius = 0.6
	self.radiusModifier = 0.2
	self.forceWalk = false
	self.offsetY = 0
	self.actionFlip = false
	self.fliping = false
	self.isGround = true
end

function Awake(self)
	XSimpleShape.Awake(self)

	self.collider = self.gameObject:GetComponent(CapsuleCollider)
	if self.collider then
		self.radius = self.collider.radius
	end

	self:InitFight()
end

function Start(self)
	local born = self.entity:GetBorn() or Vector3.zero
	if born then
		self:SetPosition(born)
	end

	self:Flipped(not self.entity:IsMaster())
	self:BreakNextBodyPosition()
end

function InitFight(self)
	self.fight = XFightManager.GetSingleton():GetCurrentFight()
	if not self.fight then
		ELOG("Can't find current fight object")
		return  false
	end

	self.fightLogic = self.fight:GetComponent(XFightLogic)

	return true
end

function InitComponent(self)
	XSimpleShape.InitComponent(self)

	-- set box internal callback
	self.boxComp.BodyMeeting = function(forceX, bodyPos)
		self:OnBodyMeeting(forceX, bodyPos)
	end

	self.boxComp.TransMeeting = function(forceX, transPos)
		self:OnTransMeeting(forceX, transPos)
	end

	self.boxComp.ReceiveAttackWarning = function(enter)
		self:OnReceiveAttackWarning(enter)
	end

	self.boxComp.HugOther = function(evtType, hugType, revID)
		self:OnHugOther(evtType, hugType, revID)
	end
end

-- Internal callbacks when logical boxes meet
function OnBodyMeeting(self, forceX, bodyPos)
	if not self.isOnStage or not self.bodyPosition or self.isBreakBodyPosition then
		return
	end

	if forceX >= 0 then
		self.isBodyMeeting = true
	end
	
	self.bodyPosition = bodyPos

	if not self.entity:IsSync() then
		self.transform.position = self.bodyPosition
	end
end

-- Internal callbacks when render transform meet
function OnTransMeeting(self, forceX, transPos)
	if not self.isOnStage or not self.renderPosition or self.isBreakTransPosition then
		return
	end

	if forceX >= 0 then
		self.isTransMeeting = true
	end

	self.renderPosition = transPos

	if not self.ignoreMove then
		self.transform.position = self.renderPosition
	end	
end

-- Internal callbacks when attack warning
function OnReceiveAttackWarning(self, enter)
end

-- Internal callbacks when hug other
function OnHugOther(self, evtType, hugType, revID)
end

function InitAnimator(self)
	if self.entity:IsSync() then
		self.animator = self.gameObject:GetComponent(XSmoothAnimator)
		if not self.animator then
			self.animator = self.gameObject:AddComponent(XSmoothAnimator)
		end

		self:EnableAnimator(false)
	else
		self.animator = self.gameObject:GetComponent(Animator)
		self:EnableAnimator(true)
	end

	self:RegisterAnimationEventCallback(AnimationEventType.BodyBox, self.OnAnimationBodyBox)
	self:RegisterAnimationEventCallback(AnimationEventType.ForwardStart,self.OnAnimationForwardStart)
	self:RegisterAnimationEventCallback(AnimationEventType.ForwardEnd, self.OnAnimationForwardEnd)
	self:RegisterAnimationEventCallback(AnimationEventType.JumpStart, self.OnAnimationJumpStart)
	self:RegisterAnimationEventCallback(AnimationEventType.JumpEnd, self.OnAnimationJumpEnd)
	self:RegisterAnimationEventCallback(AnimationEventType.BackwardStart, self.OnAnimationBackwardStart)
	self:RegisterAnimationEventCallback(AnimationEventType.BackwardEnd, self.OnAnimationBackwardEnd)
end

function UpdateAnimationEvent(self)
	local events = self.luaComp:GetFrameEvents()
    if not events then 
    	return false
    end

    events = events.Table
	
    for idx, event in ipairs(events) do
        if event.animatorStateInfo.shortNameHash == self.curStateHash then
            self:OnAnimationDispatchEvent(event)
        end
    end
end

function OnAnimationDispatchEvent(self, event)
	local func = self.dealAnimationEventFuncs[event.functionName]
	if func then
		return func(self, event)
	end
end

function OnAnimationBodyBox(self, event)
	if self.boxComp then
		self.boxComp:AnimationBodyBox(event)
	end	
end

function OnAnimationForwardStart(self, event)
	local moveCurve = XUtility.ToMoveCurve(event)
	if moveCurve then
		local bpos = self:GetBodyPosition()
		
		self.boxForward = true
		self.boxForwardCurve = moveCurve.curve
		self.boxForwardEndTime = moveCurve.endTime
		self.boxForwardStartTime = event.time
		self.boxForwardTime = event.time
		self.boxForwardX = bpos.x
		self.boxForwardLastStep = 0
		self.boxForwardState = event.animatorStateInfo.shortNameHash

		local rpos = self:GetRenderPostion()
		self.renderForward = true
		self.renderForwardCurve = moveCurve.curve
		self.renderForwardEndTime = moveCurve.endTime
		self.renderForwardStartTime = event.time
		self.renderForwardTime = event.time
		self.renderForwardX = rpos.x
		self.renderForwardLastStep = 0
		self.renderForwardState = event.animatorStateInfo.shortNameHash
	end

	self:ForceSyncBodyBox()
end

function OnAnimationForwardEnd(self, event)
	self:ForceSyncBodyBox()
end

function OnAnimationJumpStart(self, event)
	local moveCurve = XUtility.ToMoveCurve(event)
	if moveCurve then
		local bpos = self:GetBodyPosition()
		
		self.boxJump = true
		self.boxJumpCurve = moveCurve.curve
		self.boxJumpEndTime = moveCurve.endTime
		self.boxJumpStartTime = event.time
		self.boxJumpTime = event.time
		self.boxJumpY = bpos.y
		self.boxJumpLastStep = 0
		self.boxJumpState = event.animatorStateInfo.shortNameHash

		local rpos = self:GetRenderPostion()
		self.renderJump = true
		self.renderJumpCurve = moveCurve.curve
		self.renderJumpEndTime = moveCurve.endTime
		self.renderJumpStartTime = event.time
		self.renderJumpTime = event.time
		self.renderJumpY = rpos.y
		self.renderJumpLastStep = 0
		self.renderJumpState = event.animatorStateInfo.shortNameHash
	end
end

function OnAnimationJumpEnd(self, event)
	self:ForceSyncBodyBox()
end

function OnAnimationBackwardStart(self, event)
	local moveCurve = XUtility.ToMoveCurve(event)
	if moveCurve then
		local bpos = self:GetBodyPosition()

		self.boxBackward = true
		self.boxBackwardCurve = moveCurve.curve
		self.boxBackwardEndTime = moveCurve.endTime
		self.boxBackwardStartTime = event.time
		self.boxBackwardTime = event.time
		self.boxBackwardX = bpos.x
		self.boxBackwardLastStep = 0
		self.boxBackwardState = event.animatorStateInfo.shortNameHash

		local rpos = self:GetRenderPostion()
		self.renderBackward = true
		self.renderBackwardCurve = moveCurve.curve
		self.renderBackwardEndTime = moveCurve.endTime
		self.renderBackwardStartTime = event.time
		self.renderBackwardTime = event.time
		self.renderBackwardX = rpos.x
		self.renderBackwardLastStep = 0
		self.renderBackwardState = event.animatorStateInfo.shortNameHash
	end
end

function OnAnimationBackwardEnd(self, event)
	self:ForceSyncBodyBox()
end

function DrawGL(self)
	if self.boxComp then
		self.boxComp:DrawGL()
	end		
end

function SetGroundHeight(self, y)
	self.offsetY = y

	if self.boxComp then
		self.boxComp:ApplyGroundHeight(y)
	end
end

function GetGroundHeight(self)
	return self.offsetY
end

function FixedFrameUpdate(self, deltaTime)
	self:UpdateActionDirectionFlip()

	local fixedDeltaTime = deltaTime * 1.01
	XSimpleShape.FixedFrameUpdate(self, 
		fixedDeltaTime)

	self:UpdateMovement(fixedDeltaTime)
end

function FixedInputUpdate(self, deltaTime)
	local fixedDeltaTime = deltaTime * 1.01
	if self.input then
		self.input:FixedFrameUpdate(fixedDeltaTime)
	end

	if self.animator and not self.animator.enabled and self.isOnStage then
		self.animator:Update(fixedDeltaTime)
	end

	self:UpdateStateMachine()
end

function LateFrameUpdate(self, deltaTime)
	local fixedDeltaTime = deltaTime * 1.01
	XSimpleShape.LateFrameUpdate(self, 
		fixedDeltaTime)

	self:UpdateAnimationEvent()
	self:ApplyBodyBoxPosition()
end

function ForceSyncBodyBox(self)
	if self.entity:IsSync() then
		if self.bodyPosition then
			self.renderPosition = Vector3(self.bodyPosition.x, self.bodyPosition.y, self.bodyPosition.z)

			self.transform.position = self.bodyPosition
			self.isBreakBodyPosition = true

			self:UpdateRenderMovement(0)
			self:UpdateRenderPosition(0)

			XBoxSystem.GetSingleton():UpdateOnlyTransPosition()

			self.animator:PlaySync(0)
		end
	end
end

function AdjustAnimatorState(self, deltaTime)
	if self.entity:IsSync() then
		local eventStateHash = self.animator:GetEventStateHash(0)
		local avatarStateHash = self.animator:GetAvatarStateHash(0)

		if not self.preEventStateHash then
			self.preEventStateHash = eventStateHash
		end

		if eventStateHash ~= avatarStateHash then
			self:ForceSyncBodyBox()
		end

		self.preEventStateHash = eventStateHash
	end
end

function IsBound(self)
	local lastX = self.lastX or 0
	local bound = false

	return false
end

function IsWalk(self)
	if self.moveSpeed == 0 then
		return false
	end

	return self:IsCurrentStateOnLayer(StateHashCode.Forward) or self:IsCurrentStateOnLayer(StateHashCode.Backward)
end

function IsGround(self)
	return self.isGround
end

function IsCanMovement(self)
	if self.ignorePos or not self.isOnStage or not self.animator or self.animator.speed <= 0 then
		return false
	end

	if self.entity:IsStatus(resmng.FightStatus.BS_PRISON) then
		return false
	end

	return true
end

function UpdateMovement(self, deltaTime)
	if not self:IsCanMovement() then
		return false
	end

	local flip = self.entity:GetFlip()
	local flag = 1

	if flip then
		flag = -1
	end

	local zero = self.bodyPosition
    local nowX = 0
    local nowY = 0

   	local lastX = zero.x
   	self.lastX = lastX

 	-- walk direction
    local walkState = WalkState.Stay
   	if self.boxForward then
   		self.boxForwardX = zero.x
   		self.boxForwardTime = self.boxForwardTime + deltaTime
   		local percent = math.min(1, (self.boxForwardTime - self.boxForwardStartTime) / (self.boxForwardEndTime - self.boxForwardStartTime))
   		if percent >= 1 then
   			self.boxForward = false
   		end

   		local curveValue = self.boxForwardCurve:Evaluate(percent)
   		if curveValue ~= self.boxForwardLastStep then
   			local distance = (curveValue - self.boxForwardLastStep) * flag

   			local symbol = 1
   			if self.boxForwardX + distance < 0 then
   				symbol = -1
   			end

           	nowX = math.floor(math.abs(self.boxForwardX + distance) * 1000) / 1000
            nowX = nowX * symbol

            self.boxForwardLastStep = curveValue
            walkState = WalkState.Forward
   		end
   	end

   	if self.boxBackward then
   		self.boxBackwardX = zero.x
   		self.boxBackwardTime = self.boxBackwardTime + deltaTime
   		local percent = math.min(1, (self.boxBackwardTime - self.boxBackwardStartTime) / (self.boxBackwardEndTime - self.boxBackwardStartTime))
   		if percent >= 1 then
   			self.boxBackward= false
   		end

   		local curveValue = self.boxBackwardCurve:Evaluate(percent)
   		if curveValue ~= self.boxBackwardLastStep then
   			local distance = (curveValue - self.boxBackwardLastStep) * flag

   			local symbol = 1
   			if self.boxBackwardX - distance < 0 then
   				symbol = -1
   			end
   		
           	nowX = math.floor(math.abs(self.boxBackwardX - distance) * 1000) / 1000
            nowX = nowX * symbol
         
            self.boxBackwardLastStep = curveValue
            walkState = WalkState.Backward
   		end
   	end

   	if self.boxJump then
   		self.boxJumpTime = self.boxJumpTime + deltaTime
   		local percent = math.min(1, (self.boxJumpTime - self.boxJumpStartTime) / (self.boxJumpEndTime - self.boxJumpStartTime))
   		if percent >= 1 then
   			self.boxJump = false
   		end

   		local distance = self.boxJumpCurve:Evaluate(percent)

   		local symbol = 1
        if self.boxJumpY + distance < 0 then
            symbol = -1
        end

		nowY = math.floor(math.abs(self.boxJumpY + distance)*1000)/1000
        nowY = nowY * symbol
   	end

    if self:IsWalk() or self:IsForceWalk() then
    	nowX = self.bodyPosition.x + math.floor(self.moveSpeed * 10 * deltaTime) / 1000 * flag
    	if self.moveSpeed > 0 then
    		walkState = WalkState.Forward
    	else
    		walkState = WalkState.Backward
    	end
    end

    if nowX ~= 0 or walkState ~= WalkState.Stay then
    	zero.x = self:ClampPosition(nowX)
    end

    if nowY ~= 0 then
        zero.y = nowY
        if zero.y < self.offsetY then
            zero.y = self.offsetY
            self.boxJump = false
        end
    else
        if zero.y > self.offsetY then
            zero.y = zero.y - math.floor(9800 * deltaTime)/1000
        end
        if zero.y < self.offsetY then
            zero.y = self.offsetY
        end
    end
    
    local nowGround = not (zero.y > self.offsetY)
    if self.isGround ~= nowGround then
        self.isGround = nowGround
        self:OnGroundChanged(nowGround)
    end

    if not self.bodyForceOpponent then
    	if self.entity:IsMaster() then
    		self.bodyForceOpponent = self:GetEnemy()
    	end

    	if self.entity:IsEnemy() then
    		self.bodyForceOpponent = self:GetMaster()
    	end
    end

    if self.bodyForceOpponent then
    	local shape = self.bodyForceOpponent:GetShape()
    	if shape then
    		local bodyPos = shape:GetBodyPosition()

    		if walkState == WalkState.Forward then
				if (zero.x - lastX > 0 and zero.x > bodyPos.x) or (zero.x - lastX < 0 and zero.x < bodyPos.x) then
		            walkState = WalkState.Backward
		        end
    		end
    			
    		if self.isBodyMeeting and walkState == WalkState.Forward then
				local fixedMove = (zero.x - lastX) * 0.5
	    		shape:SetForceMoveX(fixedMove)
	    		
	    		if not shape:IsBound() then
	    			zero.x = zero.x - fixedMove
	    		end
    		end
    	end
    end

    self.bodyPosition = zero

	return true
end

function ApplyBodyBoxPosition(self)
	if self.ignoreMove or not self.isOnStage or not self.bodyPosition then
		return false
	end

	if self.isBodyMeeting then
		
		self.bodyPosition.x = self:ClampPosition(self.bodyPosition.x + self.forceMoveX)
		self.forceMoveX = 0
		self.isBodyMeeting = false
	end

	local sybomlFlag = 1
	if self.bodyPosition.x < 0 then
		sybomlFlag = -1
	end

	local fixedPosX = gfRoundInt(math.abs(self.bodyPosition.x) * 1000) / 1000 * sybomlFlag
	local pos = Vector3(fixedPosX, self.bodyPosition.y, self.bodyPosition.z)
	self.boxComp:SetNextMovePos(pos, self:IsBound(), self.isBreakBodyPosition)

	if self.isBreakBodyPosition then
		self.isBreakBodyPosition = false
	end
end

function UpdateRenderMovement(self, deltaTime)
	if self.ignoreMove or not self.isOnStage then
		return false
	end

	if self.animator and self.animator.speed <= 0 then
		return false
	end

	local flag = 1
	if self.entity:GetFlip() then
		flag = -1
	end

	if not self.renderPosition then
		self.renderPosition = self.transform.position
	end

	local zero = self.renderPosition
    local nowX = 0
    local nowY = 0

   	local lastX = zero.x
   	self.lastX = lastX

   	local walkState = WalkState.Stay
   	if self.renderForward then
   		self.renderForwardX = zero.x
   		self.renderForwardTime = self.renderForwardTime + deltaTime
   		local percent = math.min(1, (self.renderForwardTime - self.renderForwardStartTime) / (self.renderForwardEndTime - self.renderForwardStartTime))
   		if percent >= 1 then
   			self.renderForward = false
   		end

   		local curveValue = self.renderForwardCurve:Evaluate(percent)
   		if curveValue ~= self.renderForwardLastStep then
   			local distance = (curveValue - self.renderForwardLastStep) * flag

   			local symbol = 1
   			if self.renderForwardX + distance < 0 then
   				symbol = -1
   			end

           	nowX = math.floor(math.abs(self.renderForwardX + distance) * 1000) / 1000
            nowX = nowX * symbol

            self.renderForwardLastStep = curveValue
            walkState = WalkState.Forward
   		end
   	end

  	if self.renderBackward then
   		self.renderBackwardX = zero.x
   		self.renderBackwardTime = self.renderBackwardTime + deltaTime
   		local percent = math.min(1, (self.renderBackwardTime - self.renderBackwardStartTime) / (self.renderBackwardEndTime - self.renderBackwardStartTime))
   		if percent >= 1 then
   			self.renderBackward= false
   		end

   		local curveValue = self.renderBackwardCurve:Evaluate(percent)
   		if curveValue ~= self.renderBackwardLastStep then
   			local distance = (curveValue - self.renderBackwardLastStep) * flag

   			local symbol = 1
   			if self.renderBackwardX - distance < 0 then
   				symbol = -1
   			end

           	nowX = math.floor(math.abs(self.renderBackwardX - distance) * 1000) / 1000
            nowX = nowX * symbol

            self.renderBackwardLastStep = curveValue
            walkState = WalkState.Backward
   		end
   	end

   	if self.renderJump then
   		self.renderJumpTime = self.renderJumpTime + deltaTime
   		local percent = math.min(1, (self.renderJumpTime - self.renderJumpStartTime) / (self.renderJumpEndTime - self.renderJumpStartTime))
   		if percent >= 1 then
   			self.renderJump = false
   		end

   		local distance = self.renderJumpCurve:Evaluate(percent)

   		local symbol = 1
        if self.renderJumpY + distance < 0 then
            symbol = -1
        end

		nowY = math.floor(math.abs(self.renderJumpY + distance)*1000)/1000
        nowY = nowY * symbol
   	end

    if self:IsWalk() or self:IsForceWalk() then
    	nowX = self.renderPosition.x + math.floor(self.moveSpeed * 10 * deltaTime) / 1000 * flag
    	if self.moveSpeed > 0 then
    		walkState = WalkState.Forward
    	else
    		walkState = WalkState.Backward
    	end
    end

    if nowX ~= 0 or walkState ~= WalkState.Stay then
    	zero.x = self:ClampPosition(nowX, false, true)
    end

    if nowY ~= 0 then
        zero.y = nowY
        if zero.y < self.offsetY then
            zero.y = self.offsetY
            self.renderJump = false
        end
    else
        if zero.y > self.offsetY then
            zero.y = zero.y - math.floor(9800 * deltaTime)/1000
        end
        if zero.y < self.offsetY then
            zero.y = self.offsetY
        end
    end

    if not self.bodyForceOpponent then
    	if self.entity:IsMaster() then
    		self.bodyForceOpponent = self:GetEnemy()
    	end

    	if self.entity:IsEnemy() then
    		self.bodyForceOpponent = self:GetMaster()
    	end
    end

    if self.bodyForceOpponent then
    	local shape = self.bodyForceOpponent:GetShape()
    	if shape then
    		local renderPos = shape:GetRenderPostion()

    		if walkState == WalkState.Forward then
				if (zero.x - lastX > 0 and zero.x > renderPos.x) or (zero.x - lastX < 0 and zero.x < renderPos.x) then
		            walkState = WalkState.Backward
		        end
    		end
    			
    		if self.isBodyMeeting and walkState == WalkState.Forward then
				local fixedMove = (zero.x - lastX) * 0.5
	    		shape:SetRenderForceMoveX(fixedMove)
	    		
	    		if not shape:IsBound() then
	    			zero.x = zero.x - fixedMove
	    		end
    		end
    	end
    end

    self.renderPosition = zero

	return true
end

function UpdateRenderPosition(self)
	if self.ignoreMove or not self.isOnStage or not self.renderPosition then
		return false
	end

	if self.isTransMeeting or  self.isBodyMeeting then
		self.renderPosition.x = self:ClampPosition(self.renderPosition.x + self.renderForceMoveX, false, true)
		self.renderForceMoveX = 0
		self.isTransMeeting = false
	end

	self.boxComp:SetNextTransMovePos(self.renderPosition, self:IsBound(), self.isBreakTransPosition)

	if self.isBreakTransPosition then
		self.isBreakTransPosition = false
	end

	return true
end

function UpdateActionDirectionFlip(self)
	local opp = self:GetEnemy()
	if opp then
		local shape = opp:GetShape()
		if not shape then
			return false
		end

		local spos = self:GetBodyPosition()
		local opos = shape:GetBodyPosition()

		if spos.x ~= opos.x then
			local flip = spos.x > opos.x
       		if flip ~= self.actionFlip then
                self.actionFlip = flip
                self:ActionDirectionChanged()

                if self.input then
            		self.input:ActionDirectionChanged(flip)
                end
        	end
		end

		return true
	end
end

function ActionDirectionChanged(self)
	if self.moveSpeed > 0 then
		self.moveSpeed = -100 * resmng.MoveSpeed.PVPBackward
	elseif self.moveSpeed < 0 then
		self.moveSpeed = 100 * resmng.MoveSpeed.PVPForward
	end

	self.animator:SetInteger(AnimatorStateValue.MoveSpeed, self.moveSpeed)

	if self.animator:GetBool(AnimatorStateValue.Jump) then
		local jumpType = self.animator:GetInteger(AnimatorStateValue.JumpType)
		if jumpType > 0 then
			if jumpType == 1 then
				jumpType = 2
			else
				jumpType = 1
			end

			self.animator:SetInteger(AnimatorStateValue.JumpType, jumpType)
		end
	end
end

TurnaroundState = {
	Crouch = "crouch_turnaround",  Stand = "stand_turnaround"
}

function CheckDirection(self)
	if self:IsCrouching() then
		self:UpdateDirection(TurnaroundState.Crouch)
	elseif self.moveSpeed == 0 then
		self:UpdateDirection(TurnaroundState.Stand)
	else
		self:UpdateDirection()
	end
end

function CheckDirection(self, turn)
	if self.fliping and turn then
		return false
	end

	local turnaround = false
	local opponent = self:GetEnemy()
	if not opponent then
		return false
	end

	local flip = self.entity:GetFlip()
	local shape = opponent:GetShape()
	if shape then
		local bpos = shape:GetBodyPosition()
		if flip then
			if bpos.x > self:GetBodyPosition().x then
				turnaround = true
			end
		else
			if bpos.x < self:GetBodyPosition().x then
				turnaround = true
			end
		end

		if turnaround then
			if turn then
				self.animator:Play(turn, 0, 0)
				self.fliping = true
			else
				self:Flipped(not flip)
				self.fliping = false
			end
		end
	end

	return true
end

function FixedFrameRender(self, deltaTime)
	if self.entity:IsSync() and not self.animator.enabled then
		self:UpdateRenderMovement(deltaTime)
		self:UpdateRenderPosition(deltaTime)

		self.animator:Render(deltaTime)
	end
end

function DoStay(self)
	if self:IsForceWalk() then
		self:SetForceWalk(false)
	end

	self.animator:SetInteger(AnimatorStateValue.MoveSpeed, 0)
	self.animator:SetBool(AnimatorStateValue.Crouch, false)
	self.animator:SetBool(AnimatorStateValue.Jump, false)
	self.animator:SetInteger(AnimatorStateValue.JumpType, 0)

	return true
end

function DoWalkForward(self)
	self:DoStay()

	self.moveSpeed = 100 * resmng.MoveSpeed.PVPForward
	self.animator:SetInteger(AnimatorStateValue.MoveSpeed, self.moveSpeed)
	self.animator:SetInteger(AnimatorStateValue.JumpType, 1)

	return true
end

function DoWalkBackward(self)
	self:DoStay()

	self.moveSpeed = -100 * resmng.MoveSpeed.PVPBackward
	self.animator:SetInteger(AnimatorStateValue.MoveSpeed, self.moveSpeed)
	self.animator:SetInteger(AnimatorStateValue.JumpType, 2)

	return true
end

function DoCrouch(self)
	self:DoStay()

	self.animator:SetBool(AnimatorStateValue.Crouch, true)

	return true
end

function DoJump(self, jumpType)
	self.animator:SetInteger(AnimatorStateValue.JumpType, jumpType)
	self.animator:SetBool(AnimatorStateValue.Jump, true)

	self.animator:SetInteger(AnimatorStateValue.MoveSpeed, 0)
	self.animator:SetBool(AnimatorStateValue.Crouch, false)

	return true
end

function GetBodyPosition(self)
	return self.bodyPosition or self.transform.position
end

function GetRenderPostion(self)
	return self.renderPosition or self.transform.position
end

function StripBodyFromTrans(self)
    self.bodyPosition = self.transform.position
    self.renderPosition = self.transform.position
    self.renderForceMoveX = 0
end

function BreakNextBodyPosition(self)
    self:StripBodyFromTrans()
    self.isBreakBodyPosition = true
end

function SetForceMoveX(self, moveX)
	self.forceMoveX = moveX
end

function SetRenderForceMoveX(self, froceMoveX)
    self.renderForceMoveX = froceMoveX
end

function ClampPosition(self, x, onlyBoundary, syncRender)
	local opp = self:GetOpponent()
	if not opp then
		opp = self:GetPrimary()
	end

	local shape = opp:GetShape()
	if shape and not onlyBoundary then
		x = self:ClampBetween(shape, x, syncRender)
	end

	return x
end

function ClampBetween(self, shape, x, syncRender)
	local pos = Vector3.zero
	if syncRender then
		pos = shape:GetRenderPostion()
	else
		pos = shape:GetBodyPosition()
	end

	local dist1 = self.radius + self.radiusModifier + shape.radius + shape.radiusModifier
	local dist2 = dist1 + MAX_DISTANCE_BETWEEN_HERO
	if x < pos.x - dist2 then
		x = pos.x - dist2
	end

	if x > pos.x + dist2 then
		x = pos.x + dist2
	end

	return x
end

function GetMaster(self)
	local enemies = self.entity:GetEnemies()
	for idx, actor in ipairs(enemies) do
		if actor:IsHero() and actor:IsMaster() then
			return actor
		end
	end	
end

function GetEnemy(self)
	local enemies = self.entity:GetEnemies()
	for idx, actor in ipairs(enemies) do
		if actor:IsHero() and (actor:IsEnemy()  or actor:IsMaster()) then
			return actor
		end
	end	
end

function GetPrimary(self)
	local enemies = self.entity:GetEnemies()
	for idx, actor in ipairs(enemies) do
		if actor:IsHero() then
			return actor
		end
	end	
end

function GetOpponent(self)
	local enemies = self.entity:GetEnemies()
	for idx, actor in ipairs(enemies) do
		local chosedActor

		if not actor:IsDead() and actor:InFighting() then
			if not chosedActor then
				chosedActor = actor
			end

			if self:IsCloserBefore(actor, chosedActor) then
				chosedActor = actor
			end
		end
	end

	return chosedActor
end

function IsCloserBefore(self, newActor, oldActor)
	local bodyPos = self:GetBodyPosition()

	-- entity shape
	local newShape = newActor:GetShape()
	local oldShape = oldActor:GetShape()
	
	local oldDist = math.abs(bodyPos.x - oldShape:GetBodyPosition().x)
	local newDist = math.abs(bodyPos.x - newShape:GetBodyPosition().x)

	return newDist <= oldDist
end

function SetForceWalk(self, force)
	self.forceWalk = force
end

function IsForceWalk(self)
	return self.forceWalk
end

function FreezeAnimation(self, freezeFrame, shake, special, shakeDelta, isShakeY)
	if self.boxComp then
		if shakeDelta then
            self.boxComp:Freeze(freezeFrame, shake, special, shakeDelta, isShakeY)
        else
            self.boxComp:Freeze(freezeFrame, shake, special)
        end
	else
		if self.animator then
			self.animator.speed = 0
		end
	end
end

function UnfreezeAnimation(self)
	if self.boxComp then
		self.boxComp:Unfreeze()
	else
		if self.animator then
			self.animator.speed = 1
		end
	end
end

module("XHeroShape", package.seeall)
setmetatable(XHeroShape, {__index=XMoveableShape})

function new(entity, go)
	local shape = {
	}
	setmetatable(shape, {__index=XHeroShape})
	shape:Init(entity, go)

	return shape
end

function Init(self, entity, go)
	XMoveableShape.Init(self, entity, go)

	self.attackTypeToEffectIdx = {}
	self.attackWindow = false
	self.lightAttackCount = 0
	self.comboCount = 0
	self.enterIdleStartTime = 0
end

function Awake(self)
	XMoveableShape.Awake(self)

	self:InitStateMachine()
end

function Start(self)
	XMoveableShape.Start(self)

	-- fsm go
	self:SetState(Fsm.Idle)
end

function InitAnimator(self)
	XMoveableShape.InitAnimator(self)

	self:RegisterAnimationEventCallback(AnimationEventType.AttackStart, self.OnAnimationAttackStart)
	self:RegisterAnimationEventCallback(AnimationEventType.AttackEnd, self.OnAnimationAttackEnd)
end

function OnGroundChanged(self, nowGround)
	self.animator:SetBool(AnimatorStateValue.Ground, nowGround)

	if nowGround then
		if self:IsJump() or self:IsJumpAttack() then
			if self:IsCrouching() then
				self:CheckDirection(TurnaroundState.Crouch)
			elseif self.moveSpeed == 0 then
				self:CheckDirection(TurnaroundState.Stand)
			end

			self:CheckDirection()
		end
	end
end

function OnAnimationAttackStart(self, event)
	ELOG("OnAnimationAttackStart %s", event)
	if self.entity:IsDead() then
		return
	end

	local checkData = XUtility.ToHeroAttackCheckPoint(event)
	if checkData then
		local index = resmng.AttackSkillMapIndex[checkData.attackName] or 0
		if index > 0 then
			if checkData.checkType == XAttackCheckType.Normal then
				self:OnNormalAttack(checkData, skill, special)
			elseif checkData.checkType == XAttackCheckType.FlyProp then
				self:OnFlyPropAttack(checkData, skill, special)
			end
		else

		end
	end
end

function OnAnimationAttackEnd(self, event)
	ELOG("OnAnimationAttackEnd %s", event)
	if self.boxComp then
		self.boxComp:AttackEnd()
	end
end

function OnNormalAttack(self, checkData, skill, special)
	ELOG("OnNormalAttack %s", checkData.attackName)

	local damageEvents = {
	}

	self:IsTargetHit(self, checkData, function(receivers, hitPoses) 
		if receivers and hitPoses then
			self:DealNormalAttackEvents(receivers, hitPoses, checkData, skill, special, damageEvents)
		end
	end)
end

function OnFlyPropAttack(self, checkData, skill, special)
	ELOG("OnFlyPropAttack")
	local effect = checkData.attackSfx
	if effect then
		local effectRes = effect.effectFiles

		local number = self.entity:Int(1, #effectRes)
		if number > 0 then
			self.attackTypeToEffectIdx[checkData.attackName] = number
			self:DealFlyPropAttack(checkData, 
				skill, special, effectRes[number])
		end
	end
end

function DealFlyPropAttack(self, checkData, skill, special, effectPath)
	local sfxProperty = checkData.attackSfxProp
	if not sfxProperty then
		return false
	end

	local enemyTransAry = {}
	local enemyTransMap = {}

	local enemies = self.entity:GetEnemies()
	for idx, actor in ipairs(enemies) do
		if actor and not actor:IsDead() and actor:IsReceiveAttack() then
			local shape = actor:GetShape()
			if shape then
				local flag = true
				if Slua.IsNull(shape.transform) then
					flag = false				
				end

				if flag then
					table.insert(enemyTransAry, 
						shape.transform)

					enemyTransMap[shape.transform] = shape
				end
			end
		end
	end

	local releaseTrans = self.boxComp:GetAttackCheckPointTransform(checkData)
	if releaseTrans then
		local releasePos = Vector3(releaseTrans.position.x, releaseTrans.position.y, 0)
		
		local fixedForward = Vector3(self.forward.x, self.forward.y, self.forward.z)
		if fixedForward.x > 0 then
			fixedForward.x = 1
		else
			fixedForward.x = -1
		end

		local destPos = gfPrecisionFixedVector3(releasePos + fixedForward * (sfxProperty.speed * sfxProperty.time))
		
		-- hit
		local callback = function(bullet, target)
			if target then
				local shape = enemyTransMap[target]
				if shape then
					self:DealBulletHitDamage(checkData, shape, bullet, sfxProperty, enemyTransMap, target, skill, 1, true)
				end
			end

			return true
		end

		local loadDone = function(bullet)
		end


		XEffectManager.GenerateBulletAsync(true, effectPath, releasePos, destPos, sfxProperty,
    		callback, enemyTransAry, loadDone)
	end

	return true
end

function DealNormalAttackEvents(self, receivers, hitPoses, checkData, skill, special, damageEvents)
	ELOG("DealNormalAttackEvents")

	for i=1, #receivers do
		local actor = self.fight:GetActor(receivers[i])
		if actor then
			local shape = actor:GetShape()
			if shape then
				local event = XDamageEvent.CreateAttackFrameEvent(self.entity:cid(), actor:cid(), skill, checkData, 
					self.attackTypeToEffectIdx[checkData.attackName] or 1, hitPoses[i])
				table.insert(damageEvents, event)
			end
		end
	end

	if #damageEvents > 0 then
		self:DealAttackFrameEvents(damageEvents, checkData, special)
	end
end

function DealAttackFrameEvents(self, events, checkData, special)
	ELOG("DealAttackFrameEvents")

	local blockedFreeze = false
	local hitFreeze = 0
	local hitReact = checkData.hitReact
	local atkFreeze = 0

	for idx, event in ipairs(events) do
		local shape = self.fight:GetShape(event.revID)
		if shape then
			shape:Damage(events[idx])

			hitFreeze = hitReact.freeze

			local curAtkFreeze = hitReact.freezeSelf
			if curAtkFreeze > atkFreeze then
				atkFreeze = curAtkFreeze
			end

			if hitFreeze > 0 then
				shape:FreezeAnimation(hitFreeze, not blockedFreeze, false, hitReact.shakeDelta, true)
			end
		end
	end

	if atkFreeze > 0 then
		self:FreezeAnimation(atkFreeze, blockedFreeze, false)
	end

	local shake = hitReact.cameraShake
	if shake > 0 then
		XCameraFight.GetSingleton():PlayShake(2)
	end
end

function DealBulletHitDamage(self, checkData, ctrl, bullet, property, enemyTransMap, target, skill, randNum, isFlyProp)
	local hits = { ctrl }
	enemyTransMap[target] = nil

	for trans, enemy in pairs(enemyTransMap) do
		if enemy ~= ctrl then
			if not Slua.IsNull(trans) then
				local range = math.abs(trans.position.x - target.position.x)
				if range <= checkData.range then
					table.isnert(hits, enemy)
					enemyTransMap[trans] = nil
				end
			end
		end
	end

	local dmgCount = 0

	XFightTimer.Add(0, property.damageInterval * 1000, function() 
		local damageEvents = {
		}

		for trans, enemy in pairs(hits) do
			if enemy:GetEntity():IsReceiveAttack() then
				local event = XDamageEvent.CreateAttackFrameEvent(self.entity:cid(),ctrl:GetEntity():cid(), skill, checkData, randNum)
				if event then
					event.hitPoint = nil
					event.hitPointRot = nil
					event.current = checkData.dmgCurrent + dmgCount
					event.bullet = bullet
 					--event.hitReaction = resmng.HITANI_DOWN

 					table.insert(damageEvents, event)
				end
			end
		end

		self:DealAttackFrameEvents(damageEvents, checkData, special)		
		return false
	end)
end

function GetHitReactAnimationName(self, hitReaction)
	if self:IsGround() then
		return resmng.HeroHitReactAnimation[hitReaction]
	end

	return resmng.HeroAirHitReactAnimation[hitReaction]
end

function PlayDamageEffect(self, attacker, event)
	if not attacker or not event then
		return false
	end

	local hitAniName = self:GetHitReactAnimationName(event.hitReaction)
	if hitAniName then
		self.animator:Play(hitAniName, 0, 0)
	end

    local hitTrans
    if event.hitPoint then
        local rot = Quaternion.Euler(Vector3(0, 0, 90 * (self.entity:GetFlip() and 0 or 2)))
        hitTrans = {
        	position = event.hitPoint, 
        	rotation = rot
        }
    end

	if event.hitEffects  then
		for i=1, #event.hitEffects do
			local effect = event.hitEffects[i]
			if effect then
				local index = event.effectIndex or 0
				if index > 0 then
					self:PlayEffectConfig(effect, 0, attacker.transform, hitTrans, index)
				end
			end
		end
	end

	return true
end

function Damage(self, event)
	local attacker = self.fight:GetShape(event.atkID)
	if not attacker then
		return false
	end

	self:PlayDamageEffect(attacker, event)
	
	return true
end

Fsm = {
	Idle = "StateIdle",
	Jump = "StateJump",
	JumpAttack = "StateJumpAttack",
	Block = "StateBlock",
	Skill = "StateSkill",
	Attack = "StateAttack",
	Run = "StateRun",
	Turnaround = "StateTurnaround",
}

FsmTagHashMap = {
	[StateTagHashCode.Idle] = Fsm.Idle,
	[StateTagHashCode.Jump] = Fsm.Jump,
	[StateTagHashCode.Turnaround] = Fsm.Turnaround,
	[StateTagHashCode.Skill] = Fsm.Skill,
	[StateTagHashCode.Attack] = Fsm.Attack,
	[StateTagHashCode.Block] = Fsm.Block,
	[StateTagHashCode.Run] = Fsm.Run,
}

function OnAnimatorStateMachineEnter(self, curHash, curTagHash, playTime)
	local now = FsmTagHashMap[curTagHash]
	if now then
		self:SetState(now)
	end
end

function OnAnimatorStateMachineExit(self, curHash, curTagHash, stateInfo)

end

function GetTime(self)
	return Time.realtimeSinceStartup
end

function InitStateMachine(self)
	self:AddState(Fsm.Idle, self.OnStateIdleEnter, self.OnStateIdleUpdate, self.OnStateIdleExit)
	self:AddState(Fsm.Jump, self.OnStateJumpEnter, self.OnStateJumpUpdate, self.OnStateJumpExit)
	self:AddState(Fsm.Turnaround, self.OnStateTurnaroundEnter, self.OnStateTurnaroundUpdate, self.OnStateTurnaroundExit)
	self:AddState(Fsm.Attack, self.OnStateAttackEnter, self.OnStateAttackUpdate, self.OnStateAttackExit)
end

-------------------------------------------------------
function OnStateIdleEnter(self)
	self:SetAttackWindow(true)
	self.enterIdleStartTime = self:GetTime()

	self.fliping = false
	self:ForceSyncBodyBox()
end

function OnStateIdleUpdate(self)
	if self:IsCrouching() then
		self:CheckDirection(TurnaroundState.Crouch)
	elseif self.moveSpeed == 0 then
		self:CheckDirection(TurnaroundState.Stand)
	else
		self:CheckDirection()
	end
end

function OnStateIdleExit(self)
end

-------------------------------------------------------
function OnStateJumpEnter(self)
end

function OnStateJumpUpdate(self)
end

function OnStateJumpExit(self)
end

-------------------------------------------------------
function OnStateTurnaroundEnter(self)
	self:CheckDirection()
end

function OnStateTurnaroundUpdate(self)
end

function OnStateTurnaroundExit(self)
end

-------------------------------------------------------
function OnStateAttackEnter(self)
	ELOG("OnStateAttackEnter")
end

function OnStateAttackUpdate(self)
end

function OnStateAttackExit(self)
end

-------------------------------------------------------
function IsAir(self)
	return self.Ground
end

function IsJump(self)
	return self:IsCurrent(Fsm.Jump)
end

function IsJumpAttack(self)
	return self:IsCurrent(Fsm.JumpAttack)
end

function IsCrouching(self)
    return self:IsCurrentStateOnLayer(StateHashCode.Crouch)
end

function IsCrouchBlocking(self)

end

function Enter(self)
	XMoveableShape.Enter(self)

	if self.entity:IsMaster() then
		self.input = self.entity:GetComponent(XPlayerInput)
		if not self.input then
			self.input = self.entity:AddComponent(XPlayerInput)
		end

		self.input:Reset()
		self.input:Paused(false)
	end
end

function Leave(self)
	XMoveableShape.Leave(self)

	if self.input then
		self.input:Reset()
		self.input:Paused(true)
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

function SetAttackWindow(self, flag)
	self.attackWindow = flag
end

function GetAttackWindow(self)
	return self.attackWindow
end

function SetLightAttackCount(self, count)
	self.lightAttackCount = count
end

function GetLightAttackCount(self)
	return self.lightAttackCount
end

function ResetAnimatorParam(self)
	self.animator:SetInteger(AnimatorStateValue.MoveSpeed, 0)
	self.animator:SetBool(AnimatorStateValue.Crouch, false)
	self.animator:SetBool(AnimatorStateValue.Jump, false)
	self.animator:SetBool(AnimatorStateValue.Ground, false)
	self.animator:SetBool(AnimatorStateValue.Dead, false)
	self.animator:SetBool(AnimatorStateValue.Block, false)
	self.animator:SetBool(AnimatorStateValue.Stunned, false)
	self.animator:SetInteger(AnimatorStateValue.JumpType, 0)
end

function ResetAttackChain(self)
	self:SetLightAttackCount(0)
end

function CheckResetAttackChain(self)
	local reset = false

	local lightAttackCount = self:GetLightAttackCount()
	if lightAttackCount >= resmng.MaxLightCount then
		reset = true
	else
		if self.enterIdleStartTime > 0 then
			local time = self:GetTime() - self.enterIdleStartTime
			if time > 0.2 then			
				reset = true
			end
		end
	end

	if reset then
		self:ResetAttackChain()
	end

	self.enterIdleStartTime = 0
end

function CanAttack(self, level)
	if not self:GetAttackWindow() then
		return false, "attackWindow"
	end

	return true
end

function DoLightAttack(self, level)
	self:SetAttackWindow(false)

	local count = self:GetLightAttackCount() + 1
	local name = resmng.LightAttackName[count]
	if name then
		self.animator:Play(name, 0 , 0)
	end

	self:SetLightAttackCount(count)

	return true
end

function DoDefaultAttack(self, level)
	local ani = resmng.AttackLevelAniMap[level]
	if not ani then
		return false
	end

	self:SetAttackWindow(false)

	if self.animator then
		self.animator:Play(ani, 0, 0)
	end
end

function DoAttack(self, level, noReset)
	if not noReset then
		self:CheckResetAttackChain()
	end

	local result = false
	if level == resmng.AttackLevel.Light then
		result = self:DoLightAttack(level)
	else
		result = self:DoDefaultAttack(level)
	end

	return result
end


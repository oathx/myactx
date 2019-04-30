module("XSimpleShape", package.seeall)
setmetatable(XSimpleShape, {__index=XEntityComponent})

StateHashCode 		= resmng.StateHashCode
StateTagHashCode 	= resmng.StateTagHashCode

function new(entity, go)
	local shape = {
	}
	setmetatable(shape, {__index=XSimpleShape})
	shape:Init(entity, go)

	return shape
end

function Init(self, entity, go)
	XEntityComponent.Init(self, entity)

	self.flip = self.entity:GetFlip()
	self.curStateHash = 0
	self.stateTagHash = 0
	self.playTime = 0
	self.boneNogs = {}
	self.visible = true
	
	self:AddBehaviour(go)
end

function Awake(self)
	-- initliaze shape runtime component
	self:InitComponent()
	self:InitAnimator()
end

function InitComponent(self)
	if not self.gameObject then
		return false
	end

	self.gameObject.name = tostring(self.entity:cid())

	self.luaComp = self.gameObject:GetComponent(XLuaBehaviourScript)
	self.boxComp = self.gameObject:GetComponent(XBoxComponent)	
	if self.boxComp then
		self.boxComp.cid = self.entity:cid()
	end

	return true
end

function InitAnimator(self)
	self.animator = self.gameObject:GetComponent(XSmoothAnimator)
	if self.animator then
		self:EnableAnimator(false)
	else
		self.animator = self.gameObject:GetComponent(Animator)
		self:EnableAnimator(true)
	end
end

function UpdateAnimator(self, deltaTime)
	if self.animator and not self.animator.enabled and self.isOnStage then
		self.animator:Update(deltaTime)
	end
end

function EnableAnimator(self, enable)
    if self.animator then
        self.animator.enabled = enable
    end
end

function RegisterAnimationEventCallback(self, name, callback)
	self.dealAnimationEventFuncs = self.dealAnimationEventFuncs or {}

	local func  = self.dealAnimationEventFuncs[name]
	if not func then
		self.dealAnimationEventFuncs[name] = callback
	end
end

function IsActive(self)
    if self.gameObject and not Slua.IsNull(self.gameObject) then
        return self.gameObject.activeSelf
    end

    return false
end

function IsTargetHit(self, target, checkData, callback)
	if self.boxComp then
		return self.boxComp:IsHitEventHits(checkData, target.gameObject, callback)
	end

	return false
end

function GetBox(self)
	if not self.boxComp then
		self.boxComp = self.gameObject:GetComponent(XBoxComponent)
	end

	return self.boxComp
end

function DisableBodyBox(self)
	if self.boxComp then
    	self.boxComp:DisableBoxes(1)
    end
end

function EnableBodyBox(self)
	if self.boxComp then
    	self.boxComp:EnableBoxes(1)
    end
end

function DisableHurtBox(self)
	if self.boxComp then
    	self.boxComp:DisableBoxes(2)
    end
end

function EnableHurtBox(self)
	if self.boxComp then
    	self.boxComp:EnableBoxes(2)
    end
end

function ResetShapeTransform(self, ifzero)
	if not self.shape then
		self.shape = self.transform:Find("shape")
	end

	local flip = self.entity:GetFlip()
   	if self.shape then
	    if ifzero then
	        self.shape.localRotation = Quaternion.identity
	        self.shape.localScale = Vector3.one
	    else
	        if not flip then
	            self.shape.localRotation = Quaternion.Euler(Vector3(0,90,0))
	            self.shape.localScale = Vector3.one
	        else
	            self.shape.localRotation = Quaternion.Euler(Vector3(0,270,0))
	            self.shape.localScale = Vector3(-1,1,1)
	        end
	    end
	    
	    self.forward = self.shape.forward
	end
end

function Flipped(self, flip)
	local curFlip = self.entity:GetFlip()
	if curFlip ~= flip then
		self.entity:SetFlip(flip)
	end

	self:ResetShapeTransform()
end

function SetPosition(self, pos)
	if self.transform then
		self.transform.position = pos
	end
end

function GetPosition(self)
	if self.transform then
		return self.transform.position
	end
end

function Enter(self)
	self:EnableBodyBox()
	self:EnableHurtBox()
end

function Leave(self)
	self:DisableBodyBox()
	self:DisableHurtBox()
end

function DrawGL(self)
end

function FixedFrameUpdate(self, deltaTime)
	XEntityComponent.FixedFrameUpdate(self, deltaTime)

	if self.boxComp then
		self.boxComp:FixedFrameUpdate(deltaTime)
	end
end

function LateFrameUpdate(self, deltaTime)
	if self.boxComp then
		self.boxComp:LateFrameUpdate(deltaTime)
	end
end

function IsCurrentStateOnLayer(self, name)
    return self.curStateHash == name
end

function UpdateStateMachine(self)
	if self.animator then
		local preHash = self.curStateHash
		local preTagHash = self.stateTagHash
		local playTime = self.playTime

		local stateInfo
		if self.animator:IsInTransition(0) then
			stateInfo = self.animator:GetNextAnimatorStateInfo(0)
		else
			stateInfo = self.animator:GetCurrentAnimatorStateInfo(0)
		end

		self.curStateHash = stateInfo.shortNameHash
        self.stateTagHash = stateInfo.tagHash

        local norTime = stateInfo.normalizedTime
        self.playTime = norTime - math.floor(norTime)

        if preHash ~= self.curStateHash then
            self.preStateTagHash = preTagHash

            self:OnAnimatorStateMachineExit(preHash, preTagHash, playTime)
            self:OnAnimatorStateMachineEnter(self.curStateHash, 
            	self.stateTagHash, stateInfo)
        end
	end
end

function PlayEffect(self, effect, bind, time, follow, func)
    time = time or 0
    local parent = self:GetBoneNogTransform(bind) or self.transform
    if not parent then
        return false
    end
  
    local loadDone = function( xeffect )
        if xeffect and func then
            if follow == 2 or follow == 3 then
                xeffect.onlyFollowPosition = true

                if follow == 3 then
                    xeffect.ignoreFollowHeight = true
                    xeffect.followStayHeight = parent.position.y
                end
            end

            func(xeffect, parent)
        end
    end
    
    local xeffect
    if follow == 1 then
        XEffectManager.GenerateAsync(parent.gameObject, effect, time, loadDone);
    else
        XEffectManager.GenerateAsync(parent.gameObject, parent.position, Quaternion.identity, effect, time, loadDone);
    end

    return true
end

function GetBoneNogTransform(self, idx)
    local nog = resmng.BoneNog[idx]
    if not nog or not self.boneNogs or not self.boxComp then
        return
    end

    local trans = self.boneNogs[nog]
    if not trans then
        trans = self.boxComp.NogPoint[nog]
        self.boneNogs[nog] = trans
    end

    return trans
end

function PlayEffectConfig(self, effectConfig, time, attacker, hitTrans, effectIdx)
    if not self.visible or not effectConfig then 
    	return false 
    end

    local linkBone = effectConfig.linkBone
    local follow = effectConfig.follow
    local idx = 0
    if effectIdx then
        idx = effectIdx - 1
        if effectIdx > #effectConfig.effectFiles then
            idx = 0
        end
    end

    self:PlayEffect(effectConfig:getRes(idx), effectConfig.linkBone, time, follow,
    	function(xeffect, parent)
	        local owner = attacker or parent
	        if not (follow==1) and owner.lossyScale.x < 0 then
	            xeffect.transform:Rotate(Vector3(0,180,0))
	        end

	        if hitTrans and linkBone == resmng.BONE_NOG_HIT then
	            xeffect.transform.position = hitTrans.position
	            xeffect.transform.rotation = hitTrans.rotation
	        end
    	end)
end

function OnAnimatorStateMachineEnter(self, curHash, curTagHash, playTime)
end

function OnAnimatorStateMachineExit(self, curHash, curTagHash, stateInfo)
end

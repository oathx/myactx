module("XDamageEvent", mkcall)

mt = {
    __index = XDamageEvent,
}

local resmng = resmng
function new()
    local event = {}
    setmetatable(event, mt)
    event:Init()

    return event
end

function Init(self)
	self.atkID = 0
	self.revId = 0
	self.skill = nil
	self.value = 0
	self.total = 1
	self.current = 1
	self.rate = 0
	
	self.dmgBy = resmng.DMG_BY_UNKNOWN
	self.flags = resmng.DamageFlags.None
	self.hitReaction = resmng.HITANI_SOFT
	self.attackName = ""
	self.fixedDamage = 0
	self.effectIndex = 1
	self.priority = 4
end

function CreateAttackFrameEvent(atkID, revID, skill, checkData, effectIndex, hitPos)
    local event = XDamageEvent()
    event.atkID = atkID
    event.revID = revID
    event.skill = skill

    event.checkData = checkData
    event.hitReaction = checkData.hitReact.hitReactAni

	if checkData.dmgTotal > 0 then
        event.total = checkData.dmgTotal
        event.current = checkData.dmgCurrent
        event.rate = checkData.dmgRate/100
    end

    event.hitEffects = checkData.hitReact.hitEffects
 	event.attackName = checkData.attackName
 	event.attackLevel = resmng.AttackLevelMap[checkData.attackName]

 	event.hitPointRot = checkData.hitReact.hitPointRot
     if hitPos then
        event.hitPoint = hitPos
    else
        event.hitPoint = checkData.hitReact.hitPoint
    end

    event.effectIndex = effectIndex
    event.canDamage = checkData.hitReact.canSendDamage
    event.breakArmor = false

    return event
end

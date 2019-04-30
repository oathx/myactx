module("ActorEntity", package.seeall)
setmetatable(ActorEntity, {__index=XEntity})

function new(cid, propID)
	local entity = {
	}
	setmetatable(entity, {__index=ActorEntity})
	entity:Init(cid, propID)

	return entity
end

function Init(self, cid, propID)
	XEntity.Init(self, cid)

	self.type 			= resmng.XActorType.None
	self.propID 		= propID
	self.flip 			= false
	self.autoAnimator 	= false
	self.friends 		= {}
	self.enemies 		= {}
	self.hp 			= 1
	self.inFighting		= false
	self.status 		= resmng.FightStatus.BS_NONE
	self.invulnerable	= false
end

function GetProp(self)
end

function CampType(self)
	if self.type < resmng.XActorType.Enemy then
		return resmng.XActorTypeCamp.Friend
	elseif self.type >= resmng.XActorType.Enemy then 
		return resmng.XActorTypeCamp.Enemy
	end

	return resmng.XActorTypeCamp.Neutral
end

function SetFlip(self, flip)
	self.flip = flip
end

function GetFlip(self)
	return self.flip
end

function GetType(self)
	return self.type
end

function IsHero(self)
    return bit.band(self.type, resmng.XActorType.Hero) > 0
end

function IsMaster(self)
	return bit.band(self.type, resmng.XActorType.Master) > 0
end

function IsPlayer(self)
	return bit.band(self.type, resmng.XActorType.Player) > 0
end

function IsEnemy(self)
	return bit.band(self.type, resmng.XActorType.Enemy) > 0
end

function IsMonster(self)
	return bit.band(self.type, resmng.XActorType.Monster) > 0
end

function IsType(self, actortype)
	return bit.band(self.type, actortype) > 0
end

function GetFriends(self)
	return self.friends
end

function GetEnemies(self)
	return self.enemies
end

function InFighting(self)
	return self.inFighting
end

function IsDead(self)
	return self.hp <= 0
end

function IsReceiveAttack(self)
	if not self:InFighting() or self.invulnerable then
		return false
	end

	return true
end

function Enter(self)
	self.inFighting = true
end

function Leave(self)
	self.inFighting = false
end

function AddStatus(self, status, time, data)
end

function IsStatus(self, status)
	return bit.band(self.status, status) > 0
end

function RemoveStatus(self, status)
end

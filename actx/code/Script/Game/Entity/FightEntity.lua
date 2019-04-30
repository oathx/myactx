module("FightEntity", package.seeall)
setmetatable(FightEntity, {__index=XEntity})

function new(id)
	local entity = {
	}
	setmetatable(entity, {__index=FightEntity})
	entity:Init(id)

	return entity
end

function Init(self, id)
	XEntity.Init(self, id)
	self.actors = {}
	
	-- hero list
	self.heros = {}
	setmetatable(self.heros, 
	{
		__mode="v"
	})

	self.friends = {}
	self.enemies = {}
	self.serverDriveTime = 0
	self.clientDriveTime = 0
	self.renderWantsTime = 0
end

function IsPvp(self)
	return true
end

function SetTask(self, task)
	self.task = task
end

function GetTask(self)
	return self.task
end

function AddRelationship(self, actor)
	if actor:CampType() == resmng.XActorTypeCamp.Friend then
		local added = true
		for idx, v in ipairs(self.friends) do
			if v:cid() == actor:cid() then
				added = false
				break
			end
		end

		if added then
			table.insert(self.friends, actor:cid())
		end
	else
		local added = true
		for idx, v in ipairs(self.enemies) do
			if v:cid() == actor:cid() then
				added = false
				break
			end
		end

		if added then
			table.insert(self.enemies, actor:cid())
		end
	end
end

function DelRelationship(self, actor)
	if actor:CampType() == resmng.XActorTypeCamp.Friend then
		for idx, v in ipairs(self.friends) do
			if v:cid() == actor:cid() then
				table.remove(self.friends, idx)
				break
			end
		end
	else
		for idx, v in ipairs(self.enemies) do
			if v:cid() == actor:cid() then
				table.remove(self.enemies, idx)
				break
			end
		end
	end
end

function CreateActor(self, actorType, nID, propID)
	local actor = self.actors[nID]
	if not actor then
		actor = ActorFactory.Create(actorType, nID, propID)
		if actor then
			self.actors[nID] = actor
		end
	end

	if actor:IsHero() then
		local added = true
		for id, actor in ipairs(self.heros) do
			if actor:cid() == nID then
				added = false
				break
			end
		end

		if added then
			table.insert(self.heros, actor)
			
			-- must be sort hero list in sync fight
			table.sort(self.heros, SortHeroCompare)
		end
	end

	self:AddRelationship(actor)

	return actor
end

function GetActor(self, nID)
	return self.actors[nID]
end

function DestroyActor(self, nID, notDispose)
	local actor = self.actors[nID]
	if not actor then
		return false
	end

	if actor:IsHero() then
		for idx, v in ipairs(self.heros) do
			if v:cid() == nID then
				table.remove(self.heros, idx)
				break
			end
		end

		table.sort(self.heros, SortHeroCompare)
	end

	self:DelRelationship(actor)

	local dispose = notDispose or true
	if dispose then
		actor:Destroy()
	end
end

function SortHeroCompare(a, b)
	return a:cid() < b:cid()
end

function GetActorByType(self, actorType)
	for cid, actor in pairs(self.actors) do
		if actor:IsType(actorType) then
			return actor
		end
	end
end

function GetMaster(self)
	return self:GetActorByType(resmng.XActorType.Master)
end

function GetEnemy(self)
	return self:GetActorByType(resmng.XActorType.Enemy)
end

function GetAllActor(self)
	return self.actors
end

function GetHeroList(self)
	return self.heros
end

function GetShape(self, id)
	local actor = self:GetActor(id)
	if actor then
		return actor:GetShape()
	end
end

function UpdateOpponentList(self)
 	local friends = self.friends
    local enemies = self.enemies

    local dealActor
    local dealIndex = 1

    for i=1, #friends do
        dealActor = self:GetActor(friends[i])
        if dealActor then
            dealActor.friends = {}
            dealActor.enemies = {}

            dealIndex =  1
            while dealIndex <= #friends do
                if i ~= dealIndex then
                    table.insert(dealActor.friends, self:GetActor(friends[dealIndex]))
                end

                dealIndex = dealIndex + 1
            end

            if #enemies > 0 then
                for j, v in ipairs(enemies) do
                    table.insert(dealActor.enemies, self:GetActor(v))
                end
            end
        end
    end

    if #enemies > 0 then
        for k = 1, #enemies do
            dealActor = self:GetActor(enemies[k])
            if dealActor then
                dealActor.friends = {}
                dealActor.enemies = {}

                dealIndex = 1
                while dealIndex <= #enemies do
                    if k ~= dealIndex then
                        table.insert(dealActor.friends, self:GetActor(enemies[dealIndex]))
                    end

                    dealIndex = dealIndex + 1
                end

                for j, v in ipairs(friends) do
                    table.insert(dealActor.enemies, self:GetActor(v))
                end
            end
        end
    end

    return true
end

function Enter(self, actor)
	if actor then
		actor:Enter()
		self:UpdateOpponentList()
	end
end

function Leave(self, actor)
	if actor then
		actor:Leave()
		self:UpdateOpponentList()
	end
end


module("XFightManager", mkSingleton)

function Init(self)
	self.fights = {}
end

-- create fight
function CreateFight(self, task, fightID, component)
	local current = FightEntity.new(fightID)
	current:SetTask(task)

	if component then
		current:AddComponent(component)
	end

	table.insert(self.fights, current)

	return current
end

function GetCurrentFight(self)
	return self.fights[#self.fights]
end

function DestroyFight(self, fightID)
	for idx, fight in ipairs(self.fights) do
		if fight:cid() == fightID then
			fight:Destroy()
			table.remove(self.fights, idx)
			break
		end
	end
end

function Clearup(self)
	for idx, fight in ipairs(self.fights) do
		fight:Destroy()
	end

	self.fights = {}
end
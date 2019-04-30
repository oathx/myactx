module("FightSystem", package.seeall)
setmetatable(FightSystem, {__index=XEventBehaviour})

function new(szName)
	local home = {
	}
	setmetatable(home, {__index=FightSystem})
	home:Init(szName)

	return home
end

function Init(self, szName)
	XEventBehaviour.Init(self, szName)

	self.task 	= self:GetTask()
	self.fights = {}
end

function GetTask(self)
	local sysEntity = XEntityManager.GetSingleton():GetEntity(resmng.SYS_TASK)
	if sysEntity then
		return sysEntity:GetCurrent()
	end
end

function OnActive(self)
	if self.task then
		XFightManager.GetSingleton():CreateFight(self.task, fightID, XFightLogic)
	end
end

function OnDetive(self)
	XFightManager.GetSingleton():Clearup()
end



module("XFightLogic", package.seeall)
setmetatable(XFightLogic, {__index=XEntityComponent})

FightState = {
	Load = "StateLoad",
	Ready = "StateReady",
	Fighting = "StateFighting",
}

function new(entity)
	local comp = {
	}
	setmetatable(comp, {__index=XFightLogic})
	comp:Init(entity, task)

	return comp
end

function Init(self, entity)
	XEntityComponent.Init(self, entity)

	self.sceneCompleted = false
	self.herosCompleted = false
	self.fightCompleted = false
	self.fixedFrameCount = 0

    -- must be add MonoBehaviour
    self:AddBehaviour(GameObject(tostring(entity:cid())))
    if self.gameObject then
    	self.gameObject.tag = "FightLogic"
        GameObject.DontDestroyOnLoad(self.gameObject)
    end
end

function InitStateMachine(self)
	self:AddState(FightState.Load, 
		self.OnFightLoadEnter, self.OnFightLoadUpdate, self.OnFightLoadExit)

	self:AddState(FightState.Ready,
		self.OnFightReadyEnter, self.OnFightReadyUpdate, self.OnFightReadyExit)

	self:AddState(FightState.Fighting,
		self.OnFightingEnter, self.OnFightingUpdate, self.OnFightingExit)

	return true
end

function Awake(self)
	self:InitStateMachine()

	local borns = {
		{ type = resmng.XActorType.Master, pos = Vector3(-1.5, 0, 0), propID=resmng.CHAR_GRIL, cid=1},
		{ type = resmng.XActorType.Enemy, pos = Vector3(1.5, 0, 0), propID=resmng.CHAR_GRIL, cid=2}
	}

	for idx, born in ipairs(borns) do
		local actor = self.entity:CreateActor(born.type, born.cid, born.propID)
		if actor then
			actor:SetBorn(born.pos)
		end
	end
end

function Start(self)
	self:SetState(FightState.Load)
end

function FixedUpdate(self)

end

function Update(self)
	XEntityComponent.FixedFrameUpdate(self, Time.deltaTime)

	if not self:IsCurrent(FightState.Load) then
		self:StandaloneUpdate(Time.deltaTime)
	end
end

function StandaloneUpdate(self, deltaTime)
	XFightTimer.Tick(deltaTime)

	local intDeltaTime = math.floor(deltaTime * 10000)
	if intDeltaTime < 1000 then
		self:DoUpdate(deltaTime)
	else
		local step = intDeltaTime / 666
		local sum = 0
		for i=1, step do
			if i == step then
				local last = intDeltaTime - sum
				if last < 1000 then
					self:DoUpdate(last / 10000)
				else
					self:DoUpdate(0.0666)
					self:DoUpdate((last - 666) / 10000)
				end
			else
				self:DoUpdate(0.0666)
			end

			sum = sum + 666
		end
	end
end

function SyncUpdate(self, deltaTime)
	self:FixedFrameUpdate(deltaTime)

	local heros = self.entity:GetHeroList()
	for idx, hero in ipairs(heros) do
		local shape = hero:GetShape()
		if shape  then
			shape:AdjustAnimatorState(deltaTime)
		end
	end
end

function FixedFrameUpdate(self, dt)
	local deltaTime = dt or Time.deltaTime
	local timeScale = Time.timeScale

	local intDeltaTime = gfRoundInt(deltaTime * 10000)
	if Time.timeScale > 0 then
		if intDeltaTime <= 0 then
			XFightTimer.Tick(deltaTime)
		end

		while intDeltaTime > 0 do
			local intElapsed = math.min(330, intDeltaTime)
			
			-- update animation event, main logic update
			local dt = intElapsed * 0.0001
			self:DoUpdate(dt)
			intDeltaTime = intDeltaTime - intElapsed

			-- update fight timer
			XFightTimer.Tick(dt)
		end
	else
		self.entity.clientDriveTime = self.clientDriveTime + intDeltaTime * 0.0001
		XFightTimer.Tick(deltaTime)
	end
end

function DoUpdate(self, deltaTime)
	self.entity.clientDriveTime = self.entity.clientDriveTime + deltaTime

	local heros = self.entity:GetHeroList()
	for idx, hero in ipairs(heros) do
		local shape = hero:GetShape()
		if shape  then
			shape:FixedFrameUpdate(deltaTime)
		end
	end

	XEffectManager.FixedUpdate(deltaTime)
	
	for idx, hero in ipairs(heros) do
		local shape = hero:GetShape()
		if shape  then
			shape:FixedInputUpdate(deltaTime)
		end
	end

	for idx, hero in ipairs(heros) do
		local shape = hero:GetShape()
		if shape  then
			shape:LateFrameUpdate(deltaTime)
		end
	end

	XBoxSystem.GetSingleton():Update()

	if _DEBUG then
		for idx, hero in ipairs(heros) do
			local shape = hero:GetShape()
			if shape and shape:IsActive() then
				shape:DrawGL()
			end
		end
	end
end

function DoRender(self, deltaTime)
	local renderDeltaTime = math.min(deltaTime, self.entity.serverDriveTime - self.entity.clientDriveTime)
	local isWantsForecast = renderDeltaTime <= 0
	if isWantsForecast then
		isWantsForecast = math.floor(deltaTime * 1000) * 0.001
	else
		self.entity.renderDeltaTime = self.entity.renderDeltaTime + renderDeltaTime
		self.entity.renderWantsTime = 0
	end

	if isWantsForecast then
		self.entity.renderWantsTime = self.entity.renderWantsTime + math.floor(renderDeltaTime * 1000)
	end

	local dt = math.max(0, math.min(deltaTime, 
		(resmng.PVP_WANT_TIME - self.entity.renderWantsTime) * 0.001))

	local heros = self.entity:GetHeroList()
	for idx, hero in ipairs(heros) do
		local shape = hero:GetShape()
		if shape  then
			shape:FixedFrameRender(dt)
		end
	end

	XBoxSystem.GetSingleton():UpdateOnlyTransPosition()
end

function LoadActorAsync(self, allActor)
	local asyncConf = {
	}

	local actors = {}
	for idx, actor in pairs(allActor) do
		local conf = ScriptableObject.CreateInstance(XAvaterConfigure)
		if conf then
			conf.skeleton = actor:GetRenderSkeleton()
			conf.elements = actor:GetElements()

			table.insert(asyncConf, conf)
			table.insert(actors, actor)
		end
	end

	XAvatarSystem.LoadMultiAsync(asyncConf, function(objs) 
		self.herosCompleted = true

		for idx, obj in pairs(objs.Table) do
			local actor = actors[idx]
			if actor then
				local go = GameObject.Instantiate(obj)
				if go then
					local shape = actor:CreateShape(go)
					shape:SetGroundHeight(actor:GetBorn().y)
				end
			end
		end

		XAvatarSystem.ResetCacheAvatar()
	end)
end

function LoadFightUIAsync(self)
	UISystem.GetSingleton():OpenWidget(UIStyle.FIGHT, function(widget)
		self.widget = widget
		self.fightCompleted = true
	end)
end

function GetShape(self, actorType)
	local actor = self.entity:GetActorByType(actorType)
	if actor then
		return actor:GetShape()
	end
end

function GetMaster(self)
	local shape = self:GetShape(resmng.XActorType.Master)
	if shape then
		return shape.gameObject
	end
end

function GetEnemy(self)
	local shape = self:GetShape(resmng.XActorType.Enemy)
	if shape then
		return shape.gameObject
	end
end

function ResetFightCamera(self)
	local cameraFight = Camera.main.gameObject:AddComponent(XCameraFight)
	if cameraFight then
		local source = self:GetShape(resmng.XActorType.Master)
		local target = self:GetShape(resmng.XActorType.Enemy)

		if source and target then
			cameraFight:Configure(XCameraType.PVP, -100, 100, 
				source.gameObject, target.gameObject)

			self.widget:SetMaster(source:GetEntity())
		end
	end
end

function OnFightLoadEnter(self)
	local task = self.entity:GetTask()
	if task then
		local func = function(widget)
			local sceneID = task:GetSceneID()
			if sceneID <= 0 then
				return false
			end

			XRootContext.GetSingleton():LoadAsyncScene(sceneID, function(scene)
				self.sceneCompleted = true

				local actors = self.entity:GetAllActor()
				if actors then
					self:LoadActorAsync(actors)
				end

				self:LoadFightUIAsync()
			end)
		end

		UISystem.GetSingleton():OpenWidget(UIStyle.LOADING, func)
	end

	return true
end

function OnFightLoadUpdate(self)
	if self.sceneCompleted and self.herosCompleted and self.fightCompleted then
		self:SetState(FightState.Ready)
	end
end

function OnFightLoadExit(self)
	UISystem.GetSingleton():CloseWidget(UIStyle.LOADING)

	-- open battle ui
	if self.widget then
		self.widget:Show()
	end
end

function OnFightReadyEnter(self)
	self:ResetFightCamera()

	self:SetState(FightState.Fighting)
end

function OnFightingEnter(self)
	local actors = self.entity:GetAllActor()
	for idx, actor in pairs(actors) do
		self.entity:Enter(actor)
	end
end

function OnFightingUpdate(self)
end

function OnFightingExit(self)
end
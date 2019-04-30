module("HomeSystem", package.seeall)
setmetatable(HomeSystem, {__index=XEventModule})

function new(szName)
	local home = {
	}
	setmetatable(home, {__index=HomeSystem})
	home:Init(szName)

	return home
end

function Init(self, szName)
	XEventModule.Init(self, szName)

	-- get home entity
	self.entity = XEntityManager.GetSingleton():Query(resmng.SYS_HOME)
	if not self.entity then
		ERROR("Can't find entity %d", resmng.SYS_HOME)
	end

	self.hero = self.entity:GetRecommended()

	-- open system id
	self.loaded = false
	self.openID = 0
end

function OnActive(self)
	XRootContext.GetSingleton():LoadAsyncScene(resmng.SCENE_HOME, function(scene)
		self:OnSceneCompleted(scene)
	end)
end

function OnDetive(self)
	UISystem.GetSingleton():CloseWidget(UIStyle.HOME, function()
		local openID = self.openID or 0
		if openID <= 0 then
			return false
		end

		if self.hero then
			self.hero:Destroy()
		end

		local dispatcher = self:GetDispatcher()
		if dispatcher then
			local prop = resmng.propSystemById(openID)
			if prop then
				dispatcher:Load(prop.Name, 
					prop.ClassType, prop.Active > 0 and true or false)
			end
		end

		return true
	end)
end

function OnSceneCompleted(self, scene)
	UISystem.GetSingleton():OpenWidget(UIStyle.HOME, function(widget)
		self.widget = widget
		
		local skeleton = self.hero:GetSimpleSkeleton()
		local elements = self.hero:GetElements()
		
		XAvatarSystem.LoadAsync(skeleton, elements, function(skeleton)
			local go = GameObject.Instantiate(skeleton)
			if go then
				local shape = self.hero:AddComponent(XSimpleShape, go)
				if shape then
					shape:Flipped(false)
				end

				self.widget:SetShape(go)
			end
		end)
	end)

	self:SubscribeEvent(GuiEvent.EVT_HOMESELMODE, 	OnHomeSelectMode)
end

function OnHomeSelectMode(self, evtArgs)
	local selFunc = {
		[resmng.XPlayMode.Story] = resmng.SYS_TASK
	}

	local selected = selFunc[evtArgs.mode]
	if selected then
		-- set curent select task modle id
		self.openID = selected

		-- unload menu system
		self:Detive()	
	end

	return true
end
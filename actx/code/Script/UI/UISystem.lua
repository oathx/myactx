module("UISystem", mkSingleton)
setmetatable(UISystem, {__index=XLuaBehaviour})

function Init(self)
	self.widgets 	= {}
	self.cacheQueue	= {}
	self.uiWidth = 1334
	self.uiHeight = 750
end

function Startup(self)
	self.gameObject = GameObject.Find("UI")
	self:AddLuaBehaviour(self.gameObject)

	self.gameObject.layer = LayerMask.NameToLayer("UI")
	GameObject.DontDestroyOnLoad(self.gameObject)
	
	return true
end

function Awake(self)
	self.uiCamera 		= GameObject.Find("UICamera")
	self.bgCamera 		= GameObject.Find("BGCamera")
	self.bgCanvasObject	= GameObject.Find("BgCanvas")
	self.canvasObject	= GameObject.Find("Canvas")
	self.eventSystem	= GameObject.Find("EventSystem")
end

function Start(self)
	local aryTrans = {
		self.uiCamera, self.canvasObject, self.eventSystem
	}
	
	for idx, go in ipairs(aryTrans) do
		go.layer 			= LayerMask.NameToLayer("UI")
		go.transform:SetParent(self.transform, false)
	end

	local bgTrans = {
		self.bgCamera, self.bgCanvasObject
	}
	for idx, go in ipairs(bgTrans) do
		go.layer 			= LayerMask.NameToLayer("BGUI")
		go.transform:SetParent(self.transform, false)
	end

	self:InitComponent(self.eventSystem, EventSystem)
	self:InitComponent(self.eventSystem, StandaloneInputModule)

	self.bgCanvas = self:InitComponent(self.bgCanvasObject, Canvas)
	if self.bgCanvas then
		local bgScaler = self:InitComponent(self.bgCanvasObject, CanvasScaler)
	
		bgScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize
		bgScaler.referenceResolution = Vector2(self.uiWidth, self.uiHeight)
		bgScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand

		self.bguiCamera = self:InitComponent(self.bgCamera, Camera)
		self.bguiCamera.cullingMask = bit.lshift(1, LayerMask.NameToLayer("BGUI"))
		self.bguiCamera.depth 		= 2^16 - 2
		self.bguiCamera.clearFlags 	= CameraClearFlags.Depth
		
		self.bgCanvas.worldCamera = self.bguiCamera
		self.bgCanvas.renderMode = RenderMode.ScreenSpaceCamera
	end
	
	self.canvas = self:InitComponent(self.canvasObject, Canvas)
	if self.canvas then
		self.uguiCamera 			= self:InitComponent(self.uiCamera, Camera)
		self.uguiCamera.cullingMask = bit.lshift(1, self.gameObject.layer)
		self.uguiCamera.depth 		= 2^16 - 1
		self.uguiCamera.clearFlags 	= CameraClearFlags.Depth
		
		self.canvas.worldCamera		= self.uguiCamera
		self.canvas.renderMode 		= RenderMode.ScreenSpaceCamera	
		
		self.rectTransfrom 			= self.canvasObject:GetComponent(RectTransform)
		self.screenRect 			= self.rectTransfrom.rect;		
	end
	
	self.raycaster = self:InitComponent(self.canvasObject, GraphicRaycaster)
	
	self.canvasScaler = self:InitComponent(self.canvasObject, CanvasScaler)
	if self.canvasScaler then
		self.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize
		self.canvasScaler.referenceResolution = Vector2(self.uiWidth, self.uiHeight)
		self.canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand
	end
end

function InitComponent(self, obj, type)
	local comp = obj:GetComponent(type)
	if not comp then
		comp = obj:AddComponent(type)
	end

	return comp
end

function WorldToUIPoint(self, pos)
	if not Camera.main then
		return self.uguiCamera:WorldToScreenPoint(pos)
	end

	local wpos = Camera.main:WorldToScreenPoint(pos)
	return self.uguiCamera:ScreenToWorldPoint(wpos)	
end

function LoadAsyncWidget(self, id, szResource, classType, complete, cache)
	if not classType then
		ERROR("can't find ui class %s", classType)
	else
		local widget = self.widgets[id]
		if not widget then
			XRes.LoadAsync(szResource, GameObject, function(obj) 
				widget = self:CreateWidget(id, szResource, classType, obj, cache)
				
				if complete then
					complete(widget)
				end
			end)
		end
	end
end

function CreateWidget(self, id, szResource, classType, goResource, cache)
	local goWidget = GameObject.Instantiate(goResource)
	if not goWidget then
		ERROR("Can't instantiate GameObject(%s)", goResource)
	else
		goWidget.transform.position = Vector3.zero
		goWidget.transform:SetParent(self.canvasObject.transform, false)
		goWidget.name				= classType._NAME
		
		local rt = goWidget:GetComponent(RectTransform)
		if rt then
			rt.offsetMax 	= Vector2.zero
			rt.offsetMin 	= Vector2.zero
			rt.localScale	= Vector3.one
			rt.localPosition= Vector3.zero
		end
		
		local widget = classType.new(goWidget)
		if not widget then
			ERROR("Can't construct widget %s", classType._NAME)
		end
		
		if cache then
			self.widgets[id] = widget
		end
		
		INFO("Create widget id(%s) resource(%s), module(%s) cache(%s)",
			id, szResource, classType._NAME, cache)
				
		return widget
	
	end
end

function LoadWidget(self, id, szResource, classType, cache)
	if not classType then
		ERROR("can't find ui class type %s", szResource)
	else
		local widget = self.widgets[id]
		if not widget then
			local goResource = Resources.Load(szResource)
			if not goResource then
				ERROR("Can't load widget resource %s id(%d)", szResource, id)
			else
				return self:CreateWidget(id, szResource, classType, goResource, cache)
			end
		end
	end
end

function GetWidget(self, id)
    return self.widgets[id]
end

function UnloadWidget(self, id, complete)
	local widget = self.widgets[id]
	if widget then
		StartCoroutine(function()
			Yield()

			widget:Close(function()			
				local setting = widget:GetConfigure()
				if setting.cache then
					table.insert(self.cacheQueue, setting.ID)
				end

				local evtArgs = {
					style = setting.ID, widget = widget
				}

				XPluginManager.GetSingleton():FireEvent(GuiEvent.EVT_WINDOWCLOSED, evtArgs)

				if complete then
					complete()
				end
				
				GameObject.Destroy(
					widget.gameObject
					)
					
				self.widgets[id] = nil
			end)
		end)
	end
end

function Clearup(self)
	for id, widget in pairs(self.widgets) do
		GameObject.Destroy(widget.gameObject)
	end
	
	self.widgets = {}
end

function Shutdown(self)
	self:Clearup()
end

function OpenWidget(self, style, complete)
	local setting = UIConfigure[style]
	if not setting then
		ERROR("Can't find ui config(%s)", tostring(style))
	else
		local opened = function(widget)
			if widget then
				if not setting.active then
					widget:Hide()
				end

				widget:Configure(setting)

				local evtArgs = {
					widget = widget,
				}
				XPluginManager.GetSingleton():FireEvent(GuiEvent.EVT_WINDOWOPENED, evtArgs)
				
				if complete then
					complete(widget)
				end
			end
		end
		
		if setting.async then
			self:LoadAsyncWidget(setting.ID, setting.prefab, setting.uiClass, function(widget) 
				opened(widget)
			end, setting.cache)
		else
			opened(self:LoadWidget(setting.ID, setting.prefab,
				setting.uiClass, setting.cache))
		end
	end
end

function CloseWidget(self, style, complete)
    local set = UIConfigure[style]
	if set then
    	self:UnloadWidget(set.ID, complete)    
    end
end

function MessageBox(self, style, text, callback, args)
	self:OpenWidget(style, function(widget) 
		if widget then
			widget:Message(text, callback, args)
		end
	end)
end
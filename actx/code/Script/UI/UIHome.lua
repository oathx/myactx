module("UIHome", package.seeall)
setmetatable(UIHome, {__index=UIWidget})

UH_STORY 		= "UH_STORY"
UH_ABATTOIR 	= "UH_ABATTOIR"
UH_TASK			= "UH_TASK"
UH_DRILL		= "UH_DRILL"
UH_CHALLENGE	= "UH_CHALLENGE"
UH_MOUNT		= "UH_MOUNT"

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UIHome})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	UIWidget.Init(self, go)
end

function Awake(self)
	self:Install({
			UH_STORY, UH_ABATTOIR, UH_TASK, UH_DRILL, UH_CHALLENGE, UH_MOUNT
		})
end

function Start(self)
	local aryHomeMenu = {
		UH_STORY, 
		UH_ABATTOIR, 
		UH_TASK, 
		UH_DRILL, 
		UH_CHALLENGE
	}

	for idx, name in pairs(aryHomeMenu) do
		self:RegisterClickEvent(name, 	OnMenuClicked)
	end

	self:DOBackMove(aryHomeMenu, true)	
end

function Close(self, complete)
	local group = self.gameObject:GetComponentInChildren(CanvasGroup)
	if group then
		XStaticDOTween.DOFade(group, 0, 0.8)
	end
	
	local aryHomeMenu = {
		UH_STORY, 
		UH_ABATTOIR, 
		UH_TASK, 
		UH_DRILL, 
		UH_CHALLENGE
	}
	self:DOBackMove(aryHomeMenu, false, function() 
		if complete then
			complete()
		end
	end)	
end

function SetShape(self, go)
	local mount = self:Query(UH_MOUNT)
	if mount then
		go.transform.position 	= Vector3.zero
		go.transform.rotation 	= Quaternion.Euler(0, 0, 0)
		go.transform.localScale = Vector3.one
		go.transform:SetParent(mount.transform, false)

		local renders = go:GetComponentsInChildren(SkinnedMeshRenderer)
		for idx, smr in pairs(renders.Table) do
			smr.gameObject.layer = mount.gameObject.layer
		end
	end
end

function OnMenuClicked(self, goSend, evtData)
	local playModeMap = {
		[UH_STORY] 		= resmng.XPlayMode.Story,
		[UH_ABATTOIR] 	= resmng.XPlayMode.Abattoir,
		[UH_TASK] 		= resmng.XPlayMode.Task,
		[UH_DRILL] 		= resmng.XPlayMode.Drill,
		[UH_CHALLENGE]	= resmng.XPlayMode.Challenge
	}
	
	local selectMode = playModeMap[goSend.name] or 0
	if selectMode > 0 then
		local evtArgs = {
		}
		evtArgs.mode	= selectMode
		evtArgs.widget	= self
		
		XPluginManager.GetSingleton():FireEvent(GuiEvent.EVT_HOMESELMODE, evtArgs)		
	end
end
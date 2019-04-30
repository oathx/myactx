module("UIWidget", package.seeall)
setmetatable(UIWidget, {__index=XLuaBehaviour})

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UIWidget})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	-- current cache ui transform info
	self.cacheTransform = {
	}
	self.userData		= {}

	self:AddLuaBehaviour(go)
end

function Install(self, aryName)
	local aryTransform = self.gameObject:GetComponentsInChildren(Transform)
	for i=1, aryTransform.Length do
		for j, name in pairs(aryName) do
			if name == aryTransform[i].name then
				self.cacheTransform[name] = aryTransform[i].gameObject
			end
		end
	end
end

function Configure(self, cfg)
    self.setting = cfg
end

function GetConfigure(self)
    return self.setting
end

function SetUserData(self, userData)
	self.userData = userData
end

function GetUserData(self)
	return self.userData
end

function RegisterClickEvent(self, szName, evtCallback)
	local widget = self.cacheTransform[szName]
	if not widget then
		error("Can't find transform " .. szName)
	end

    TRACE("register clicked event [%s](%s)",
        szName, evtCallback)

	XUIEventTriggerListener.Get(widget, self).onClick = evtCallback
end

function RegisterClickObject(self, target, evtCallback)
	XUIEventTriggerListener.Get(target, self).onClick = evtCallback
end

function Query(self, name)
	return self.cacheTransform[name]
end

function Close(self, complete)
	if complete then
		complete()
	end		
end

function Show(self)
	if self.gameObject then
		self.gameObject:SetActive(true)
	end
end

function Hide(self)
	if self.gameObject then
		self.gameObject:SetActive(false)
	end
end

function Reset(self)
end

function AddModel(self, widgetName)
    local model = self:Query(widgetName)
    if model then
        return UIModel.new(model)
    end
end

function AddScreen(self, widgetName)
	local screen = self:Query(widgetName)
	if screen then
		return UIScreen.new(screen)
	end
end

function GetRect(self, name)
	local rectTrans = self:GetRectTransform(name)
	if rectTrans then
		return rectTrans.rect
	end
end

function GetRectTransform(self, name)
	local widget = self.cacheTransform[name]
	if widget then
		local rectTrans = widget:GetComponent(RectTransform)
		if rectTrans then
			return rectTrans
		end
	end
end

function GetSpriteImage(self, name)
	local widget = self.cacheTransform[name]
	if widget then
		local image = widget:GetComponent(Image)
		if image then
			return image
		end
	end
end

function SetText(self, name, text)
	local widget = self.cacheTransform[name]
	if widget then
		local lab = widget:GetComponent(Text)
		if lab then
			lab.text = text

        end
	end
end

function GetText(self, name)
    local widget = self.cacheTransform[name]
    if widget then
        local lab = widget:GetComponent(Text)
        if lab then
            return lab.text
        end
    end
end

function GetInputText(self, name)
    local widget = self.cacheTransform[name]
    if widget then
	    local ipt = widget:GetComponent(InputField)
	  	if ipt then
	  		return ipt.text
	  	end
    end
end

function SetInputText(self, name, text)
    local widget = self.cacheTransform[name]
    if widget then
	    local ipt = widget:GetComponent(InputField)
	  	if ipt then
	  		ipt.text = text
	  	end
    end
end

function SetInteractable(self, name, interactable)
    local widget = self.cacheTransform[name]
    if widget then
		local btn = widget:GetComponent(Button)
		if btn then
			btn.interactable = interactable
		end
	end
end

function DOCanvasGroupFade(self, from, to, duration, compelte)
	self.canvasGroup = self.gameObject:GetComponentInChildren(CanvasGroup)
	if not self.canvasGroup then
		self.canvasGroup = self.gameObject:AddComponent(CanvasGroup)
	end

	self.canvasGroup.alpha = from
	
	local tween = XStaticDOTween.DOFade(self.canvasGroup, to, duration)
	if tween then
		tween:OnComplete(function() 
			if compelte then
				compelte()
			end
		end)
	else
		if compelte then compelte() end
	end
end

function DOGroupMove(self, aryConfig, compelte)
	for idx, cfg in ipairs(aryConfig) do
		local rectTrans = self:GetRectTransform(cfg.name)
		if rectTrans then
			local newDir = cfg.dir and 1 or -1
			local newPos = rectTrans.localPosition
			local curPos = newPos

			if cfg.axis == resmng.MoveAxis.X then
				curPos = Vector3(curPos.x + ((rectTrans.rect.width + cfg.offset) * newDir), 
					curPos.y, curPos.z)
			elseif cfg.axis == resmng.MoveAxis.Y then
				curPos = Vector3(curPos.x, 
					curPos.y + ((rectTrans.rect.height + cfg.offset) * newDir), curPos.z)
			elseif cfg.axis == resmng.MoveAxis.Z then
				curPos = Vector3(curPos.x, curPos.y, curPos.z + cfg.offset)
			end

			rectTrans.localPosition = curPos

			local tween = XStaticDOTween.DOLocalMove(rectTrans, newPos, cfg.speed, false)
			if tween then
				tween:SetEase(XEase.Linear)
				tween:SetDelay(cfg.wait or 0)

				tween:OnComplete(function() 
					if compelte and idx == #aryConfig then
						compelte()
					end
				end)
			end				
		end
	end
end

function DOBackMove(self, aryMove, flag, compelte)
	for idx, name in ipairs(aryMove) do
		local rectTrans = self:GetRectTransform(name)
		if rectTrans then
			local newPos = Vector3.zero
			local newPos = Vector3.zero
			local twEase = XEase.OutBack
			local fSpeed = 0.3
			
			if flag then
				newPos = rectTrans.localPosition
				curPos = Vector3(newPos.x + rectTrans.rect.width, newPos.y, newPos.z)
				twEase = XEase.OutBack
				fSpeed = 0.3	
				rectTrans.localPosition = curPos
			else
				curPos = rectTrans.localPosition
				newPos = Vector3(curPos.x + rectTrans.rect.width * 1.1, curPos.y, curPos.z)
				twEase = XEase.InBack
				fSpeed = 0.15				
			end
			
			local tween = XStaticDOTween.DOLocalMove(rectTrans, 
				newPos, idx * fSpeed, false)
			if tween then
				tween:SetEase(twEase)
				
				tween:OnComplete(function() 
					if idx == #aryMove and compelte then 
						compelte() 
					end
				end)
			end
		end
	end
end


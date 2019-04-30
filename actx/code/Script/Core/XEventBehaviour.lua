module("XEventBehaviour", package.seeall)
setmetatable(XEventBehaviour, {__index=XEventModule})

function new(szName)
	local obj = {
	}
	setmetatable(obj, {__index=XEventBehaviour})
	obj:Init(szName)
	
	return obj
end

function Init(self, szName)
	XEventModule.Init(self, szName)
	
	self.gameObject = GameObject(szName)
	if self.gameObject then
		XLuaBehaviourScript.AddLuaBehaviourScript(self.gameObject, self)
	end
end

function Active(self)
	if not self.active then
		self.active = true
		self.gameObject:SetActive(true)

		self:OnActive()
	end
end

function Detive(self)
	if self.active then
		self.active = false
		self.gameObject:SetActive(false)

		self:OnDetive()
	end	
	
end





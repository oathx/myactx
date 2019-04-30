module("UILoading", package.seeall)
setmetatable(UILoading, {__index=UIWidget})

UL_TEXT = "UL_TEXT"
UL_PROC = "UL_PROC"

function new(go)
	local obj = {
	}
	setmetatable(obj, {__index=UILoading})
	obj:Init(go)
	
	return obj
end

function Init(self, go)
	UIWidget.Init(self, go)
end

function Awake(self)
	self:Install({
	})
end



module("UIModel", package.seeall)

function new(trans)
	local model = {
	}
	setmetatable(model, {__index=UIModel})
	model:Init(trans)

	return model
end

function Init(self, trans)
	self.mount = trans
end

module("LoginEntity", package.seeall)
setmetatable(LoginEntity, {__index=XEntity})

function new(id)
	local entity = {
	}
	setmetatable(entity, {__index=LoginEntity})
	entity:Init(id)

	return entity
end

function Init(self, id)
	XEntity.Init(self, id)

	self.userName = "Super"
	self.password = "Super"	
end

function GetUserName(self)
	return self.userName
end

function GetPassword(self)
	return self.password
end

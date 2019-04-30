module("HeroEntity", package.seeall)
setmetatable(HeroEntity, {__index=ActorEntity})

function new(cid, propID)
	local entity = {
	}
	setmetatable(entity, {__index=HeroEntity})
	entity:Init(cid, propID)

	return entity
end

function Init(self, cid, propID)
	ActorEntity.Init(self, cid, propID)

	self.isSync = false
	self.type = resmng.XActorType.Hero
	self.prop = resmng.propCharacterById(propID)
	if not self.prop then
		ELOG("Can't find character cid(%d) propID(%d)", cid, propID)
	end

	self.bornPos = Vector3.zero
end

function IsSync(self)
	return self.isSync
end

function SetBorn(self, pos)
	self.bornPos = pos
end

function GetBorn(self)
	return self.bornPos
end

function GetProp(self)
	return self.prop
end

function GetRenderSkeleton(self)
	return self.prop.Skeleton
end

function GetSimpleSkeleton(self)
	return self.prop.UISkeleton
end

function GetElements(self)
	local elements = {
	}

	local shapeIDs = self.prop.Shape or {}
	for idx, id in pairs(shapeIDs) do
		local shape = resmng.propShapeById(id)
		if shape then
			table.insert(elements, shape.Path)
		end
	end

	return elements
end

function CreateShape(self, go)
	self:DestroyShape()

	if not self.shape then
		self.shape = self:AddComponent(XHeroShape, go)
	end

	return self.shape
end

function GetShape(self)
	return self.shape
end

function DestroyShape(self)
	if self.shape and self.shape.gameObject then
		self.shape = self:RemoveComponent(XHeroShape)
	end
end

function IsReceiveAttack(self)
	local result = ActorEntity.IsReceiveAttack(self)
	if not result then
		return false
	end

	if self.shape then
		result = self.shape.gameObject.activeSelf
	end

	return result
end

function Enter(self)
	ActorEntity.Enter(self)

	local shape = self:GetShape()
	if shape then
		local boxComp = shape.gameObject:GetComponent(XBoxComponent)
		if boxComp then
			XBoxSystem.GetSingleton():Register(boxComp)
		end

		shape:Enter()
	end
end

function Leave(self)
	ActorEntity.Leave(self)

	local shape = self:GetShape()
	if shape then
		local boxComp = shape.gameObject:GetComponent(XBoxComponent)
		if boxComp then
			XBoxSystem.GetSingleton():UnRegister(boxComp)
		end

		shape:Leave()
	end	
end
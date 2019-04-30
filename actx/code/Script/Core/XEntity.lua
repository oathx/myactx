module("XEntity", mkcall)

function new(id)
	local entity = {
	}
	setmetatable(entity, {__index=XEntity})
	entity:Init(id)

	return entity
end

function Init(self, id)
	self.random		= XRandom.new(0, id)
	self.components = {}
	self.id 		= id or 0
	self.active		= true
	self.propertys	= {}
end

function cid(self)
	return self.id
end

function Int(self, min, max)
	return self.random:Int(min, max)
end

function SetSeed(self, seed)
	self.random:SetSeed(seed)
end

function Float(self, min, max)
	return self.random:Float(min, max)
end

function AddProperty(self, index, value)
	local prop = XProperty(value)
	if prop then
		self.propertys[index] = prop
	end

	return prop
end

function GetProperty(self, index)
	return self.propertys[index]
end

function SetPropertyValue(self, index, value)
	local property = self.propertys[index]
	if property then
		local oldValue = property:Get()
		if oldValue ~= value then
			property:Set(value)

			-- if property value changed, notify
			local funcs = self:GetPropertyChangedFuncs() or {}
			for idx, func in ipairs(funcs) do
				func(self, oldValue, value, index)
			end
		end
	end
end

function GetPropertyValue(self, index)
	local property = self.propertys[index]
	if property then
		return property:Get()
	end
end

function RegisterPropertyFunc(self, index, func)
	if not self.propertyFuncs[index] then
		self.propertyFuncs[index] = {}
	end

	table.insert(self.propertyFuncs, func)
end

function GetPropertyFuncs(self, index)
	return self.propertyFuncs[index]
end

function ResetPropertyFuncs(self, index)
	self.propertyFuncs[index] = {}
end

function AddComponent(self, component, ...)
	local comp = component.new(self, ...)
	if comp then
		if comp.gameObject then
		else
			-- if the compoenent not a GameObject then final call OnDestroy
			comp:Awake()
			comp:Start()
		end

		table.insert(self.components, comp)
		
		return comp
	end
end

function RemoveComponent(self, component)
	for idx, comp in ipairs(self.components) do
		if comp:TypeName() == component:TypeName() then
			if comp.gameObject then
				GameObject.Destroy(comp.gameObject)
			else
				comp:OnDestroy()
			end

			table.remove(self.components, idx)

			break
		end
	end
end

function GetComponent(self, component)
	for idx, comp in ipairs(self.components) do
		if comp:TypeName() == component._NAME then
			return comp
		end
	end
end

function GetComponents(self)
	return self.components
end

function Destroy(self)
	self.propertyFuncs = {}
	for idx, comp in ipairs(self.components) do
		-- if the compoenent not a GameObject then final call OnDestroy
		if comp.gameObject then
			GameObject.Destroy(comp.gameObject)
		else
			comp:OnDestroy()
		end
	end

	self.components = {}
end


module("XEntityComponent", package.seeall)
setmetatable(XEntityComponent, {__index=XStateMachine})

function new(entity, ...)
	local comp = {
	}
	setmetatable(comp, {__index=XEntityComponent})
	comp:Init(entity, ...)

	return comp
end

function Init(self, entity, ...)
	XStateMachine.Init(self)

	self.typeCode 	= self._NAME
	self.entity 	= entity
end

-- If the entity component is a Unity GameObject then need add MonoBehaviour
function AddBehaviour(self, go)
	return XLuaBehaviourScript.AddLuaBehaviourScript(go, self)
end

function TypeName(self)
	return self.typeCode
end

function GetEntity(self)
	return self.entity
end

-- Awake is called when the script instance is being loaded.
function Awake(self)
end

--Start is called on the frame
--when a script is enabled just before any of the Update methods is called the first time.
function Start(self)
end

--Update is called every frame, if the MonoBehaviour is enabled.
function Update(self)
end

--This function is called when the MonoBehaviour will be destroyed.
function OnDestroy(self)
end

function FixedFrameUpdate(self, deltaTime)
	XStateMachine.Update(self, deltaTime)
end

function LateFrameUpdate(self, deltaTime)
end

function FixedFrameRender(self, deltaTime)
end

function IsActive(self)
	return true
end




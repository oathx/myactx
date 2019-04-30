module("XLuaBehaviour", package.seeall)

function AddLuaBehaviour(self, go)
    XLuaBehaviourScript.AddLuaBehaviourScript(go, self)
end

function Active(self)
	self.gameObject:SetActive(true)
end

function Detive(self)
	self.gameObject:SetActive(false)
end


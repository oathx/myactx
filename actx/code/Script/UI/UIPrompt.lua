module("UIPrompt", package.seeall)
setmetatable(UIPrompt, {__index=UIWidget})

UP_TEXT = "UP_TEXT"

function new(go)
    local widget = {
    }
    setmetatable(widget, {__index=UIPrompt})
    widget:Init(go)

    return widget
end

function Init(self, go)
    UIWidget.Init(self, go)
end

function Awake(self)
    self:Install({
        UP_TEXT
    })
end

function Start(self)
    local vStart    = self.transform.position
    local vEnd      = Vector3(vStart.x, vStart.y + 40, vStart.z)
    
    local tween = XStaticDOTween.DOMove(self.transform, vEnd, 1.5, false)
    if tween then
        tween:OnComplete(function() 
            GameObject.Destroy(self.gameObject)    
        end)
    end
end

function Message(self, text)
   self:SetText(UP_TEXT, text)
end


module("XSceneManager", mkSingleton)

function Init(self)
	self.scenes = {
	}
end

function LoadAsyncScene(self, nSceneID, complete)
	local prop = resmng.propSceneById(nSceneID)
	if prop then
		XRes.LoadSceneAsync(prop.Path, function(name)
			local obser
			local sysID = prop.Script or 0
			if sysID ~= 0 then
				local sys = resmng.propSystemById(sysID)
				if sys then
					obser = self.gamePlugin:Load(sys.Name, 
							sys.ClassType, sys.Active == 1 and true or false)
				end
			end

			if complete then
				complete(prop, obser)
			end
		end)
	end	
end



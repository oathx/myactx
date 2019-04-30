module("XRandom", package.seeall)

function new(seed, cid)
	local rd = {
	}
	setmetatable(rd, {__index=XRandom})
	rd:Init(seed, cid)

	return rd
end

function Init(self, seed, cid)
	self.seed = seed or math.random(os.time())
	self.count = 0
	self.cid = cid
end

function SetSeed(self, seed)
	self.seed = seed
end

function GetSeed(self)
	return self.seed
end

function UpdateSeed(self)
	self.seed = (self.seed * 29 + 37) % 2147483647
	return self.seed
end

function Random(self, low, up)
	if not low then
		return self:UpdateSeed() * (1.0 / 2147483648)
	end

	if up and up > low then
		return low + self:UpdateSeed() % (up - low)
	end

	return 1 + self:UpdateSeed() % low
end

function Int(self, min, max)
	return self:Random(min, max)
end

function Float(self, min, max)
	return min + (max - min) * (self:Random(0, 1000) * 0.001)
end
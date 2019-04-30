module("XFightTimer", package.seeall)

timer_list_ = {}
timer_add_list_ = {}
timer_delete_list_ = {}
timer_snid_ = 0
delta_tick_ = 0

function Add(delay, cycle, callback)
	timer_snid_ = timer_snid_ + 1

	local new_tm = {
		id = timer_snid_,
		delay = delay,
	}

	if cycle then
		if type(cycle) == "function" then
			new_tm.cycle = 0
			new_tm.exec = cycle
		elseif type(cycle) == "number" then
			new_tm.cycle = cycle
			new_tm.exec = callback
		end

		timer_add_list_[timer_snid_] = new_tm

		return timer_snid_
	end
end

function Tick(deltaTime)
	delta_tick_ = gfRoundInt(deltaTime * 1000)

	for snid, tm in pairs(timer_list_) do
		tm.delay = math.max(tm.delay - delta_tick_, 0)
		if tm.delay <= 0.01 then
			local added = false
			if tm.exec then 
				added = tm.exec(tm.id)
				if added == nil then
					added = true
				end
			end

			if added and tm.cycle > 0 then
				tm.delay = tm.cycle
			end				
		end

		if tm.delay <= 0.01 then
			timer_list_[snid] = nil
		end
	end

	for k,v in pairs(timer_add_list_) do
		timer_list_[k] = v
	end

	timer_add_list_ = {}
end

function get_delta_tick()
	return delta_tick_
end

function Delete(id)
	timer_list_[id] = nil
	if timer_add_list_[id] then
		timer_add_list_[id] = nil
	end
end

function DeleteAll()
	timer_add_list_ = {}
	timer_list_ = {}
	timer_snid_ = 0
end

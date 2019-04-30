module("NetImp", package.seeall)

function NetConnectSuccess()
	ELOG("NetConnectSuccess")
end

function NetConnectFailure()
	ELOG("NetConnectFailure")
end

function NetDisconnect()
	ELOG("NetDisconnect")
end

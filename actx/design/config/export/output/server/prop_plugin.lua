--
-- $Id$
--

module( "resmng" )
svnnum("$Id$")

propPlugin = {

	[PLUGIN_GAME] = { ID = PLUGIN_GAME, Name = "Game", ClassType = XGamePlugin, Active = 1, Version = 1,},
	[PLUGIN_SERVER] = { ID = PLUGIN_SERVER, Name = "Server", ClassType = nil, Active = nil, Version = nil,},
}

--
-- $Id$
--

module( "resmng" )
svnnum("$Id$")

propSystem = {

	[SYS_LOGIN] = { ID = SYS_LOGIN, Name = "Login", ClassType = "LoginSystem", Active = 1,},
	[SYS_HOME] = { ID = SYS_HOME, Name = "Home", ClassType = "HomeSystem", Active = 1,},
	[SYS_CONTROLLER] = { ID = SYS_CONTROLLER, Name = "Controller", ClassType = "ControllerSystem", Active = 1,},
	[SYS_TASK] = { ID = SYS_TASK, Name = "Task", ClassType = "TaskSystem", Active = 1,},
	[SYS_FIGHT] = { ID = SYS_FIGHT, Name = "Fight", ClassType = "FightSystem", Active = 1,},
	[SYS_MASTER] = { ID = SYS_MASTER, Name = "Master", ClassType = "MasterSystem", Active = 1,},
}

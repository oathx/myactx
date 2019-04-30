module("ActorFactory", package.seeall)

function ActorHero(cid, propID)
	return HeroEntity.new(cid, propID)
end

function ActorMaster(cid, propID)
	return MasterEntity.new(cid, propID)
end

function ActorPlayer(cid, propID)
	return PlayerEntity.new(cid, propID)
end

function ActorEnemy(cid, propID)
	return EnemyEntity.new(cid, propID) 
end

function ActorMonster(cid, propID)
end

function ActorStatic(cid, propID)
end

function ActorItem(cid, propID)
end

function ActorMoveable(cid, propID)

end

ActoryFactory = {
	[resmng.XActorType.Hero] = ActorHero,
	[resmng.XActorType.Master] =  ActorMaster,
	[resmng.XActorType.Player] = ActorPlayer,
	[resmng.XActorType.Enemy] = ActorEnemy,
	[resmng.XActorType.Monster] = ActorMonster,
	[resmng.XActorType.Static] = ActorStatic,
	[resmng.XActorType.Item] = ActorItem,
	[resmng.XActorType.Moveable] = ActorMoveable,
}

function Create(actorType, cid, propID)
	local create = ActoryFactory[actorType]
	if create then
		return create(cid, propID)
	end
end
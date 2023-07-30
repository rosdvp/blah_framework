﻿using System;

namespace Blah.Ecs
{
public class BlahEcsFilter
{
	private readonly IBlahEcsPool[] _incCompPools;
	private readonly IBlahEcsPool[] _excCompPools;

	private BlahEcsEntity[] _entities      = new BlahEcsEntity[2];
	private int[]           _entityIdToIdx = new int[1];
	private int             _entitiesCount = 1;

	private DelayedOp[] _delayedOps = new DelayedOp[1];
	private int         _delayedOpsCount;

	private int _goingIteratorsCount;

	public BlahEcsFilter() { }

	public BlahEcsFilter(BlahEcsWorld world, IBlahEcsPool[] incCompPools, IBlahEcsPool[] excCompPools)
	{
		_incCompPools = incCompPools;
		_excCompPools = excCompPools;

		(var set, int[] alivePtrs, int aliveCount) = world.GetEntities();
		for (var i = 0; i < aliveCount; i++)
		{
			ref var entity = ref set.Get(alivePtrs[i]);
			if (IsSuitable(entity.Id))
				AddEntity(entity);
		}
	}
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private bool Has(int entityId) => entityId < _entityIdToIdx.Length && _entityIdToIdx[entityId] != 0;

	
	private void OnCompIncAddedOrExcRemoved(BlahEcsEntity entity)
	{
		if (_goingIteratorsCount > 0)
		{
			if (_delayedOps.Length == _delayedOpsCount)
				Array.Resize(ref _delayedOps, _delayedOpsCount * 2);
			ref var op = ref _delayedOps[_delayedOpsCount++];
			op.Entity   = entity;
			op.IsTryAdd = true;
			return;
		}

		if (!Has(entity.Id) && IsSuitable(entity.Id))
			AddEntity(entity);
	}

	private void OnCompIncRemovedOrExcAdded(BlahEcsEntity entity)
	{
		if (_goingIteratorsCount > 0)
		{
			if (_delayedOps.Length == _delayedOpsCount)
				Array.Resize(ref _delayedOps, _delayedOpsCount * 2);
			ref var op = ref _delayedOps[_delayedOpsCount++];
			op.Entity   = entity;
			op.IsTryAdd = false;
			return;
		}
		
		if (!Has(entity.Id))
			return;
		
		int idx = _entityIdToIdx[entity.Id];
		_entityIdToIdx[entity.Id] = 0;
		if (_entitiesCount == 2)
			_entitiesCount = 1;
		else
			_entities[idx] = _entities[--_entitiesCount];
	}

	private bool IsSuitable(int entityId)
	{
		foreach (var pool in _incCompPools)
			if (!pool.Has(entityId))
				return false;
		foreach (var pool in _excCompPools)
			if (pool.Has(entityId))
				return false;
		return true;
	}

	private void AddEntity(BlahEcsEntity entity)
	{
		if (_entities.Length == _entitiesCount)
			Array.Resize(ref _entities, _entitiesCount * 2);
		if (entity.Id >= _entityIdToIdx.Length)
			Array.Resize(ref _entityIdToIdx, entity.Id * 2);

		int idx = _entitiesCount++;
		_entities[idx]            = entity;
		_entityIdToIdx[entity.Id] = idx;
	}


	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private void ApplyDelayedOps()
	{
		for (var i = 0; i < _delayedOpsCount; i++)
		{
			if (_delayedOps[i].IsTryAdd)
				OnCompIncAddedOrExcRemoved(_delayedOps[i].Entity);
			else
				OnCompIncRemovedOrExcAdded(_delayedOps[i].Entity);
		}
		_delayedOpsCount = 0;
	}
	
	private struct DelayedOp
	{
		public bool          IsTryAdd;
		public BlahEcsEntity Entity;
	}
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public Enumerator GetEnumerator()
	{
		_goingIteratorsCount += 1;
		return new Enumerator(this, _entities, _entitiesCount);
	}

	private void OnIterationEnd()
	{
		if (--_goingIteratorsCount == 0 && _delayedOpsCount > 0)
			ApplyDelayedOps();
	}
	

	public struct Enumerator : IDisposable
	{
		private readonly BlahEcsFilter    _owner;
		private readonly BlahEcsEntity[] _entities;
		private readonly int             _entitiesCount;

		private int _cursor;

		public Enumerator(BlahEcsFilter owner, BlahEcsEntity[] entities, int entitiesCount)
		{
			_owner         = owner;
			_entities      = entities;
			_entitiesCount = entitiesCount;
			_cursor        = 0; // entities starts from 1, but we need 0 for first MoveNext
		}
		
		public BlahEcsEntity Current => _entities[_cursor];

		public bool MoveNext() => ++_cursor < _entitiesCount;

		public void Dispose() => _owner.OnIterationEnd();
	}
}
}
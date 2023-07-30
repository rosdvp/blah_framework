﻿using System;
using System.Collections.Generic;
using Blah.Pools;
using Blah.Services;
using Blah.Systems;

namespace Blah.Features.Tests
{
internal class MockFeatureInvalidService : BlahFeatureBase
{
	public override bool IsEnabled => true;

	public override int SystemsGroupId => 0;

	public override HashSet<Type> Consumers { get; } = new()
	{
		typeof(MockCmdA),
	};

	public override HashSet<Type> Producers { get; } = new()
	{
		typeof(MockEventA)
	};

	public override IReadOnlyList<Type> Systems { get; } = new[]
	{
		typeof(MockInvalidSystem)
	};

	public override HashSet<Type> Services { get; } = new()
	{
		typeof(MockServiceA)
	};


	public class MockInvalidService : BlahServiceBase
	{
		protected override void InitImpl(IBlahServicesInitData initData, IBlahServicesContainerLazy services) { }
	}
	
	public class MockInvalidSystem : IBlahRunSystem
	{
		private MockInvalidService _service;

		public void Run() { }
	}
}
}
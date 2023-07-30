using System;
using System.Collections.Generic;

namespace Blah.Features
{
public abstract class BlahFeatureBase
{
	public abstract bool IsEnabled      { get; }
	public abstract int  SystemsGroupId { get; }

	public abstract HashSet<Type> Consumers { get; }
	
	public abstract HashSet<Type> Producers { get; }

	public abstract HashSet<Type> Services { get; }

	public abstract IReadOnlyList<Type> Systems { get; }
}
}

using System;

namespace ZFrameWork
{
	public interface IDynamicProperty
	{
		void DoChangeProperty(int id, object oldValue, object newValue);
		PropertyItem GetProperty(int id);
	}
}


using System;

namespace RedLoader.Fixes
{
	internal static class UnhandledException
	{
		internal static void Install(AppDomain domain) =>
			domain.UnhandledException +=
				(sender, args) =>
					RLog.Error((args.ExceptionObject as Exception).ToString());
	}
}
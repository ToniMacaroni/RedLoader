using System.Threading.Tasks;
using System.IO;
using System;
namespace UnityGLTF.Loader
{
	[Obsolete("Please switch to IDataLoader.  This interface is deprecated and will be removed in a future release.")]
	public interface ILoader
	{
		Task LoadStream(string relativeFilePath);

		void LoadStreamSync(string jsonFilePath);

		Stream LoadedStream { get; }

		bool HasSyncLoadMethod { get; }
	}
}

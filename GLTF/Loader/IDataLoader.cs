using System.Threading.Tasks;
using System.IO;
namespace UnityGLTF.Loader
{
	public interface IDataLoader
	{
		Task<Stream> LoadStreamAsync(string relativeFilePath);
	}
}

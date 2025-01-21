using Unity.Collections;
using GLTF.Schema;


namespace GLTF
{
	public class AttributeAccessor
	{
		public AccessorId AccessorId { get; set; }
		public NumericArray AccessorContent { get; set; }
		
		public NativeArray<byte> bufferData { get; set; }
		public uint Offset { get; set; }

		public AttributeAccessor()
		{
			AccessorContent = new NumericArray();
		}
	}
}

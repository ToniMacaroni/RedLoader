using System.Collections.Generic;
using Tomlet;
using Tomlet.Models;

namespace RedLoader
{
    public class TomlMapper
    {
        static TomlMapper()
             => TomletMain.RegisterMapper(WriteLemonTupleInt, ReadLemonTupleInt);

        public T[] ReadArray<T>(TomlValue value) => TomletMain.To<T[]>(value);
        public TomlArray WriteArray<T>(T[] value) => (TomlArray)TomletMain.ValueFrom(value);

        public List<T> ReadList<T>(TomlValue value) => TomletMain.To<List<T>>(value);
        public TomlArray WriteList<T>(List<T> value) => (TomlArray)TomletMain.ValueFrom(value);

        public TomlValue ToToml<T>(T value) => TomletMain.ValueFrom(value);
        public T FromToml<T>(TomlValue value) => TomletMain.To<T>(value);

        private static TomlValue WriteLemonTupleInt(LemonTuple<int, int> value)
        {
            int[] ints = new[] { value.Item1, value.Item2 };
            return ConfigSystem.Mapper.WriteArray(ints);
        }

        private static LemonTuple<int, int> ReadLemonTupleInt(TomlValue value)
        {
            int[] ints = ConfigSystem.Mapper.ReadArray<int>(value);
            if (ints == null || ints.Length != 2)
                return default;
            return new LemonTuple<int, int>() { Item1 = ints[0], Item2 = ints[1] };
        }
    }
}

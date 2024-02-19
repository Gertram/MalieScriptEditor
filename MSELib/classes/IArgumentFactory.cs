using System.IO;
using System;
using System.Collections.Generic;

namespace MSELib.classes
{
    public interface IArgumentFactory
    {
        IArgument Create();
    }
    internal class ArgumentFactory<T> : IArgumentFactory where T : IArgument, new()
    {
        public IArgument Create()
        {
            return new T();
        }
    }
    internal class ByteArgument : IArgument
    {
        protected byte _value;
        public virtual uint Value
        {
            get => _value;
            set {}
        }

        public virtual void Read(BinaryReader reader, IDictionary<uint, StringsItem> contents)
        {
            _value = reader.ReadByte();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_value);
        }
    }
    internal class UshortArgument : IArgument
    {
        protected ushort _value;
        public virtual uint Value
        {
            get => _value;
            set { }
        }

        public virtual void Read(BinaryReader reader, IDictionary<uint, StringsItem> contents)
        {
            _value = reader.ReadUInt16();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_value);
        }
    }
    internal class UintArgument : IArgument
    {
        protected uint _value;
        public virtual uint Value
        {
            get => _value;
            set{}
        }

        public virtual void Read(BinaryReader reader, IDictionary<uint, StringsItem> contents)
        {
            _value = reader.ReadUInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(_value);
        }
    }
    internal class ByteStrArgument : ByteArgument
    {
        public override uint Value
        {
            get => base.Value;
            set
            {
                if (value > 0xFF)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _value = (byte)value;
            }
        }
        public override void Read(BinaryReader reader, IDictionary<uint, StringsItem> contents)
        {
            base.Read(reader, contents);

            if (!contents.TryGetValue(_value, out var stringsItem))
            {
                throw new Exception("Doesn't found string for argument");
            }
            stringsItem.Arguments.Add(this);
        }
    }
    internal class UshortStrArgument : UshortArgument
    {
        public override uint Value
        {
            get => base.Value;
            set
            {
                if (value > 0xFFFF)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _value = (byte)value;
            }
        }
        public override void Read(BinaryReader reader, IDictionary<uint, StringsItem> contents)
        {
            base.Read(reader, contents);

            if (!contents.TryGetValue(_value, out var stringsItem))
            {
                throw new Exception("Doesn't found string for argument");
            }
            stringsItem.Arguments.Add(this);
        }
    }
    internal class UintStrArgument : UintArgument
    {
        public override uint Value 
        {
            get => base.Value; 
            set => _value = value;
        }
        public override void Read(BinaryReader reader, IDictionary<uint, StringsItem> contents)
        {
            base.Read(reader, contents);

            if (!contents.TryGetValue(_value, out var stringsItem))
            {
                throw new Exception("Doesn't found string for argument");
            }
            stringsItem.Arguments.Add(this);
        }
    }
    internal class ByteFunctionArgument : ByteArgument { }
    internal class UintFunctionArgument : UintArgument { }
}

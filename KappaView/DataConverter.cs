using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KappaView
{
    public class DataConverter
    {
        private static (string type, int size) Separate(string datatype)
        {
            // Colon is used to split the parameter into datatype name and size of datatype if it's an array
            int colon = datatype.IndexOf(':');
            string type = colon > -1 ? datatype.Substring(0, colon) : datatype;
            int size = colon > -1 ? Convert.ToInt32(datatype.Substring(colon + 1)) : 1;

            return (type, size);
        }

        public static int GetSize(string datatype)
        {
            (string type, int size) = Separate(datatype);

            return Marshal.SizeOf(GetDataType(type)) * size;
        }

        public static dynamic GetDataType(string datatype)
        {
            try
            {
                (string type, int size) = Separate(datatype);

                if (type == "Bool")
                {
                    if (size > 1)
                        return new bool[size];
                    else
                        return new bool();
                }
                else if (type == "Float")
                {
                    if (size > 1)
                        return new float[size];
                    else
                        return new float();
                }
                else if (type == "Double")
                {
                    if (size > 1)
                        return new double[size];
                    else
                        return new double();
                }
                else if (type == "Char")
                {
                    if (size > 1)
                        return new char[size];
                    else
                        return new char();
                }
                else if (type.Contains("Int"))
                {
                    if (type == "Int8")
                    {
                        if (size > 1)
                            return new sbyte[size];
                        else
                            return new sbyte();
                    }
                    else if (type == "UInt8")
                    {
                        if (size > 1)
                            return new byte[size];
                        else
                            return new byte();
                    }
                    if (type == "Int16")
                    {
                        if (size > 1)
                            return new short[size];
                        else
                            return new short();
                    }
                    else if (type == "UInt16")
                    {
                        if (size > 1)
                            return new ushort[size];
                        else
                            return new ushort();
                    }
                    if (type == "Int32")
                    {
                        if (size > 1)
                            return new int[size];
                        else
                            return new int();
                    }
                    else if (type == "UInt32")
                    {
                        if (size > 1)
                            return new uint[size];
                        else
                            return new uint();
                    }
                    if (type == "Int64")
                    {
                        if (size > 1)
                            return new long[size];
                        else
                            return new long();
                    }
                    else if (type == "UInt64")
                    {
                        if (size > 1)
                            return new ulong[size];
                        else
                            return new ulong();
                    }
                }
                else
                {
                    throw new Exception(string.Format("Unknown datatype: {0}.", type));
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static dynamic ConvertBuffer(string datatype, byte[] buffer)
        {
            try
            {
                (string type, int size) = Separate(datatype);

                if (type == "Bool")
                {
                    if (size > 1)
                        return Array.ConvertAll(buffer, val => val == 0 ? false : true);
                    else
                        return buffer[0] == 0 ? false : true;
                }
                else if (type == "Float")
                {
                    if (size > 1)
                    {
                        float[] temp = new float[size];
                        for (int i = 0; i < size; i++)
                        {
                            temp[i] = BitConverter.ToSingle(buffer, i * sizeof(float));
                        }
                        return temp;
                    }
                    else
                        return BitConverter.ToSingle(buffer, 0);
                }
                else if (type == "Double")
                {
                    if (size > 1)
                    {
                        double[] temp = new double[size];
                        for (int i = 0; i < size; i++)
                        {
                            temp[i] = BitConverter.ToDouble(buffer, i * sizeof(double));
                        }
                        return temp;
                    }
                    else
                        return BitConverter.ToDouble(buffer, 0);
                }
                else if (type == "Char")
                {
                    return Encoding.ASCII.GetString(buffer);
                }
                else if (type.Contains("Int"))
                {
                    if (type == "Int8")
                    {
                        if (size > 1)
                            return Array.ConvertAll(buffer, val => unchecked((sbyte)val));
                        else
                            return unchecked((sbyte)buffer[0]);
                    }
                    else if (type == "UInt8")
                    {
                        if (size > 1)
                            return buffer;
                        else
                            return buffer[0];
                    }
                    if (type == "Int16")
                    {
                        if (size > 1)
                        {
                            short[] temp = new short[size];
                            for (int i = 0; i < size; i++)
                            {
                                temp[i] = BitConverter.ToInt16(buffer, i * sizeof(short));
                            }
                            return temp;
                        }
                        else
                            return BitConverter.ToInt16(buffer, 0);
                    }
                    else if (type == "UInt16")
                    {
                        if (size > 1)
                        {
                            ushort[] temp = new ushort[size];
                            for (int i = 0; i < size; i++)
                            {
                                temp[i] = BitConverter.ToUInt16(buffer, i * sizeof(ushort));
                            }
                            return temp;
                        }
                        else
                            return BitConverter.ToUInt16(buffer, 0);
                    }
                    if (type == "Int32")
                    {
                        if (size > 1)
                        {
                            int[] temp = new int[size];
                            for (int i = 0; i < size; i++)
                            {
                                temp[i] = BitConverter.ToInt32(buffer, i * sizeof(int));
                            }
                            return temp;
                        }
                        else
                            return BitConverter.ToInt32(buffer, 0);
                    }
                    else if (type == "UInt32")
                    {
                        if (size > 1)
                        {
                            uint[] temp = new uint[size];
                            for (int i = 0; i < size; i++)
                            {
                                temp[i] = BitConverter.ToUInt32(buffer, i * sizeof(uint));
                            }
                            return temp;
                        }
                        else
                            return BitConverter.ToUInt32(buffer, 0);
                    }
                    if (type == "Int64")
                    {
                        if (size > 1)
                        {
                            long[] temp = new long[size];
                            for (int i = 0; i < size; i++)
                            {
                                temp[i] = BitConverter.ToInt64(buffer, i * sizeof(long));
                            }
                            return temp;
                        }
                        else
                            return BitConverter.ToInt64(buffer, 0);
                    }
                    else if (type == "UInt64")
                    {
                        if (size > 1)
                        {
                            ulong[] temp = new ulong[size];
                            for (int i = 0; i < size; i++)
                            {
                                temp[i] = BitConverter.ToUInt64(buffer, i * sizeof(ulong));
                            }
                            return temp;
                        }
                        else
                            return BitConverter.ToUInt64(buffer, 0);
                    }
                }
                else
                {
                    throw new Exception(string.Format("Unknown datatype: {0}.", type));
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static dynamic ConvertValue(string datatype, dynamic value)
        {
            try
            {
                (string type, int size) = Separate(datatype);

                if (size > 1)
                {
                    throw new Exception("Array conversion is not ready yet.");
                }

                if (type == "Bool")
                {
                    return (bool)value;
                }
                else if (type == "Float")
                {
                    return (float)value;
                }
                else if (type == "Double")
                {
                    return (double)value;
                }
                else if (type == "Char")
                {
                    return (string)value;
                }
                else if (type.Contains("Int"))
                {
                    if (type == "Int8")
                    {
                        return (sbyte)value;
                    }
                    else if (type == "UInt8")
                    {
                        return (byte)value;
                    }
                    if (type == "Int16")
                    {
                        return (short)value;
                    }
                    else if (type == "UInt16")
                    {
                        return (ushort)value;
                    }
                    if (type == "Int32")
                    {
                        return (int)value;
                    }
                    else if (type == "UInt32")
                    {
                        return (uint)value;
                    }
                    if (type == "Int64")
                    {
                        return (long)value;
                    }
                    else if (type == "UInt64")
                    {
                        return (ulong)value;
                    }
                }
                else
                {
                    throw new Exception(string.Format("Unknown datatype: {0}.", type));
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}

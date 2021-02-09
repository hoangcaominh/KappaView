using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using NLua;

namespace KappaView
{
    public class ResourceReader
    {
        public class ResourceObject
        {
            public string Name, Type, DataType, Description;
            public List<string> Address, Parameters;
            public Dictionary<string, string> Display;
        }

        public class ResetObject
        {
            public LuaFunction Function;
            public List<string> Parameters;
        }

        private readonly JObject json;
        public Version Version;
        public string Target;
        public int Platform;

        private readonly Lua state = new Lua();
        private readonly List<string> LuaImport;
        private readonly ResetObject reset = new ResetObject();

        private readonly Dictionary<string, ResourceObject> ResourceList = new Dictionary<string, ResourceObject>();
        private readonly Dictionary<string, LuaFunction> FunctionList = new Dictionary<string, LuaFunction>();
        private readonly Dictionary<string, dynamic> PrintList = new Dictionary<string, dynamic>();
        private Dictionary<string, dynamic> ResourceValueList = new Dictionary<string, dynamic>();

        private readonly bool Okay = false;

        public bool IsOkay()
        {
            return Okay;
        }

        public ResourceReader(string path)
        {
            try
            {
                // Load profile
                json = JObject.Parse(File.ReadAllText(path));

                foreach (var obj in json)
                {
                    if (obj.Key == "Version")
                    {
                        Version = Version.Parse(obj.Value.ToObject<string>());
                    }
                    else if (obj.Key == "Target")
                    {
                        Target = obj.Value.ToObject<string>();
                    }
                    else if (obj.Key == "Platform")
                    {
                        Platform = obj.Value.ToObject<int>();
                        if (Platform != 32 && Platform != 64)
                        {
                            throw new Exception(string.Format("Invalid platform: {0}.", Platform));
                        }
                    }
                    else if (obj.Key == "Import")
                    {
                        LuaImport = obj.Value.ToObject<List<string>>();
                    }
                    else if (obj.Key == "Reset")
                    {
                        JObject reset_obj = JObject.Parse(obj.Value.ToString());
                        if (!reset_obj.ContainsKey("Parameters"))
                        {
                            throw new Exception("Parameters for reset function not defined.");
                        }
                        reset.Parameters = reset_obj["Parameters"].ToObject<List<string>>();
                    }
                    else
                    {
                        ResourceObject resource = new ResourceObject();
                        foreach (var attr in JObject.Parse(obj.Value.ToString()))
                        {
                            if (attr.Key == "Name")
                            {
                                resource.Name = attr.Value.ToObject<string>();
                            }
                            else if (attr.Key == "Description")
                            {
                                resource.Description = attr.Value.ToObject<string>();
                            }
                            else if (attr.Key == "DataType")
                            {
                                resource.DataType = attr.Value.ToObject<string>();
                            }
                            else if (attr.Key == "Type")
                            {
                                resource.Type = attr.Value.ToObject<string>();
                            }
                            else if (attr.Key == "Address")
                            {
                                resource.Address = attr.Value.ToObject<List<string>>();
                            }
                            else if (attr.Key == "Display")
                            {
                                resource.Display = attr.Value.ToObject<Dictionary<string, string>>();
                            }
                            else if (attr.Key == "Parameters")
                            {
                                resource.Parameters = attr.Value.ToObject<List<string>>();
                            }
                            else
                            {
                                throw new Exception(string.Format("Unknown key defined: {0}.\nIn variable: {1}.", attr.Key, obj.Key));
                            }
                        }
                        if (resource.Name == null)
                        {
                            throw new Exception(string.Format("\"Name\" not defined.\nIn variable: {0}.", obj.Key));
                        }
                        else if (resource.DataType == null)
                        {
                            throw new Exception(string.Format("\"DataType\" not defined.\nIn variable: {0}.", obj.Key));
                        }
                        else if (resource.Type == null)
                        {
                            throw new Exception(string.Format("\"Type\" not defined.\nIn variable: {0}.", obj.Key));
                        }
                        else if (resource.Type != "Static" && resource.Type != "Counter" && resource.Type != "Custom")
                        {
                            throw new Exception(string.Format("Unknown value type defined: {0}.\nIn variable: {1}.", resource.Type, obj.Key));
                        }
                        else if ((resource.Type == "Static" || resource.Type == "Counter") && resource.Address == null)
                        {
                            throw new Exception(string.Format("\"Address\" not defined.\nIn variable: {0}.", obj.Key));
                        }
                        else if (resource.Type == "Custom" && resource.Parameters == null)
                        {
                            throw new Exception(string.Format("Parameters for custom function not defined.\nIn variable: {0}.", obj.Key));
                        }
                        else
                        {
                            ResourceList.Add(obj.Key, resource);
                            ResourceValueList.Add(obj.Key, DataConverter.GetDataType(resource.DataType));
                            PrintList.Add(obj.Key, resource.Type == "Static" ? DataConverter.GetDataType(resource.DataType) : 0);
                        }
                    }
                }

                if (Version == null)
                {
                    throw new Exception("\"Version\" not defined.");
                }
                else if (Target == null)
                {
                    throw new Exception("\"Target\" not defined.");
                }
                else if (Platform == 0)
                {
                    throw new Exception("\"Platform\" not defined.");
                }
                else if (LuaImport == null)
                {
                    throw new Exception("\"Import\" not defined.");
                }
                else
                {
                    // Do nothing
                }

                // Load Lua functions
                foreach (string file in LuaImport)
                {
                    state.DoFile(Path.Combine(Directory.GetParent(path).FullName, file));
                }

                reset.Function = state["Reset"] as LuaFunction;
                if (reset.Parameters != null && reset.Function == null)
                {
                    throw new Exception("Function Reset not found.");
                }

                foreach (var obj in ResourceList)
                {
                    if (obj.Value.Type == "Counter" || obj.Value.Type == "Custom")
                    {
                        if (!(state[obj.Key] is LuaFunction function))
                        {
                            throw new Exception(string.Format("Function \"{0}\" not found.", obj.Key));
                        }
                        FunctionList.Add(obj.Key, function);
                    }
                }

                Okay = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Parse Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        private IntPtr handle;

        public bool GetProcess()
        {
            var proc = Process.GetProcessesByName(Target);
            if (proc.Length > 0)
            {
                // PROCESS_VM_READ (0x10)
                handle = OpenProcess(0x10, false, proc.FirstOrDefault().Id);

                if (handle != null)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public List<Tuple<string, object>> Read()
        {
            // Read
            Dictionary<string, dynamic> temp = new Dictionary<string, dynamic>();

            try
            {
                List<dynamic> parameters_value = new List<dynamic>();
                foreach (string key in reset.Parameters)
                {
                    parameters_value.Add(PrintList[key]);
                }
                // Reset counter
                if ((bool)reset.Function.Call(parameters_value.Cast<object>().ToArray()).First())
                {
                    foreach (var obj in ResourceList)
                    {
                        if (obj.Value.Type == "Counter")
                        {
                            PrintList[obj.Key] = 0;
                        }
                    }
                }

                foreach (var obj in ResourceList)
                {
                    if (obj.Value.Type == "Static" || obj.Value.Type == "Counter")
                    {
                        // Pointer address processing
                        byte[] p_buffer = new byte[8];
                        IntPtr bytesRead = IntPtr.Zero;
                        ulong address = 0;
                        bool read;
                        int i = 0;
                        string addr = obj.Value.Address[i];
                        while (i < obj.Value.Address.Count - 1)
                        {
                            read = ReadProcessMemory(handle, (IntPtr)(address + Convert.ToUInt64(addr, 16)), p_buffer, Platform / 8, out bytesRead);
                            if (!read)
                            {
                                throw new Exception(string.Format("Failed to read pointer's memory of variable {0}.", obj.Key));
                            }
                            address = BitConverter.ToUInt64(p_buffer, 0);
                            addr = obj.Value.Address[++i];
                        }

                        // Resource address processing
                        byte[] buffer = new byte[DataConverter.GetSize(obj.Value.DataType)];
                        read = ReadProcessMemory(handle, (IntPtr)(address + Convert.ToUInt64(addr, 16)), buffer, buffer.Length, out bytesRead);
                        if (!read)
                        {
                            throw new Exception(string.Format("Failed to read memory of variable {0}.", obj.Key));
                        }

                        temp.Add(obj.Key, DataConverter.ConvertBuffer(obj.Value.DataType, buffer));
                    }
                    else if (obj.Value.Type == "Custom")
                    {
                        parameters_value = new List<dynamic>();
                        foreach (string key in obj.Value.Parameters)
                        {
                            parameters_value.Add(PrintList[key]);
                        }
                        object val = FunctionList[obj.Key].Call(parameters_value.Cast<object>().ToArray()).First();
                        temp.Add(obj.Key, DataConverter.ConvertValue(ResourceList[obj.Key].DataType, val));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // Print
            List<Tuple<string, object>> ret = new List<Tuple<string, object>>();

            try
            {
                foreach (string key in ResourceList.Keys)
                {
                    if (!ResourceValueList[key].Equals(temp[key]))
                    {
                        if (ResourceList[key].Type == "Counter")
                        {
                            string datatype = ResourceList[key].DataType;
                            object[] param = { DataConverter.ConvertValue(datatype, ResourceValueList[key]), DataConverter.ConvertValue(datatype, temp[key]) };
                            
                            if (Convert.ToBoolean(FunctionList[key].Call(param).First()))
                            {
                                PrintList[key] = (int)PrintList[key] + 1;
                            }
                        }
                        // Static or Custom
                        else
                        {
                            PrintList[key] = temp[key];
                        }
                    }
                }
                ResourceValueList = temp;

                foreach (string key in ResourceList.Keys)
                {
                    if (ResourceList[key].Display != null)
                    {
                        // Display option
                        if (ResourceList[key].Display.Count > 0 && ResourceList[key].Display.ContainsKey(Convert.ToString(PrintList[key])))
                        {
                            ret.Add(new Tuple<string, object>(ResourceList[key].Name, ResourceList[key].Display[Convert.ToString(PrintList[key])]));
                        }
                        else
                        {
                            ret.Add(new Tuple<string, object>(ResourceList[key].Name, PrintList[key]));
                        }
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}

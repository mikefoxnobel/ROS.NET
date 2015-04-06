﻿// File: XmlRpcValue.cs
// Project: XmlRpc_Wrapper
// 
// ROS.NET
// Eric McCann <emccann@cs.uml.edu>
// UMass Lowell Robotics Laboratory
// 
// Reimplementation of the ROS (ros.org) ros_cpp client in C#.
// 
// Created: 11/06/2013
// Updated: 07/23/2014

#region USINGZ

//#define REFDEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace XmlRpc_Wrapper
{
    //TODO: OPERATOR GARBAGE?
    public class XmlRpcValue : IDisposable
    {
        #region Reference Tracking + unmanaged pointer management

        public XmlRpcValue this[int key]
        {
            [DebuggerStepThrough] get { return Get(key); }
            [DebuggerStepThrough] set { Set(key, value); }
        }

        public XmlRpcValue this[string key]
        {
            [DebuggerStepThrough] get { return Get(key); }
            [DebuggerStepThrough] set { Set(key, value); }
        }

        private IntPtr __instance;

        [DebuggerStepThrough]
        public void Dispose()
        {
            Clear();
        }

        [DebuggerStepThrough]
        ~XmlRpcValue()
        {
            Clear();
        }

        private static Dictionary<IntPtr, int> _refs = new Dictionary<IntPtr, int>();
        private static object reflock = new object();
#if REFDEBUG
        private static Thread refdumper;
        private static void dumprefs()
        {
            while (true)
            {
                Dictionary<IntPtr, int> dainbrammage = null;
                lock (reflock)
                {
                    dainbrammage = new Dictionary<IntPtr, int>(_refs);
                }
                Console.WriteLine("REF DUMP");
                foreach (KeyValuePair<IntPtr, int> reff in dainbrammage)
                {
                    Console.WriteLine("\t" + reff.Key + " = " + reff.Value);
                    Console.WriteLine("\t" + new XmlRpcValue(reff.Key));
                }
                Thread.Sleep(500);
            }
        }
#endif

        [DebuggerStepThrough]
        public static XmlRpcValue LookUp(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                AddRef(ptr);
                return new XmlRpcValue(ptr);
            }
            return null;
        }


        [DebuggerStepThrough]
        private static void AddRef(IntPtr ptr)
        {
#if REFDEBUG
            if (refdumper == null)
            {
                refdumper = new Thread(dumprefs);
                refdumper.IsBackground = true;
                refdumper.Start();
            }
#endif
            lock (reflock)
            {
                if (!_refs.ContainsKey(ptr))
                {
#if REFDEBUG
                    Console.WriteLine("Adding a new reference to: " + ptr + " (" + 0 + "==> " + 1 + ")");
#endif
                    _refs.Add(ptr, 1);
                }
                else
                {
#if REFDEBUG
                    Console.WriteLine("Adding a new reference to: " + ptr + " (" + _refs[ptr] + "==> " + (_refs[ptr] + 1) + ")");
#endif
                    _refs[ptr]++;
                }
            }
        }

        [DebuggerStepThrough]
        private static void RmRef(ref IntPtr ptr)
        {
            lock (reflock)
            {
                if (_refs.ContainsKey(ptr))
                {
                    if (ptr == IntPtr.Zero)
                        throw new Exception("OHHH NOOOO!!!");
#if REFDEBUG
                    Console.WriteLine("Removing a reference to: " + ptr + " (" + _refs[ptr] + "==> " + (_refs[ptr] - 1) + ")");
#endif
                    _refs[ptr]--;
                    if (_refs[ptr] == 0)
                    {
#if REFDEBUG
                        Console.WriteLine("KILLING " + ptr + " BECAUSE IT'S DEAD!");
#endif
                        _refs.Remove(ptr);
                        ptr = IntPtr.Zero;
                    }
                    return;
                }
                ptr = IntPtr.Zero;
            }
        }

        public IntPtr instance
        {
            [DebuggerStepThrough]
            get
            {
                if (__instance == IntPtr.Zero)
                {
                    Console.WriteLine("UH OH MAKING A NEW INSTANCE IN instance.get!");
                    __instance = create();
                    AddRef(__instance);
                }
                return __instance;
            }
            [DebuggerStepThrough]
            set
            {
                if (value != IntPtr.Zero)
                {
                    if (__instance != IntPtr.Zero)
                        RmRef(ref __instance);
                    AddRef(value);
                    __instance = value;
                }
            }
        }

        #endregion

        [DebuggerStepThrough]
        public XmlRpcValue()
        {
            __instance = create();
            AddRef(__instance);
        }

        [DebuggerStepThrough]
        public XmlRpcValue(params object[] initialvalues)
            : this()
        {
            for (int i = 0; i < initialvalues.Length; i++)
            {
                int ires = 0;
                double dres = 0;
                bool bres = false;
                if (initialvalues[i] == null)
                    Set(i, "");
                else if (initialvalues[i] is string)
                    Set(i, initialvalues[i].ToString());
                else if (initialvalues[i] is int && int.TryParse(initialvalues[i].ToString(), out ires))
                    Set(i, ires);
                else if (initialvalues[i] is double && double.TryParse(initialvalues[i].ToString(), out dres))
                    Set(i, dres);
                else if (initialvalues[i] is bool && bool.TryParse(initialvalues[i].ToString(), out bres))
                    Set(i, bres);
                else
                    Set(i, initialvalues[i].ToString());
            }
        }

        [DebuggerStepThrough]
        public XmlRpcValue(bool value)
        {
            __instance = create(value);
            AddRef(__instance);
        }

        [DebuggerStepThrough]
        public XmlRpcValue(int value)
        {
            __instance = create(value);
            AddRef(__instance);
        }

        [DebuggerStepThrough]
        public XmlRpcValue(double value)
        {
            __instance = create(value);

            AddRef(__instance);
        }

        [DebuggerStepThrough]
        public XmlRpcValue(string value)
        {
            __instance = create(value);
            AddRef(__instance);
        }

        [DebuggerStepThrough]
        public XmlRpcValue(XmlRpcValue value)
            : this(value.instance)
        {
        }

        [DebuggerStepThrough]
        public XmlRpcValue(IntPtr existingptr)
        {
            if (existingptr == IntPtr.Zero)
                throw new Exception("SUCK IS CONTAGEOUS!");
            __instance = existingptr;
            AddRef(existingptr);
        }

        #region P/Invoke

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Create1", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create();

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Create2", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create([MarshalAs(UnmanagedType.I1)] bool value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Create3", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create(int value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Create4", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create(double value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Create5", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create([In] [Out] [MarshalAs(UnmanagedType.LPStr)] string value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Create6", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create(IntPtr rhs);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Valid", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool valid(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_SetType", CallingConvention = CallingConvention.Cdecl)]
        private static extern int settype(IntPtr target, int type);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Type", CallingConvention = CallingConvention.Cdecl)]
        private static extern int gettype(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Size", CallingConvention = CallingConvention.Cdecl)]
        private static extern int getsize(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_SetSize", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setsize(IntPtr target, int size);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_HasMember", CallingConvention = CallingConvention.Cdecl)
        ]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool hasmember(IntPtr target, [In] [Out] [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Set1", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set(IntPtr target, [In] [Out] [MarshalAs(UnmanagedType.LPStr)] string value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Set3", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set(IntPtr target, IntPtr value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Set5", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set(IntPtr target, int value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Set7", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set(IntPtr target, [MarshalAs(UnmanagedType.I1)] bool value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Set9", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set(IntPtr target, double value);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Get1", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get(IntPtr target, int key);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Get2", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get(IntPtr target, [In] [Out] [MarshalAs(UnmanagedType.LPStr)] string key);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_GetInt0", CallingConvention = CallingConvention.Cdecl)]
        private static extern int getint(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_GetString0", CallingConvention = CallingConvention.Cdecl
            )]
        private static extern IntPtr getstring(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_GetBool0", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool getbool(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_GetDouble0", CallingConvention = CallingConvention.Cdecl
            )]
        private static extern double getdouble(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_Dump", CallingConvention = CallingConvention.Cdecl)]
        private static extern void dump(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcValue_ToString", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tostring(IntPtr target);

        #endregion

        public TypeEnum Type
        {
            [DebuggerStepThrough]
            get
            {
                int balls = gettype(instance);
                if (balls < 0 || balls >= ValueTypeHelper._typearray.Length)
                {
                    return TypeEnum.TypeInvalid;
                }
                return ValueTypeHelper._typearray[balls];
            }
            [DebuggerStepThrough]
            set
            {
                SegFault();
                settype(instance, (int) value);
            }
        }

        public bool Valid
        {
            [DebuggerStepThrough]
            get
            {
                SegFault();
                return valid(__instance);
            }
        }

        public int Size
        {
            [DebuggerStepThrough]
            get
            {
                SegFault();
                if (!Valid || Type == TypeEnum.TypeInvalid || Type == TypeEnum.TypeIDFK)
                {
                    Clear();
                    return 0;
                }
                if (Type != TypeEnum.TypeString && Type != TypeEnum.TypeStruct && Type != TypeEnum.TypeArray)
                    return 0;
                return getsize(instance);
            }
            [DebuggerStepThrough]
            set
            {
                SegFault();
                setsize(instance, value);
            }
        }

        [DebuggerStepThrough]
        public void Set<T>(T t)
        {
            if ("" is T)
            {
                set(instance, (string) (object) t);
            }
            else if (0 is T)
            {
                set(instance, (int) (object) t);
            }
            else if (this is T)
            {
                set(instance, ((XmlRpcValue) (object) t).instance);
            }
            else if (true is T)
            {
                set(instance, (bool) (object) t);
            }
            else if (0d is T)
            {
                set(instance, (double) (object) t);
            }
        }

        [DebuggerStepThrough]
        public void Set<T>(int key, T t)
        {
            this[key].Set(t);
        }

        [DebuggerStepThrough]
        public void Set<T>(string key, T t)
        {
            this[key].Set(t);
        }

        [DebuggerStepThrough]
        public T Get<T>() // where T : class, new()
        {
            if (!Valid)
            {
                Console.WriteLine("Trying to get something with an invalid size... BAD JUJU!\n\t" + this);
            }
            else if ("" is T)
            {
                return (T) (object) GetString();
            }
            else if (0 is T)
            {
                return (T) (object) GetInt();
            }
            else if (this is T)
            {
                return (T) (object) this;
            }
            else if (true is T)
            {
                return (T) (object) GetBool();
            }
            else if (0d is T)
            {
                return (T) (object) GetDouble();
            }
            Console.WriteLine("I DUNNO WHAT THAT IS!");
            return default(T);
        }

        [DebuggerStepThrough]
        private T Get<T>(int key)
        {
            return this[key].Get<T>();
        }

        [DebuggerStepThrough]
        private T Get<T>(string key)
        {
            return this[key].Get<T>();
        }

        [DebuggerStepThrough]
        private XmlRpcValue Get(int key)
        {
            IntPtr nested = get(instance, key);
            return LookUp(nested);
        }

        [DebuggerStepThrough]
        private XmlRpcValue Get(string key)
        {
            IntPtr nested = get(instance, key);
            return LookUp(nested);
        }

        [DebuggerStepThrough]
        public int GetInt()
        {
            SegFault();
            return getint(__instance);
        }

        [DebuggerStepThrough]
        public string GetString()
        {
            SegFault();
            return Marshal.PtrToStringAnsi(getstring(__instance));
        }

        [DebuggerStepThrough]
        public bool GetBool()
        {
            SegFault();
            return getbool(__instance);
        }

        [DebuggerStepThrough]
        public double GetDouble()
        {
            SegFault();
            return getdouble(__instance);
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            if (__instance == IntPtr.Zero)
                return "(NULL)";
            string s = Marshal.PtrToStringAnsi(tostring(instance));
            return s;
        }

        [DebuggerStepThrough]
        public static XmlRpcValue Create(ref IntPtr existingvalue)
        {
            if (existingvalue == IntPtr.Zero)
            {
                Console.WriteLine("Well, that pointer was invalid, so here's a real one.");
                XmlRpcValue PSYCHE = new XmlRpcValue();
                existingvalue = PSYCHE.__instance;
                return PSYCHE;
            }
            return new XmlRpcValue(existingvalue);
        }

        [DebuggerStepThrough]
        public bool HasMember(string name)
        {
            SegFault();
            return hasmember(instance, name);
        }

        [DebuggerStepThrough]
        public void Dump()
        {
            SegFault();
            dump(__instance);
        }

        [DebuggerStepThrough]
        public void Clear()
        {
            SegFault();
            Clear(__instance);
        }

        [DebuggerStepThrough]
        public static bool Clear(IntPtr ptr)
        {
            RmRef(ref ptr);
            return (ptr == IntPtr.Zero);
        }

        [DebuggerStepThrough]
        public void SegFault()
        {
            if (__instance == IntPtr.Zero)
            {
                Console.WriteLine("IF YOU DEREFERENCE A NULL POINTER AGAIN I'LL PUNCH YOU IN THE ASS!");
            }
        }
    }

    [DebuggerStepThrough]
    public static class ValueTypeHelper
    {
        public static TypeEnum[] _typearray =
        {
            TypeEnum.TypeInvalid,
            TypeEnum.TypeBoolean,
            TypeEnum.TypeInt,
            TypeEnum.TypeDouble,
            TypeEnum.TypeString,
            TypeEnum.TypeDateTime,
            TypeEnum.TypeBase64,
            TypeEnum.TypeArray,
            TypeEnum.TypeStruct,
            TypeEnum.TypeIDFK
        };
    }

    public enum TypeEnum
    {
        TypeInvalid,
        TypeBoolean,
        TypeInt,
        TypeDouble,
        TypeString,
        TypeDateTime,
        TypeBase64,
        TypeArray,
        TypeStruct,
        TypeIDFK
    }
}
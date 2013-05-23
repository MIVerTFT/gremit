﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using GrEmit.Utils;

using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class HackHelpersTest
    {
        [Test]
        public void TestCall()
        {
            CForCallTest.callStatic = 0;
            var c = new CForCallTest();
            HackHelpers.CallMethod(c, test => test.CallInstance(-1), null, new object[] {2});
            Assert.AreEqual(2, c.callInstance);
            Assert.AreEqual(0, CForCallTest.callStatic);
        }

        [Test]
        public void TestCallWithRet()
        {
            CForCallTest.callStatic = 0;
            var c = new CForCallTest();
            Assert.AreEqual(3, HackHelpers.CallMethod(c, test => test.CallInstanceRet(-1), null, new object[] {3}));
            Assert.AreEqual(3, c.callInstance);
            Assert.AreEqual(0, CForCallTest.callStatic);
        }

        [Test]
        public void TestCallStatic()
        {
            CForCallTest.callStatic = 0;
            var c = new CForCallTest();
            HackHelpers.CallStaticMethod(() => CForCallTest.CallStatic(-1), null, new object[] {2});
            Assert.AreEqual(0, c.callInstance);
            Assert.AreEqual(2, CForCallTest.callStatic);

            Assert.AreEqual(20,
                            (int)
                            HackHelpers.CallStaticMethod(() => CForCallTest.CallStaticGeneric(""), new[] {typeof(int)},
                                                         new object[] {20}));
            Assert.AreEqual(22,
                            (int)
                            HackHelpers.CallStaticMethod(() => CForCallTest.CallStaticGeneric2<string>(-1), new[] {typeof(int)},
                                                         new object[] {21}));
            Assert.AreEqual(null,
                            HackHelpers.CallStaticMethod(() => CForCallTest.EmitInjectServices<int>(null), new[] {typeof(HackHelpersTest)},
                                                         new object[] {null}));
        }

        [Test]
        public void TestConstructGenericMethodDefinitionForGenericClass()
        {
            MethodInfo constructedMethod =
                HackHelpers.ConstructGenericMethodDefinitionForGenericClass<CGeneric<int>>(x => x.Method<long>(),
                                                                                           new[] {typeof(string)},
                                                                                           new[] {typeof(object)});
            Assert.AreEqual(HackHelpers.GetMethodDefinition<CGeneric<string>>(generic => generic.Method<object>()),
                            constructedMethod);

            Assert.AreNotEqual(HackHelpers.GetMethodDefinition<CGeneric<int>>(generic => generic.Method<object>()),
                               constructedMethod);
            Assert.AreNotEqual(HackHelpers.GetMethodDefinition<CGeneric<int>>(generic => generic.Method<long>()),
                               constructedMethod);
            Assert.AreNotEqual(HackHelpers.GetMethodDefinition<CGeneric<string>>(generic => generic.Method<long>()),
                               constructedMethod);
        }

        [Test]
        public void TestConstructMethodDefinition()
        {
            Assert.AreEqual(GetMethod(typeof(ClassWithMethods), "Method", new[] {typeof(long)}),
                            HackHelpers.ConstructMethodDefinition<ClassWithMethods>(o => o.Method<int>(),
                                                                                    new[] {typeof(long)}));
            Assert.AreEqual(GetMethod(typeof(ClassWithMethods), "NonGenericMeth", null),
                            HackHelpers.ConstructMethodDefinition<ClassWithMethods>(o => o.NonGenericMeth(), null));
            Assert.AreEqual(
                GetMethod(typeof(ClassWithMethods), "Method2", new[] {typeof(long), typeof(object)}),
                HackHelpers.ConstructMethodDefinition<ClassWithMethods>(o => o.Method2<int, string>(default(string)),
                                                                        new[] {typeof(long), typeof(object)}));

            Assert.AreEqual(GetMethod(typeof(CForCallTest), "CallStatic", null),
                            HackHelpers.ConstructStaticMethodDefinition(() => CForCallTest.CallStatic(11), null));
        }

        [Test]
        public void TestGetGenericMethodDefinitionForGenericClass2()
        {
            MethodInfo constructedMethod =
                HackHelpers.ConstructGenericMethodDefinitionForGenericClass<CGeneric<int>>(x => x.Method<long>(1),
                                                                                           new[] {typeof(string)},
                                                                                           new[] {typeof(object)});
            Assert.AreEqual(HackHelpers.GetMethodDefinition<CGeneric<string>>(generic => generic.Method<object>(1)),
                            constructedMethod);
        }

        [Test]
        public void TestGetGenericMethodDefinitionForGenericClass3()
        {
            MethodInfo constructedMethod =
                HackHelpers.ConstructGenericMethodDefinitionForGenericClass<CGeneric<int>>(x => x.Method(1L, "b", 2),
                                                                                           new[] {typeof(object)},
                                                                                           new[] {typeof(A), typeof(B)});
            Assert.AreEqual(
                HackHelpers.GetMethodDefinition<CGeneric<object>>(
                    generic => generic.Method(default(A), default(B), default(object))),
                constructedMethod);
        }

        [Test]
        public void TestGetGenericMethodDefinitionForGenericClassWorksForNonGenerics()
        {
            MethodInfo constructedMethod =
                HackHelpers.ConstructGenericMethodDefinitionForGenericClass<ClassWithMethods>(x => x.NonGenericMeth(),
                                                                                              null, null);
            Assert.AreEqual(HackHelpers.GetMethodDefinition<ClassWithMethods>(generic => generic.NonGenericMeth()),
                            constructedMethod);
        }

        [Test]
        public void TestGetMethodDefinition()
        {
            Assert.AreEqual(GetMethod(typeof(Type), "op_Equality", null),
                            HackHelpers.GetMethodDefinition<Type>(type => type == typeof(int)));

            Assert.AreEqual(GetMethod(typeof(ClassWithMethods), "NonGenericMeth", null),
                            HackHelpers.GetMethodDefinition<ClassWithMethods>(o => o.NonGenericMeth()));
            Assert.AreEqual(GetMethod(typeof(ClassWithMethods), "op_Addition", null),
                            HackHelpers.GetMethodDefinition<ClassWithMethods>(
                                o => o + default(ClassWithMethods)));
        }

        [Test]
        public void TestGetObjectConstruction()
        {
            Assert.AreEqual(typeof(HackHelpersTest).GetConstructor(Type.EmptyTypes),
                            HackHelpers.GetObjectConstruction(() => new HackHelpersTest()));
            Assert.AreEqual(typeof(string).GetConstructor(new[] {typeof(char), typeof(int)}),
                            HackHelpers.GetObjectConstruction(() => new string('x', 1)));
            try
            {
                HackHelpers.GetObjectConstruction(() => new CStruct());
                Assert.Fail("no crash");
            }
            catch(NotSupportedException e)
            {
                Assert.AreEqual("Struct creation without arguments", e.Message);
            }
            Assert.AreEqual(typeof(CStruct).GetConstructor(new[] {typeof(int)}),
                            HackHelpers.GetObjectConstruction(() => new CStruct(1)));

            Assert.AreEqual(typeof(CGeneric<long>).GetConstructor(new[] {typeof(long)}),
                            HackHelpers.GetObjectConstruction(() => new CGeneric<string>("zzz"), typeof(long)));
        }

        [Test]
        public void TestGetValueTypeForNullableOrNull()
        {
            Assert.IsNull(HackHelpers.GetValueTypeForNullableOrNull(typeof(int)));
            Assert.IsNull(HackHelpers.GetValueTypeForNullableOrNull(typeof(object)));
            Assert.IsNull(HackHelpers.GetValueTypeForNullableOrNull(typeof(Nullable<>)));
            Assert.IsNull(HackHelpers.GetValueTypeForNullableOrNull(typeof(int?).GetGenericArguments()[0]));
            Assert.IsNull(HackHelpers.GetValueTypeForNullableOrNull(typeof(Generic<>)));
            Assert.IsNull(HackHelpers.GetValueTypeForNullableOrNull(typeof(Generic<int>)));
            Assert.AreEqual(typeof(int), HackHelpers.GetValueTypeForNullableOrNull(typeof(int?)));
            // ReSharper disable ConvertNullableToShortForm
            Assert.AreEqual(typeof(long), HackHelpers.GetValueTypeForNullableOrNull(typeof(Nullable<long>)));
            // ReSharper restore ConvertNullableToShortForm
        }

        private static MethodInfo GetMethod(Type type, string name, Type[] genericArguments)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach(MethodInfo info in methods)
            {
                if(info.Name == name)
                {
                    if(genericArguments == null || genericArguments.Length == 0 && !info.IsGenericMethod)
                        return info;
                    if(info.IsGenericMethod)
                        return info.MakeGenericMethod(genericArguments);
                }
            }
            throw new MissingMethodException(type.Name, name);
        }

        private static FieldInfo GetField(Type type, string name)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach(FieldInfo info in fields)
            {
                if(info.Name == name)
                    return info;
            }
            throw new MissingMethodException(type.Name, name);
        }

        // ReSharper disable ClassNeverInstantiated.Local

        private class A
        {
        }

        private class B
        {
        }

        private class CGeneric<TC>
        {
            public CGeneric(TC c)
            {
            }

            public CGeneric(int a)
            {
            }

            public void Method<T>()
            {
            }

            public void Method<T>(int x)
            {
            }

            public void Method<T, T2>(T a, T2 b, TC c)
            {
            }

            public string Prop { get; set; }
            public int fInt;
            public TC fGen;
        }

        private class ClassWithMethods
        {
            // ReSharper disable MemberCanBeMadeStatic.Local
            public void NonGenericMeth()
            {
            }

            public void Method<T>()
            {
            }

            public int Method2<T, TT>(TT val)
            {
                return 0;
            }

            public static ClassWithMethods operator +(ClassWithMethods a, ClassWithMethods b)
            {
                return null;
            }

            // ReSharper restore MemberCanBeMadeStatic.Local
        }

        private class Generic<T>
        {
        }

        private class CForCallTest
        {
            public static TZ CallStaticGeneric<TZ>(TZ z)
            {
                return z;
            }

            public static int CallStaticGeneric2<TZ>(int a)
            {
                return a + 1;
            }

            public static Action<IContainer, T> EmitInjectServices<T>(IEnumerable<FieldInfo> serviceTargets)
            {
                return null;
            }

            public static void CallStatic(int a)
            {
                callStatic += a;
            }

            public void CallInstance(int a)
            {
                callInstance += a;
            }

            public int CallInstanceRet(int a)
            {
                callInstance += a;
                return a;
            }

            public static int callStatic;
            public int callInstance;
        }

        private struct CStruct
        {
            public CStruct(int x)
            {
            }
        }
    }
}
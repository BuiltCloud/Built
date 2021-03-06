﻿using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Built.Grpc.HttpGateway
{
    /*
     * 1,网关启动时，加载protos文件下所有proto文件并且对比MD5是否发生变化
     * 2,根据proto文件生成对应的cache文件夹，里面存放对应的dll文件以及proto的MD5值，

         */

    public static class GrpcServiceMethodFactory
    {
        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string PluginPath = Path.Combine(BaseDirectory, "plugins");
        public static readonly string ProtoPath = Path.Combine(BaseDirectory, "protos");
        public static ConcurrentDictionary<string, GrpcMethodHandlerInfo> Handers = new ConcurrentDictionary<string, GrpcMethodHandlerInfo>();

        // dll文件队列;
        public static ProducerConsumer<string> DllQueue = new ProducerConsumer<string>(fileName => LoadAsync(fileName).Wait());

        // proto文件队列;
        public static ProducerConsumer<string> ProtoQueue = new ProducerConsumer<string>(protoFileName =>
        {
            InnerLogger.Log(LoggerLevel.Debug, "出队:" + protoFileName);
            try
            {
                if (CodeGenerate.Generate(BaseDirectory, protoFileName))
                {
                    InnerLogger.Log(LoggerLevel.Debug, "生成成功:" + protoFileName);
                    var name = Path.GetFileNameWithoutExtension(protoFileName);
                    var csharp_out = Path.Combine(BaseDirectory, $"plugins/.{name}");
                    if (CodeBuild.Build(csharp_out, name))
                    {
                        InnerLogger.Log(LoggerLevel.Debug, "Build成功:" + protoFileName);
                        var dllPath = Path.Combine(csharp_out, $"{name}.dll");
                        var xmlDocPath = Path.Combine(csharp_out, $"{name}.xml");
                        //生成plugin.yml
                        var serializer = new SerializerBuilder().Build();
                        var yaml = serializer.Serialize(new ProtoPluginModel
                        {
                            DllFileMD5 = dllPath.GetMD5(),
                            FileName = name,
                            ProtoFileMD5 = protoFileName.GetMD5(),
                            XmlFileMD5 = xmlDocPath.GetMD5()
                        });
                        File.WriteAllText(Path.Combine(csharp_out, "plugin.yml"), yaml);
                        DllQueue.Enqueue(dllPath);
                    }
                    else
                    {
                        InnerLogger.Log(LoggerLevel.Debug, "Build失败:" + protoFileName);
                    }
                }
                else
                {
                    InnerLogger.Log(LoggerLevel.Debug, "生成失败:" + protoFileName);
                }
            }
            catch (Exception er)
            {
                InnerLogger.Log(LoggerLevel.Debug, "出队:" + er.StackTrace);
            }
        });

        public static async Task ReLoadAsync()
        {
            await InitAsync();
        }

        public static Task InitAsync()
        {
            if (!Directory.Exists(PluginPath)) Directory.CreateDirectory(PluginPath);
            if (!Directory.Exists(ProtoPath)) Directory.CreateDirectory(ProtoPath);
            Handers = new ConcurrentDictionary<string, GrpcMethodHandlerInfo>();
            return Task.Factory.StartNew(() =>
            {
                var dllFiles = Directory.GetFiles(PluginPath, "*.dll");
                foreach (var file in dllFiles)
                {
                    DllQueue.Enqueue(file);
                }

                var protoFiles = Directory.GetFiles(ProtoPath, "*.proto");
                foreach (var file in protoFiles)
                {
                    var NeedGenerate = true;
                    var GenerateDllPath = string.Empty;
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var csharp_out = Path.Combine(BaseDirectory, $"plugins/.{fileName}");
                    if (Directory.Exists(csharp_out))
                    {
                        var pluginYml = Path.Combine(csharp_out, $"plugin.yml");
                        GenerateDllPath = Path.Combine(csharp_out, $"{fileName}.dll");
                        var xmlDocPath = Path.Combine(csharp_out, $"{fileName}.xml");
                        if (File.Exists(pluginYml) && File.Exists(GenerateDllPath) && File.Exists(xmlDocPath))
                        {
                            var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(new CamelCaseNamingConvention())
                            .Build();
                            var setting = new ProtoPluginModel();
                            using (FileStream fs = new FileStream(pluginYml, FileMode.Open, FileAccess.Read))
                            {
                                var dic = (Dictionary<object, object>)deserializer.Deserialize(new StreamReader(fs, Encoding.Default));

                                dic.TryGetValue("FileName", out object fName);
                                setting.FileName = fName?.ToString();

                                dic.TryGetValue("DllFileMD5", out object dName);
                                setting.DllFileMD5 = dName?.ToString();

                                dic.TryGetValue("ProtoFileMD5", out object pName);
                                setting.ProtoFileMD5 = pName?.ToString();

                                dic.TryGetValue("XmlFileMD5", out object xName);
                                setting.XmlFileMD5 = xName?.ToString();
                                //var setting = deserializer.Deserialize<ProtoPluginModel>(File.ReadAllText(pluginYml));
                            }
                            var protoMD5 = file.GetMD5();
                            var dllMD5 = GenerateDllPath.GetMD5();
                            var xmlMD5 = xmlDocPath.GetMD5();
                            if (setting.ProtoFileMD5 == protoMD5 && setting.DllFileMD5 == dllMD5 && setting.XmlFileMD5 == xmlMD5)
                            {
                                NeedGenerate = false;
                            }
                        }
                    }
                    if (NeedGenerate)
                    {
                        ProtoQueue.Enqueue(file);
                    }
                    else
                    {
                        DllQueue.Enqueue(GenerateDllPath);
                    }
                }
            });
        }

        public static Task LoadAsync(string fileFullPath)
        {
            return Task.Run(() =>
            {
                byte[] assemblyBuf = File.ReadAllBytes(fileFullPath);
                var assembly = Assembly.Load(assemblyBuf);
                HandersAddOrUpdate(assembly);
            });
        }

        private static void HandersAddOrUpdate(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.Name.EndsWith("Base"))
                {
                    if (type.ReflectedType == null) continue;
                    // 获取__ServiceName
                    FieldInfo f_key = type.ReflectedType.GetField("__ServiceName", BindingFlags.Static | BindingFlags.NonPublic);
                    if (f_key == null) continue;
                    var ServiceName = f_key.GetValue(type.ReflectedType);
                    GetGrpcMethods(ServiceName.ToString(), type);
                }
            }
            // 通过 ProductBasicReflection.Descriptor 获取方法列表也是可以的
        }

        public static void GetGrpcMethods(string serviceName, Type serviceType)
        {
            GetGrpcMethods(serviceName, serviceType, GrpcMarshallerFactory.DefaultInstance);
        }

        public static void GetGrpcMethods(string serviceName, Type serviceType, IGrpcMarshallerFactory marshallerFactory)
        {
            foreach (GrpcMethodHandlerInfo handler in GrpcReflection.EnumerateServiceMethods(serviceName, serviceType, marshallerFactory))
            {
                Handers.AddOrUpdate(handler.GetHashString(), handler);
                InnerLogger.Log(LoggerLevel.Debug, handler.GetHashString());
            }
        }
    }
}
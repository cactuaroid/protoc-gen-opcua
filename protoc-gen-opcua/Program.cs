﻿using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace ProtocGenOpcua
{
    // command line for compile
    // protoc.exe --plugin=protoc-gen-opcua.exe --opcua_out=./ --proto_path=%userprofile%\.nuget\packages\google.protobuf.tools\3.21.1\tools --proto_path=./ my_service.proto
    // 
    // - current directory is the output directory
    // - current directory contains protoc.exe

    internal class Program
    {
        static void Main(string[] args)
        {
            // you can launch debugger
            // System.Diagnostics.Debugger.Launch();

            // get request from standard input
            CodeGeneratorRequest request;
            using (var stdin = Console.OpenStandardInput())
            {
                request = Deserialize<CodeGeneratorRequest>(stdin);
            }

            var response = new CodeGeneratorResponse();

            foreach (var file in request.FileToGenerate)
            {
                var allProtos = request.ProtoFile.Where((x) => !x.Name.Contains("google/protobuf")); // ignore imported well-known types
                var mainProto = allProtos.First((x) => x.Name == file);
                var packageName = mainProto.Package;

                var builder = new StringBuilder();

                builder.AppendLine(
                    $@"<?xml version=""1.0"" encoding=""utf-8""?>
                    <!-- THIS FILE IS GENERATED BY protoc-gen-opcua -->
                    <opc:ModelDesign
                        xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
	                    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
	                    xmlns:opc=""http://opcfoundation.org/UA/ModelDesign.xsd""
                        xmlns:ua=""http://opcfoundation.org/UA/""
                        xmlns:uax=""http://opcfoundation.org/UA/2008/02/Types.xsd""
	                    xmlns=""{packageName}""
                        TargetNamespace=""{packageName}"">

                        <opc:Namespaces>
                            <opc:Namespace Name=""OpcUa"" Prefix=""Opc.Ua"" XmlNamespace=""http://opcfoundation.org/UA/2008/02/Types.xsd"">http://opcfoundation.org/UA/</opc:Namespace>
                            <opc:Namespace Name=""{packageName}"" Prefix=""{packageName}"">{packageName}</opc:Namespace>
                        </opc:Namespaces>");

                {

                    builder.AppendLine("<!-- services -->");

                    foreach (var service in mainProto.Service)
                    {
                        var serviceName = service.Name;
                        var serviceType = serviceName + "ObjectType";

                        builder.AppendLine(
                        $@"<opc:Object SymbolicName=""{serviceName}"" TypeDefinition=""{serviceType}"">
                            <opc:References>
                                <opc:Reference IsInverse=""true"">
                                    <opc:ReferenceType>ua:Organizes</opc:ReferenceType>
                                    <opc:TargetId>ua:ObjectsFolder</opc:TargetId>
                                </opc:Reference>
                            </opc:References>
                        </opc:Object>");

                        builder.AppendLine(
                            $@"<opc:ObjectType SymbolicName=""{serviceType}"" BaseType=""ua:BaseObjectType"">
                                <opc:Children>");
                        {
                            foreach (var method in service.Method)
                            {
                                var methodName = method.Name;
                                var methodType = methodName + "Method";

                                builder.AppendLine(
                                    $@"<opc:Method SymbolicName=""{methodName}"" TypeDefinition=""{methodType}"" />");
                            }
                        }
                        builder.AppendLine(
                                @"</opc:Children>
                            </opc:ObjectType>");

                        foreach (var method in service.Method)
                        {
                            var methodName = method.Name;
                            var methodType = methodName + "Method";

                            builder.AppendLine(
                                $@"<opc:Method SymbolicName=""{methodType}"">");
                            {
                                builder.AppendLine(@"<opc:InputArguments>");
                                {
                                    // number of argument is 0 or 1.
                                    if (method.HasInputType && method.InputType != ".google.protobuf.Empty")
                                    {
                                        var argumentName = GetTypeShortName(method.InputType);
                                        var argumentDataType = ToDataTypeName(method.InputType);

                                        builder.AppendLine(
                                            $@"<opc:Argument Name=""{argumentName}"" DataType=""{argumentDataType}"" /> ");
                                    }
                                }
                                builder.AppendLine(@"</opc:InputArguments>");
                            }

                            {
                                builder.AppendLine(@"<opc:OutputArguments>");
                                {
                                    // number of argument is 0 or 1.
                                    if (method.HasOutputType && method.OutputType != ".google.protobuf.Empty")
                                    {
                                        var argumentName = GetTypeShortName(method.OutputType);
                                        var argumentDataType = ToDataTypeName(method.OutputType);

                                        builder.AppendLine(
                                            $@"<opc:Argument Name=""{argumentName}"" DataType=""{argumentDataType}"" /> ");
                                    }
                                }
                                builder.AppendLine(@"</opc:OutputArguments>");
                            }
                            builder.AppendLine(@"</opc:Method>");
                        }
                    }

                    builder.AppendLine("<!-- messages -->");

                    // OPC UA does not support nested data type definition.
                    // All nested message/enum definition are getting flattened. Unique naming required.
                    foreach (var message in GetAllMessageTypes(allProtos))
                    {
                        var messageType = message.Name + "DataType";

                        builder.AppendLine(
                            $@"<opc:DataType SymbolicName=""{messageType}"" BaseType=""ua:Structure"" >
                                <opc:Fields>");
                        {
                            foreach (var field in message.Field)
                            {
                                // 'oneof' is ignored and all fields simply being listed.
                                builder.AppendLine($@"<opc:Field");
                                {
                                    builder.AppendLine($@"Name=""{SnakeToPascal(field.Name)}""");
                                    builder.AppendLine($@"DataType=""{ToDataTypeName(GetFieldTypeName(field))}""");
                                    if (field.Label.HasFlag(FieldDescriptorProto.Types.Label.Repeated))
                                    {
                                        builder.AppendLine(@"ValueRank=""Array""");
                                    }
                                }
                                builder.AppendLine(" />");
                            }
                        }
                        builder.AppendLine(
                                $@"</opc:Fields>
                            </opc:DataType>");
                    }

                    builder.AppendLine("<!-- enums -->");

                    foreach (var @enum in GetAllEnumTypes(allProtos))
                    {
                        var enumType = @enum.Name + "DataType";

                        builder.AppendLine(
                            $@"<opc:DataType SymbolicName=""{enumType}"" BaseType=""ua:Enumeration"" >
                                <opc:Fields>");
                        {
                            foreach (var value in @enum.Value)
                            {
                                builder.AppendLine($@"<opc:Field");
                                {
                                    builder.AppendLine($@"Name=""{SnakeToPascal(value.Name)}""");
                                    builder.AppendLine($@"Identifier=""{value.Number}""");
                                }
                                builder.AppendLine(" />");
                            }
                        }
                        builder.AppendLine(
                                $@"</opc:Fields>
                            </opc:DataType>");
                    }
                }

                builder.AppendLine(@"</opc:ModelDesign>");

                // set as response
                var xdoc = XDocument.Parse(builder.ToString());
                response.File.Add(
                    new CodeGeneratorResponse.Types.File()
                    {
                        Name = file + ".xml",
                        Content = xdoc.Declaration.ToString() + "\r\n" + xdoc.ToString(),
                    }
                );
            }

            // set result to standard output
            using (var stdout = Console.OpenStandardOutput())
            {
                response.WriteTo(stdout);
            }
        }

        static Dictionary<string, string> s_uaTypes = new Dictionary<string, string>
        {
            // https://reference.opcfoundation.org/v105/Core/docs/Part3/#8

            // https://developers.google.com/protocol-buffers/docs/proto3#scalar
            { "Double", "ua:Double" },
            { "Float", "ua:Float" },
            { "Int32", "ua:Int32" },
            { "Int64", "ua:Int64" },
            { "Uint32", "ua:Uint32" },
            { "Uint64", "ua:Uint64" },
            { "String", "ua:String" },
            { "Bool", "ua:Boolean" },
            { "Sint32", "ua:Int32" },
            { "Sint64", "ua:Int64" },
            { "Fixed32", "ua:UInt32" },
            { "Fixed64", "ua:Uint64" },
            { "Sfixed32", "ua:Int32" },
            { "Sfixed64", "ua:Int64" },
            { "Bytes", "ua:ByteString" },

            // https://developers.google.com/protocol-buffers/docs/reference/google.protobuf
            { ".google.protobuf.Any", "ua:ByteString" },
            { ".google.protobuf.BoolValue", "ua:Boolean" },
            { ".google.protobuf.BytesValue", "ua:ByteString" },
            { ".google.protobuf.DoubleValue", "ua:Double" },
            { ".google.protobuf.Duration", "ua:Duration" },
            { ".google.protobuf.FloatValue", "ua:Float" },
            { ".google.protobuf.Int32Value", "ua:Int32" },
            { ".google.protobuf.Int64Value", "ua:Int64" },
            { ".google.protobuf.StringValue", "ua:String" },
            { ".google.protobuf.Timestamp", "ua:DateTime" },
            { ".google.protobuf.Uint32", "ua:Uint32" },
            { ".google.protobuf.Uint64", "ua:Uint64" },
        };

        static T Deserialize<T>(Stream stream) where T : IMessage<T>, new()
            => new MessageParser<T>(() => new T()).ParseFrom(stream);

        static string GetTypeShortName(string typeName)
            => typeName.Split(".").Last();

        static string GetFieldTypeName(FieldDescriptorProto field)
            => (field.HasTypeName) ? field.TypeName : field.Type.ToString();

        static string ToDataTypeName(string protoType)
        => s_uaTypes.TryGetValue(protoType, out var uaType) ? uaType : GetTypeShortName(protoType) + "DataType";

        static string SnakeToPascal(string source)
            => new CultureInfo("en-US").TextInfo.ToTitleCase(source.ToLowerInvariant()).Replace("_", "");

        static IEnumerable<DescriptorProto> GetAllMessageTypes(IEnumerable<FileDescriptorProto> protos)
            => protos.SelectMany((x) => GetAllMessageTypes(x.MessageType));

        static IEnumerable<DescriptorProto> GetAllMessageTypes(IEnumerable<DescriptorProto> messages)
            => messages.Concat(messages.SelectMany((x) => GetAllMessageTypes(x.NestedType)));

        static IEnumerable<EnumDescriptorProto> GetAllEnumTypes(IEnumerable<FileDescriptorProto> protos)
            => GetAllEnumTypes(protos.SelectMany((x) => x.EnumType), protos.SelectMany((x) => x.MessageType));

        static IEnumerable<EnumDescriptorProto> GetAllEnumTypes(IEnumerable<EnumDescriptorProto> enums, IEnumerable<DescriptorProto> messages)
            => enums.Concat(GetAllMessageTypes(messages).SelectMany((x) => x.EnumType));
    }
}
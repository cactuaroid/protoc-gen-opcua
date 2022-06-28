# Overview
protoc plugin for generating OPC UA design xml for [UA-ModelCompiler](https://github.com/OPCFoundation/UA-ModelCompiler/) from gRPC .proto.

- `stream` is ignored.
- `oneof` is ignored and all fields simply being listed.
- All nested messages/enums will be flattened. Unique naming required.

### Input
##### my_service.proto[^1]
[^1]: https://github.com/cactuaroid/protoc-gen-opcua/blob/main/protoc-gen-opcua/my_service.proto
```proto
service MyService {
  rpc Write(MyMessage) returns (google.protobuf.Empty) {}
  rpc Read(google.protobuf.Empty) returns (MyMessage) {}
}

message MyMessage {
	string message_tag = 1;
	NestedContent message_content = 2;
	google.protobuf.Timestamp registered_time = 3;

	message NestedContent {
		NestedContent2 content2 = 1;
		NestedEnum nested_enum = 2;
		MyEnum my_enum = 3; // defined in my_enum.proto

		message NestedContent2 {
			repeated string repeated_field = 1;
			oneof oneof_value {
				int32 oneof_value1 = 2;
				int32 oneof_value2 = 3;
			}
		}

		enum NestedEnum {
			FIRST = 0;
			SECOND = 1;
		}
	}
}
```

##### my_enum.proto[^2]
[^2]: https://github.com/cactuaroid/protoc-gen-opcua/blob/main/protoc-gen-opcua/my_enum.proto
```proto
enum MyEnum {
	FIRST = 0;
	SECOND = 1;
}
```

### Command Line
`
protoc.exe --plugin=protoc-gen-opcua.exe --opcua_out=./ --proto_path=%userprofile%\.nuget\packages\google.protobuf.tools\3.21.1\tools --proto_path=./ my_service.proto
`

### Output my_service.proto.xml
```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- THIS FILE IS GENERATED BY protoc-gen-opcua -->
<opc:ModelDesign xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:opc="http://opcfoundation.org/UA/ModelDesign.xsd" xmlns:ua="http://opcfoundation.org/UA/" xmlns:uax="http://opcfoundation.org/UA/2008/02/Types.xsd" xmlns="ProtocGenOpcua" TargetNamespace="ProtocGenOpcua">
  <opc:Namespaces>
    <opc:Namespace Name="OpcUa" Prefix="Opc.Ua" XmlNamespace="http://opcfoundation.org/UA/2008/02/Types.xsd">http://opcfoundation.org/UA/</opc:Namespace>
    <opc:Namespace Name="ProtocGenOpcua" Prefix="ProtocGenOpcua">ProtocGenOpcua</opc:Namespace>
  </opc:Namespaces>
  <!-- services -->
  <opc:Object SymbolicName="MyService" TypeDefinition="MyServiceObjectType">
    <opc:References>
      <opc:Reference IsInverse="true">
        <opc:ReferenceType>ua:Organizes</opc:ReferenceType>
        <opc:TargetId>ua:ObjectsFolder</opc:TargetId>
      </opc:Reference>
    </opc:References>
  </opc:Object>
  <opc:ObjectType SymbolicName="MyServiceObjectType" BaseType="ua:BaseObjectType">
    <opc:Children>
      <opc:Method SymbolicName="Write" TypeDefinition="WriteMethod" />
      <opc:Method SymbolicName="Read" TypeDefinition="ReadMethod" />
    </opc:Children>
  </opc:ObjectType>
  <opc:Method SymbolicName="WriteMethod">
    <opc:InputArguments>
      <opc:Argument Name="MyMessage" DataType="MyMessageDataType" />
    </opc:InputArguments>
    <opc:OutputArguments></opc:OutputArguments>
  </opc:Method>
  <opc:Method SymbolicName="ReadMethod">
    <opc:InputArguments></opc:InputArguments>
    <opc:OutputArguments>
      <opc:Argument Name="MyMessage" DataType="MyMessageDataType" />
    </opc:OutputArguments>
  </opc:Method>
  <!-- messages -->
  <opc:DataType SymbolicName="MyMessageDataType" BaseType="ua:Structure">
    <opc:Fields>
      <opc:Field Name="MessageTag" DataType="ua:String" />
      <opc:Field Name="MessageContent" DataType="NestedContentDataType" />
      <opc:Field Name="RegisteredTime" DataType="ua:DateTime" />
    </opc:Fields>
  </opc:DataType>
  <opc:DataType SymbolicName="NestedContentDataType" BaseType="ua:Structure">
    <opc:Fields>
      <opc:Field Name="Content2" DataType="NestedContent2DataType" />
      <opc:Field Name="NestedEnum" DataType="NestedEnumDataType" />
      <opc:Field Name="MyEnum" DataType="MyEnumDataType" />
    </opc:Fields>
  </opc:DataType>
  <opc:DataType SymbolicName="NestedContent2DataType" BaseType="ua:Structure">
    <opc:Fields>
      <opc:Field Name="RepeatedField" DataType="ua:String" ValueRank="Array" />
      <opc:Field Name="OneofValue1" DataType="ua:Int32" />
      <opc:Field Name="OneofValue2" DataType="ua:Int32" />
    </opc:Fields>
  </opc:DataType>
  <!-- enums -->
  <opc:DataType SymbolicName="MyEnumDataType" BaseType="ua:Enumeration">
    <opc:Fields>
      <opc:Field Name="FIRST" Identifier="0" />
      <opc:Field Name="SECOND" Identifier="1" />
    </opc:Fields>
  </opc:DataType>
  <opc:DataType SymbolicName="NestedEnumDataType" BaseType="ua:Enumeration">
    <opc:Fields>
      <opc:Field Name="FIRST" Identifier="0" />
      <opc:Field Name="SECOND" Identifier="1" />
    </opc:Fields>
  </opc:DataType>
</opc:ModelDesign>
```

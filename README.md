# Overview
protoc-gen-opcua is the protoc plugin for generating OPC UA design xml for [UA-ModelCompiler](https://github.com/OPCFoundation/UA-ModelCompiler/) from gRPC .proto.

- `stream` is ignored.
- `oneof` is ignored and all fields simply being listed.
- All nested messages/enums will be flattened. Unique naming required.

##### Input
- [my_service.proto](https://github.com/cactuaroid/protoc-gen-opcua/blob/main/protoc-gen-opcua/my_service.proto)
- [my_enum.proto](https://github.com/cactuaroid/protoc-gen-opcua/blob/main/protoc-gen-opcua/my_enum.proto)

##### Command Line
`
protoc.exe --plugin=protoc-gen-opcua.exe --opcua_out=./ --proto_path=%userprofile%\.nuget\packages\google.protobuf.tools\3.21.1\tools --proto_path=./ my_service.proto
`

##### Output
[my_service.proto.xml](https://github.com/cactuaroid/protoc-gen-opcua/blob/main/protoc-gen-opcua/expected/my_service.proto.xml)

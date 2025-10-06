curPath=$(pwd)
projectPath=$1
cd "$projectPath"
cd ..
rootPath=$(pwd)
outputPath="$rootPath$2"

cd Common/Protocol
protoc -I=. --csharp_out="$outputPath" Protocol.proto Enum.proto Struct.proto

cd "$rootPath/Tools/PacketGenerator/bin"
dotnet PacketGenerator.dll -o "$2" -t "$3"

cd "$curPath"
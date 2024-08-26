The pp-client.cs is generated from a file named client.proto located in the Portfolio Performance GitHub repository https://github.com/portfolio-performance/portfolio/blob/299ecbceb671e0fd55e065c1684af5fa5019451e/name.abuchen.portfolio/src/name/abuchen/portfolio/model/client.proto
Portfolio Performance is licensed under the Eclipse Public License, see https://github.com/portfolio-performance/portfolio/blob/master/LICENSE

How to generate C# classes from the Protobuf definition of Portfolio Performance?
- Download client.proto from the Portfolio Performance GitHub repository to this directory and rename it to pp-client.proto
- Add the following line under the "option java_..." lines in pp-client.proto: option csharp_namespace = "Toqe.PortfolioReader.Business.Protobuf";
- Run: dotnet tool install --global protobuf-net.Protogen --version 3.2.42
- Run: protogen --proto_path=. --csharp_out=. pp-client.proto

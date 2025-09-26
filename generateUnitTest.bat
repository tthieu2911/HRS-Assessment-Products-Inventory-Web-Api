rmdir /q /s "D:\test-results\"
dotnet test --collect:"XPlat Code Coverage" --results-directory "D:\test-results"
reportgenerator -reports:"D:\test-results\*\coverage.cobertura.xml" -targetdir:"D:\test-results\coveragereport" -reporttypes:Html
Pause
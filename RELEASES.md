# Release Process

1. Update the version in `src/APIAlerts/APIAlerts.csproj`
2. Update the version in `src/APIAlerts/Constants.kt`
3. PR to `main` branch, ensure tests pass then merge
4. Create a new release on GitHub on `main`
5. GitHub Actions will publish to Nuget

# AppSettingsReload

### This package removes the need for a dependency on IOptions<T> and also does easy reloading of appsetings when changed.

To use simply add AppSettingsReload to your project.json

``"AppSettingsReload": "0.x.x-*"```
Then add to where ever you injecting you appseting in DI
```using AppSettingsReload```

```

services.ConfigureSetting<TestConfig>(Configuration, "TestConfig", true);

```
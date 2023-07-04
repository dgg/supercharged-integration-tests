# Supercharged Integration Tests

Supporting code for [Danfoss Digital Nation 2023](https://danfossdigitalnation.com/) talk on Integration Tests using Docker.

[Slides](https://dgg.github.io/supercharged-integration-tests/)

There are multiple "main" branches for the supporting code, one per platform:

 * [Node.js](https://nodejs.org/en/about)
 * [.NET](https://dotnet.microsoft.com/en-us/)


## Running the Tests

```shell
$ dotnet test
```

If you are running tests on Apple Silicon, [Ryuk](https://github.com/testcontainers/moby-ryuk) resource reaping may not work unless disabled.<br/>
To run the tests with it disabled:
```shell
$ dotnet test -e TESTCONTAINERS_RYUK_DISABLED=true
```

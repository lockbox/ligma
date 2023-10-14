# ligma

## Run Server

in one terminal:
```bash
$ spacetime start
...
```
in other terminal:
```bash
$ cd ligma-server
$ spacetime server set-default http://127.0.0.1:3000
```


## Other commands

### Publish module to spacetime
(from within `ligma-server` -- after `spacetime start` is up and running)
```bash
ligma-server $ spacetime publish -c ligma-module
```

### Generate bindings
(from within `ligma-server`)
```bash
ligma-server $ spacetime generate --out-dir ../Client/Asserts/module_bindings --lang=csharp
```
**Note: Does not currently function as an API. The application is still being built.**

This is mainly an exercise for me to improve my knowledge with databases. Some things may be wonky, but I will try to fix issues as I can. Once things are sorted, I'll work on getting a more complete version. For the foreseeable future, though, this won't be very useful.

# Instructions

There are different arguments you can run if you only wish to fetch certain parts of the data. The information is dumped into `data/mhgen.db`.

`dotnet run --items`

...would only fill the database with the items. Valid arguments are `--items`, `--monsters`, `--quests`, or `--all`. If you simply run `dotnet run` without params, it defaults to `--all`.

If you made edits, use `dotnet test` to run the test cases.

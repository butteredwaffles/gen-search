**Note: Does not fully function as an API. The application is still being built.**

This is mainly an exercise for me to improve my knowledge with databases. Some things may be wonky, but I will try to fix issues as I can. 

# Instructions

There are different arguments you can run if you only wish to fetch certain parts of the data. The information is dumped into `data/mhgen.db`.

`dotnet run --items`

...would only fill the database with the items. If you simply run `dotnet run` without parameters, it defaults to `--all`.

Valid arguments are `--items, --monsters, --quests, --weapons, --skills, --arts, --decorations, --armor, --palico-skills, --palico-armor, --palico-weapons, and --all. `

If you made edits, use `dotnet test` to run the test cases.

## Converting the database

The program creates an SQLite database with the information. However, the web app uses a MySQL database. Therefore, if you are planning on running the service yourself, you will have to create a dump of the SQLite one.

Change the current directory to `app/utility` and run `sqlite3-translator.py` in order to convert the SQLite database dump (which you can get using a program such as SQLiteStudio) to a MySQL compatible file. Python3 is required, as is the `progressbar2` library.

`python3 sqlite3-translator.py <path-to-sqlite-dump>`

This will create a folder named `output` in your current directory containing the `mysql.sql` file. Run this file on your MySQL database and it will import everything into it.

Change the current directory to the `app` folder, and put the relevant MySQL information inside `config.json`. For clarification, `db_name` refers to the name of the database on your MySQL server, *not* the name of the SQLite one.

## Running the web server

If you haven't already, `cd` into the `app` directory. To run the server, the `flask`, `peewee`, and `requests` library must be installed. You must also be running Python3.6 at the minimum.

Run `python gensearch.py` and navigate to `localhost:5000` in your web browser. Now you're set!

The website contains documentation and such already, so feel free to look in there for usage!


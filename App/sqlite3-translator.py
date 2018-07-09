import re
import sys
import progressbar

IGNOREDPREFIXES = [
    'PRAGMA',
    'BEGIN TRANSACTION;',
    'COMMIT',
    'DELETE FROM sqlite_sequence;',
    'INSERT INTO "sqlite_sequence"',
    'DROP INDEX',
    'CREATE INDEX'
]


REPLACEMAP = {
    "AUTOINCREMENT": "AUTO_INCREMENT",
    "DEFAULT 't'": "DEFAULT '1'",
    "DEFAULT 'f'": "DEFAULT '0'",
    ",'t'": ",'1'",
    ",'f'": ",'0'",
    "VARCHAR": "VARCHAR(255)",
    '\"': "`"
}


def convert(filename):
    data = ""
    with open(filename, "r") as f:
        data = f.read()
    with open(filename, "w") as f:
        data = data.replace(' usage', '`usage`')
        data = re.sub(r"\n((?:([A-Z])[a-z]))", r" & \1", data)
        f.write(data)

    newfile = []
    with open(filename, "r", encoding="utf-8") as f:
        current_prefix = "INSERT INTO"
        changed = False
        for line in progressbar.progressbar(f.readlines()):
            changed_this_round = False
            if any(prefix in line for prefix in IGNOREDPREFIXES):
                continue
            if not changed and line.startswith(current_prefix):
                current_prefix = ' '.join(line.split()[0:3])
                line = line.replace(';', '')
                changed = True
                changed_this_round = True
            elif changed and not line.startswith(current_prefix):
                changed = False
                current_prefix = "INSERT INTO"
                newfile.append(';')
            for key in REPLACEMAP.keys():
                line = re.sub(key, REPLACEMAP[key], line, flags=re.IGNORECASE)
            if not changed_this_round and line.startswith(current_prefix):
                try:
                    values = line.split(" VALUES ")[1].strip()[:-1]
                    line = line.replace(line, ', ' + values + '\n')
                except IndexError:
                    pass
            newfile.append(line)
    with open("output/mysql.sql", "w", encoding="utf-8") as nf:
        nf.writelines(newfile)


if __name__ == "__main__":
    try:
        convert(sys.argv[1])
    except IndexError:
        print("\n\nUsage: python3 sqlite3-translator.py <path-to-sqlite-dump>\n\n")

import json
from peewee import MySQLDatabase

with open('config.json', 'r') as f:
    config = json.load(f)

db = MySQLDatabase(config["db_name"], **{'charset': 'utf8', 'use_unicode': True, 'host': config["db_host"], 'port': config["db_port"], 'user': config["db_username"], 'password': config["db_password"]})


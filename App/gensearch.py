from flask import Flask, render_template, url_for, jsonify
from peewee import MySQLDatabase
import json
import models.db_config as db_config
from models.models import *

app = Flask(__name__)

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/api/monster', methods=["GET"])
def get_all_monsters():
    db_config.db.connect()
    monsters = {"monsters": []}
    for monster in Monster.select():
        monsters["monsters"].append({
            "name": monster.mon_name,
            "url": url_for('get_individual_monster', name=monster.mon_name)
        })
    return jsonify(monsters)

@app.route('/api/monster/<name>', methods=["GET"])
def get_individual_monster(name):
    pass

if __name__ == "__main__":
    app.run(debug=True)
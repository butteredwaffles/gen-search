from flask import Flask, render_template, url_for, jsonify
import models.db_config as db_config
from models.models import *

app = Flask(__name__)
BREAK_WORDS = ["Wound", "Capture", "Shiny", "Break", "Carve", "Gather"]

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
    db_config.db.close()
    return jsonify(monsters)


@app.route('/api/monster/<name>', methods=["GET"])
def get_individual_monster(name):
    db_config.db.connect()
    mon = Monster.get(Monster.mon_name == name.title())
    monster = {
        "name": mon.mon_name,
        "base_hp": mon.base_hp,
        "base_size": mon.base_size,
        "crown_sizes": {
            "small_gold": mon.small_size,
            "silver": mon.silver_size,
            "large_gold": mon.king_size
        },
        "parts": [],
    }

    for part in MonsterPart.select().where(mon.id == MonsterPart.monsterid):
        if not any(break_keyword in part.part_name for break_keyword in BREAK_WORDS):
            monster["parts"].append({
                "part_name": part.part_name,
                "stagger_value": part.stagger_value,
                "extract_color": part.extract_color if part.extract_color is not None else "N/A"
            })
    db_config.db.close()
    return jsonify(monster)


if __name__ == "__main__":
    app.run(debug=True)

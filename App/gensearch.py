from flask import Flask, render_template, url_for, jsonify
import json
import requests
import models.db_config as db_config
from models.models import *

app = Flask(__name__)
BREAK_WORDS = ["Wound", "Capture", "Shiny", "Break", "Carve", "Gather"]


@app.route('/')
def index():
    example = json.dumps(requests.get(url_for('get_individual_monster', name="seltas", _external=True)).json(), indent=4, ensure_ascii=False)
    print(example)
    return render_template('index.html', code_example=example)


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
        "drops": {
            "high": [],
            "low": []
        },
        "quests": [],
        "armor": [],
        "weapons": {
            "blademaster": [],
            "gunner": []
        }
    }

    for part in MonsterPart.select().where(mon.id == MonsterPart.monsterid):
        if not any(break_keyword in part.part_name for break_keyword in BREAK_WORDS):
            monster["parts"].append({
                "part_name": part.part_name,
                "stagger_value": part.stagger_value,
                "extract_color": part.extract_color if part.extract_color is not None else "N/A"
            })

    for drop in MonsterDrop.select().where(mon.id == MonsterDrop.monsterid):
        if drop.rank == "Low":
            monster["drops"]["low"].append({
                "item_name": Item.get(Item.id == drop.itemid).item_name,
                "source": MonsterPart.get(drop.sourceid == MonsterPart.id).part_name,
                "rank": drop.rank,
                "drop_chance": str(drop.drop_chance) + "%",
                "quantity": drop.quantity
            })
        else:
            monster["drops"]["high"].append({
                "item_name": Item.get(Item.id == drop.itemid).item_name,
                "source": MonsterPart.get(drop.sourceid == MonsterPart.id).part_name,
                "rank": drop.rank,
                "drop_chance": str(drop.drop_chance) + "%",
                "quantity": drop.quantity
            })

    all_armor = [armor.armor_set for armor in Armor.select().where(mon.id == Armor.monster_id)]
    monster["armor"] = list(set(all_armor))

    for quest in QuestMonster.select().where(mon.id == QuestMonster.monsterid):
        monster["quests"].append({
            "quest_name": Quest.get(quest.questid == Quest.id).quest_name,
            "amount": quest.amount,
            "special_attribute": quest.isSpecial,
            "monster_stats": {
                "hp": quest.mon_hp,
                "stagger_multiplier": quest.stag_multiplier,
                "attack_multiplier": quest.atk_multiplier,
                "defense_multiplier": quest.def_multiplier,
                "exhaust_multiplier": quest.exh_multiplier,
                "dizzy_multiplier": quest.diz_multiplier,
                "mount_multiplier": quest.mnt_multiplier
            }
        })

    monster["weapons"]["blademaster"] = [weapon.sword_name for weapon in BlademasterWeapon.select().where(mon.id == BlademasterWeapon.monster_id)]
    monster["weapons"]["gunner"] = [weapon.bow_name for weapon in Bow.select().where(mon.id == Bow.monster_id)]
    monster["weapons"]["gunner"] += [weapon.bg_name for weapon in Bowgun.select().where(mon.id == Bowgun.monster_id)]

    db_config.db.close()
    return jsonify(monster)


if __name__ == "__main__":
    app.run(debug=True, host="localhost", port=5000)

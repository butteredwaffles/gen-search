from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

monster_routes = Blueprint('monsters', __name__, template_folder="../templates", static_folder="../static")
BREAK_WORDS = ["Wound", "Capture", "Shiny", "Break", "Carve", "Gather"]


@monster_routes.route('/', methods=["GET"])
def get_all_monsters():
    db.connect()
    monsters = {"monsters": []}
    for monster in Monster.select():
        monsters["monsters"].append({
            "name": monster.mon_name,
            "url": url_for('.get_individual_monster', name=monster.mon_name, _external=True)
        })
    db.close()
    return jsonify(monsters)


@monster_routes.route('/<name>', methods=["GET"])
def get_individual_monster(name):
    db.connect()
    mon = Monster.get(Monster.mon_name == name.title().replace("_", " "))
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

    weapons = {
        "blademaster": SwordValue.select().where(mon.id == SwordValue.monster_id),
        "bow": Bow.select().where(mon.id == Bow.monster_id),
        "bowgun": Bowgun.select().where(mon.id == Bowgun.monster_id)
    }
    for weapon in weapons["blademaster"]:
        monster["weapons"].setdefault(weapon.sword_class.lower().replace("&", "and").replace(" ", "_"), []).append(weapon.sword_set_name)
    for weapon in weapons["bow"]:
        monster["weapons"].setdefault("bow", []).append(weapon.bow_set_name)
    for weapon in weapons["bowgun"]:
        monster["weapons"].setdefault(weapon.bg_type.lower().replace(" ", "_"), []).append(weapon.bg_set_name)
    for wpn_class in monster["weapons"].keys():
        monster["weapons"][wpn_class] = list(set(monster["weapons"][wpn_class]))

    db.close()
    return jsonify(monster)

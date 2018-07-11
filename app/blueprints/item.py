from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

item_routes = Blueprint('items', __name__, template_folder="../templates", static_folder="../static")


@item_routes.route('/', methods=["GET"])
def get_all_items():
    db.connect()
    items = {"items": []}
    for item in Item.select():
        items["items"].append({
            "name": item.item_name,
            "url": url_for('.get_individual_item', name=item.item_name, _external=True)
        })
    db.close()
    return jsonify(items)


@item_routes.route('/<name>', methods=["GET"])
def get_individual_item(name):
    db.connect()
    db_item = Item.get(Item.item_name == name.title().replace("_", " "))
    item = {
        "name": db_item.item_name,
        "description": db_item.description,
        "rarity": db_item.rarity,
        "max_stack_size": db_item.max_stack,
        "sell_price": db_item.sell_price,
        "combination": db_item.combinations.split(' + '),
        "crafting": {
            "weapons": {
                "create": [],
                "upgrade": [],
                "byproduct": []
            },
            "armor": {
                "create": [],
                "upgrade": [],
                "byproduct": []
            }
        }
    }

    weapon_crafts = CraftItem.select().where(CraftItem.item_name == item["name"])
    for craft in weapon_crafts:
        weapon_name = ""
        if craft.creation_type == "Blademaster":
            weapon_name = BlademasterWeapon.get(BlademasterWeapon.sword_id == craft.creation_id).sword_name
        elif craft.creation_type == "Bowgun":
            weapon_name = Bowgun.get(Bowgun.bg_id == craft.creation_id).bg_name
        else:
            weapon_name = Bow.get(Bow.bow_id == craft.creation_id).bow_name
        item["crafting"]["weapons"][craft.usage].append({
            "weapon_name": weapon_name,
            "quantity": craft.quantity,
            "unlocks_creation": True if craft.unlocks_creation == "yes" else False
        })

    return jsonify(item)

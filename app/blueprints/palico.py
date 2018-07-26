from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

palico_routes = Blueprint('palicoes', __name__, template_folder="../templates", static_folder="../static")


@palico_routes.route('/<category>', methods=["GET"])
def get_all_palico_category_info(category):
    db.connect()
    if category == "weapons":
        db_weapons = PalicoWeapon.select(PalicoWeapon.pw_name)
        weapons = {
            "weapons": [{"name": weapon.pw_name, "url": url_for(".get_individual_palico_info", category="weapons", name=weapon.pw_name, _external=True)} for weapon in db_weapons]
        }
        db.close()
        return jsonify(weapons)
    db.close()
    return jsonify({"message": "Invalid category!"})


@palico_routes.route('/<category>/<name>', methods=["GET"])
def get_individual_palico_info(category, name):
    db.connect()
    if category == "weapons":
        dbwep = PalicoWeapon.get(PalicoWeapon.pw_name == name)
        weapon = {
            "name": dbwep.pw_name,
            "description": dbwep.pw_description,
            "rarity": dbwep.pw_rarity,
            "price": dbwep.pw_price,
            "type": dbwep.pw_type,
            "damage_type": dbwep.pw_damage_type,
            "melee_damage": dbwep.pw_damage,
            "melee_affinity": dbwep.pw_affinity,
            "melee_element": dbwep.pw_element.title(),
            "melee_element_damage": dbwep.pw_element_amt,
            "melee_sharpness": dbwep.pw_sharpness.title(),
            "boomerang_damage": dbwep.pw_boomerang_damage,
            "boomerang_affinity": dbwep.pw_boomerang_affinity,
            "boomerang_element": dbwep.pw_boomerang_element.title(),
            "boomerang_element_damage": dbwep.pw_boomerang_element_amt,
            "defense": dbwep.pw_defense,
            "crafting_materials": get_crafting_list("weapon", dbwep.pw_name)
        }

        return jsonify(weapon)
    return {"message": "That is not a valid palico item!"}


def get_crafting_list(item_type, name):
    materials = []
    craftitems = PalicoCraftItem.select() \
        .where(PalicoCraftItem.type == item_type and PalicoCraftItem.palico_item == name)
    for craftitem in craftitems:
        item = Item.get_by_id(craftitem.item_id)
        materials.append({
            'name': item.item_name,
            'url': url_for("items.get_individual_item", name=item.item_name, _external=True),
            'quantity': craftitem.quantity
        })
    return materials

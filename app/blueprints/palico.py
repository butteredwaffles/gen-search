from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

palico_routes = Blueprint('palicoes', __name__, template_folder="../templates", static_folder="../static")


@palico_routes.route('/<category>', methods=["GET"])
def get_all_palico_category_info(category):
    db.connect()
    if category == "weapons":
        db_weapons = PalicoWeapon.select(PalicoWeapon.pw_name)
        weapons = {"weapons": [{"name": weapon.pw_name, "url": url_for(".get_individual_palico_info", category="weapons", name=weapon.pw_name, _external=True)} for weapon in db_weapons]}
        db.close()
        return jsonify(weapons)
    elif category == "armor":
        db_armor = PalicoArmor.select(PalicoArmor.pa_name)
        armor = {"weapons": [{"name": armor.pa_name, "url": url_for(".get_individual_palico_info", category="armor", name=armor.pa_name, _external=True)} for armor in db_armor]}
        db.close()
        return jsonify(armor)
    elif category == "skills":
        db_skills = PalicoSkill.select()
        skills = {"skills": []}
        for skill in db_skills:
            skills["skills"].append({
                "name": skill.ps_name,
                "type": skill.ps_type,
                "description": skill.ps_description,
                "memory": skill.ps_memory_req,
                "learn_level": skill.ps_learn_level
            })
        db.close()
        return jsonify(skills)
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
            "sharpness": dbwep.pw_sharpness.title(),
            "melee_damage": dbwep.pw_damage,
            "melee_affinity": dbwep.pw_affinity,
            "melee_element": dbwep.pw_element.title(),
            "melee_element_damage": dbwep.pw_element_amt,
            "boomerang_damage": dbwep.pw_boomerang_damage,
            "boomerang_affinity": dbwep.pw_boomerang_affinity,
            "boomerang_element": dbwep.pw_boomerang_element.title(),
            "boomerang_element_damage": dbwep.pw_boomerang_element_amt,
            "defense": dbwep.pw_defense,
            "crafting_materials": get_crafting_list("weapon", dbwep.pw_name)
        }
        db.close()
        return jsonify(weapon)
    elif category == "armor":
        dbarm = PalicoArmor.get(PalicoArmor.pa_name == name)
        armor = {
            "name": dbarm.pa_name,
            "description": dbarm.pa_description,
            "rarity": dbarm.pa_rarity,
            "price": dbarm.pa_price,
            "defense": dbarm.pa_defense,
            "fire_def": dbarm.pa_fire,
            "water_def": dbarm.pa_water,
            "thunder_def": dbarm.pa_thunder,
            "ice_def": dbarm.pa_ice,
            "dragon_def": dbarm.pa_dragon,
            "crafting_materials": get_crafting_list("armor", dbarm.pa_name)
        }
        db.close()
        return jsonify(armor)
    elif category == "skills":
        return jsonify({"message": f"There is no individual page for palico skills. If you need information for them, use this endpoint: {url_for('.get_all_palico_category_info', category='skills', _external=True)}"})
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

from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

weapon_routes = Blueprint('weapons', __name__, template_folder="../templates", static_folder="../static")


@weapon_routes.route('/<category>', methods=["GET"])
def get_all_weapons_by_category(category):
    category = category.lower()
    db.connect()
    weapons = {"weapons": []}
    found_sets = []
    if category == "blademaster":
        for weapon in SwordValue.select():
            if weapon.sword_set_name not in found_sets:
                found_sets.append(weapon.sword_set_name)
                weapons["weapons"].append({
                    "set": weapon.sword_set_name,
                    "class": weapon.sword_class,
                    "url": url_for(".get_weapon", _external=True, category=category, setname=weapon.sword_set_name)
                })
    db.close()
    return jsonify(weapons)


@weapon_routes.route('/<category>/<setname>', methods=["GET"])
def get_weapon(category, setname):
    category = category.lower()
    db.connect()
    weapons = {"weapons": []}
    if category == "blademaster":
        for wep in SwordValue.select().where(SwordValue.sword_set_name == setname):
            weapon = {
                "name": wep.sword_name,
                "class": wep.sword_class,
                "damage": wep.raw_dmg,
                "affinity": wep.affinity,
                "slots": wep.slots,
                "rarity": wep.rarity,
                "description": wep.description,
                "price": wep.price,
                "monster": "none",
                "element": "none",
                "element_damage": 0,
                "upgrades_into": [
                    url_for('.get_weapon', _external=True, category="blademaster", setname=upgrade.replace(' 1', ""))
                    for upgrade in wep.upgrades_into.split(' & ') if upgrade != "none"
                ],
                "sharpness": {},
                "crafting_materials": [],
                "scraps": []
            }

            element = ElementDamage.select().where(ElementDamage.weapon_id == wep.sword_id)
            if len(element) > 0:
                weapon["element"] = element[0].elem_type
                weapon["element_damage"] = element[0].elem_amount

            if wep.monster_id != -1:
                weapon["monster"] = Monster.get_by_id(wep.monster_id).mon_name

            sharpness_bars = {
                "normal": SharpnessValue.get_by_id(wep.sharp_0_id),
                "sharpness+1": SharpnessValue.get_by_id(wep.sharp_1_id),
                "sharpness+2": SharpnessValue.get_by_id(wep.sharp_2_id)
            }
            for key, value in sharpness_bars.items():
                weapon["sharpness"][key] = {
                    "red_sharpness": value.red_sharpness_length,
                    "orange_sharpness": value.orange_sharpness_length,
                    "yellow_sharpness": value.yellow_sharpness_length,
                    "green_sharpness": value.green_sharpness_length,
                    "blue_sharpness": value.blue_sharpness_length,
                    "white_sharpness": value.white_sharpness_length
                }

            if wep.sword_class == "Hunting Horn":
                weapon["notes"] = HuntingHorn.get(HuntingHorn.sword_id == wep.sword_id).notes
            
            if wep.sword_class in ["Charge Blade", "Switch Axe", "Gunlance"]:
                weapon["phial_shell"] = PhialAndShellWeapon.get(PhialAndShellWeapon.sword_id == wep.sword_id).phial_or_shell_type

            for item in CraftItem.select().where(CraftItem.creation_type == "Blademaster", CraftItem.creation_id == wep.sword_id):
                _item = {
                    "name": item.item_name,
                    "quantity": item.quantity,
                    "unlocks_creation": True if item.unlocks_creation == "yes" else False,
                }
                if item.is_scrap == "no":
                    weapon["crafting_materials"].append(_item)
                else:
                    weapon["scraps"].append(_item)
            
            weapons["weapons"].append(weapon)
    db.close()
    return jsonify(weapons)

from flask import Blueprint, jsonify, url_for
from models.models import *
from models.db_config import db


BOW_COATINGS = {
    "Pow1": {
        "name": "Power Lvl1",
        "stack_size": 50,
        "effect": "1.35x raw"
    },
    "Pow2": {
        "name": "Power Lvl2",
        "stack_size": 50,
        "effect": "1.50x raw"
    },
    "Ele1": {
        "name": "Element Lvl1",
        "stack_size": 20,
        "effect": "1.35x element"
    },
    "Ele2": {
        "name": "Element Lvl2",
        "stack_size": 20,
        "effect": "1.50x element"
    },
    "CRan": {
        "name": "Close Range",
        "stack_size": 20,
        "effect": "Extends critical distance length"
    },
    "Psn": {
        "name": "Poison",
        "stack_size": 20,
        "effect": "Applies poison element"
    },
    "Par": {
        "name": "Paralysis",
        "stack_size": 20,
        "effect": "Applies paralysis element"
    },
    "Sle": {
        "name": "Sleep",
        "stack_size": 20,
        "effect": "Applies sleep element"
    },
    "Exh": {
        "name": "Exhaust",
        "stack_size": 20,
        "effect": "Applies exhaust and KO (if hit on head)"
    },
    "Bla": {
        "name": "Blast",
        "stack_size": 20,
        "effect": "Applies blast element"
    }
}
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
    elif category == "bow":
        for weapon in Bow.select():
            if weapon.bow_set_name not in found_sets:
                found_sets.append(weapon.bow_set_name)
                weapons["weapons"].append({
                    "set": weapon.bow_set_name,
                    "url": url_for(".get_weapon", _external=True, category=category, setname=weapon.bow_set_name)
                })
    elif category == "bowgun":
        for weapon in Bowgun.select():
            if weapon.bg_set_name not in found_sets:
                found_sets.append(weapon.bg_set_name)
                weapons["weapons"].append({
                    "set": weapon.bg_set_name,
                    "type": weapon.bg_type,
                    "url": url_for(".get_weapon", _external=True, category=category, setname=weapon.bg_set_name)
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
                "elements": [],
                "upgrades_into": [
                    url_for('.get_weapon', _external=True, category="blademaster", setname=upgrade.replace(' 1', ""))
                    for upgrade in wep.upgrades_into.split(' & ') if upgrade != "none"
                ],
                "sharpness": {},
                "crafting_materials": [],
                "scraps": [],
                "defense": 0,
            }

            elements = ElementDamage.select().where(ElementDamage.weapon_id == wep.sword_id)
            if len(elements) > 0:
                for element in elements:
                    if "Def" == element.elem_type:
                        weapon["defense"] = element.elem_amount
                    else:
                        weapon["elements"].append({
                            "element": element.elem_type,
                            "element_damage": element.elem_amount
                        })

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

            weapon["crafting_materials"], weapon["scraps"] = get_crafting_items("Blademaster", wep.sword_id)
            weapons["weapons"].append(weapon)
    elif category == "bow":
        for db_bow in Bow.select().where(Bow.bow_set_name == setname):
            bow = {
                "name": db_bow.bow_name,
                "damage": db_bow.bow_damage,
                "affinity": db_bow.affinity,
                "arc_type": db_bow.arc_type,
                "charge_shots": [db_bow.level_one_charge, db_bow.level_two_charge, db_bow.level_three_charge, db_bow.level_four_charge],
                "supported_coatings": [BOW_COATINGS[coating] for coating in db_bow.supported_coatings.split(', ')],
                "slots": db_bow.slots,
                "rarity": db_bow.rarity,
                "description": db_bow.description,
                "elements": [],
                "defense": 0,
                "monster": "none"
            }
            bow["crafting_materials"], bow["scraps"] = get_crafting_items("Bow", db_bow.bow_id)

            if db_bow.monster_id != -1:
                bow["monster"] = Monster.get_by_id(db_bow.monster_id).mon_name

            elements = ElementDamage.select().where(ElementDamage.weapon_id == db_bow.bow_id)
            if len(elements) > 0:
                for element in elements:
                    if "Def" == element.elem_type:
                        bow["defense"] = element.elem_amount
                    else:
                        bow["elements"].append({
                            "element": element.elem_type,
                            "element_damage": element.elem_amount
                        })

            weapons["weapons"].append(bow)
    elif category == "bowgun":
        for db_gun in Bowgun.select().where(Bowgun.bg_set_name == setname):
            gun = {
                "name": db_gun.bg_name,
                "type": db_gun.bg_type,
                "damage": db_gun.bg_damage,
                "affinity": db_gun.affinity,
                "reload_speed": db_gun.reload_speed,
                "recoil": db_gun.recoil,
                "deviation": db_gun.deviation,
                "slots": db_gun.slots,
                "rarity": db_gun.rarity,
                "description": db_gun.description,
                "shots": {
                    "regular": [],
                    "internal": [],
                    "special_fire": []
                },
                "defense": 0
            }

            if db_gun.monster_id != -1:
                gun["monster"] = Monster.get_by_id(db_gun.monster_id).mon_name
            gun["crafting_materials"], gun["scraps"] = get_crafting_items("Bowgun", db_gun.bg_id)
            defense = ElementDamage.select().where(ElementDamage.weapon_id == db_gun.bg_id).limit(1)
            if len(defense) > 0:
                # Bowguns do not have traditional elements, but they do have defense
                gun["defense"] = defense[0].elem_amount

            ammo = BowgunAmmo.get(BowgunAmmo.bowgun_id == db_gun.bg_id)
            for shot, amount in ammo.__dict__["__data__"].items():
                if shot == "ammo_id" or shot == "bowgun_id":
                    continue
                gun["shots"]["regular"].append(
                    {
                        "ammo_name": shot,
                        "amount": int(amount[0]),
                        "skill_required": True if "skill" in amount else False
                    }
                )

            for shot in SpecialBowgunAmmo.select().where(SpecialBowgunAmmo.bowgun_id == db_gun.bg_id):
                sp_shot = {
                    "ammo_name": shot.ammo_name,
                    "shots": shot.shots,
                }
                if shot.ammo_type == "Rapid Fire":
                    # Converting the multiplier to a value between 0 and 1
                    sp_shot["multiplier"] = float(shot.multiplier / 100)
                    sp_shot["wait"] = shot.wait

                gun["shots"]["special_fire"].append(sp_shot)

            for internal_ammo in InternalBowgunAmmo.select().where(InternalBowgunAmmo.bowgun_id == db_gun.bg_id):
                gun["shots"]["internal"].append({
                    "ammo_name": internal_ammo.ammo_name,
                    "total_amount": internal_ammo.total_ammo,
                    "load_amount": internal_ammo.load_amt
                })

            weapons["weapons"].append(gun)
    db.close()
    return jsonify(weapons)


def get_crafting_items(creation_type, weapon_id):
    crafts = []
    scraps = []
    for item in CraftItem.select().where(CraftItem.creation_type == creation_type,
                                         CraftItem.creation_id == weapon_id):
        _item = {
            "name": item.item_name,
            "quantity": item.quantity,
            "unlocks_creation": True if item.unlocks_creation == "yes" else False,
        }
        if item.is_scrap == "no":
            crafts.append(_item)
        else:
            scraps.append(_item)
    return crafts, scraps
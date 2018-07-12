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
            },
            "decorations": [],
            "palico": {
                "armor": [],
                "weapon": []
            }
        },
        "quest_rewards": []
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
            "name": weapon_name,
            "quantity": craft.quantity,
            "unlocks_creation": True if craft.unlocks_creation == "yes" else False
        })

    armor_crafts = ArmorCraftItem.select().where(ArmorCraftItem.item_name == item["name"])
    for craft in armor_crafts:
        item["crafting"]["armor"]["create"].append({
            "name": Armor.get(Armor.armor_id == craft.armor_id).armor_name,
            "quantity": craft.quantity,
            "unlocks_armor": True if craft.unlocks_armor == 1 else False
        })

    upgrade_crafts = ArmorUpgradeItem.select().where(ArmorUpgradeItem.item_name == item["name"])
    for craft in upgrade_crafts:
        item["crafting"]["armor"]["upgrade"].append({
            "name": Armor.get(Armor.armor_id == craft.armor_id).armor_name,
            "quantity": craft.quantity,
            "level": craft.upgrade_level
        })

    if "Scrap" in item["name"]:
        reward_scraps = ArmorScrapReward.select().where(ArmorScrapReward.item_id == db_item.id)
        for scrap in reward_scraps:
            item["crafting"]["armor"]["byproduct"].append({
                "name": Armor.get(Armor.armor_id == scrap.armor_id).armor_name,
                "quantity": scrap.quantity,
                "source": scrap.type,
                "level": scrap.level
            })

    decocombos = DecorationCombination.select()
    for combo in decocombos.where(DecorationCombination.item_1_id == db_item.id or DecorationCombination.item_2_id == db_item.id or DecorationCombination.item_3_id == db_item.id):
        quantity = 0
        if combo.item_1_id == db_item.id:
            quantity = combo.item_1_quantity
        elif combo.item_1_id == db_item.id:
            quantity = combo.item_2_quantity,
        else:
            quantity = combo.item_3_quantity
        item["crafting"]["decorations"].append({
            "name": Decoration.get(combo.deco_id == Decoration.deco_id).deco_name,
            "quantity": quantity
        })

    for craft in PalicoCraftItem.select().where(PalicoCraftItem.item_id == db_item.id):
        item["crafting"]["palico"][craft.type].append({
            "name": craft.palico_item,
            "quantity": craft.quantity
        })

    for reward in QuestBoxItem.select().where(QuestBoxItem.itemid == db_item.id):
        item["quest_rewards"].append({
            "quest": Quest.get(Quest.id == reward.questid).quest_name,
            "quantity": reward.quantity,
            "box": reward.box_type,
            "appearance_chance": reward.appear_chance
        })

    return jsonify(item)

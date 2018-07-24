from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

deco_routes = Blueprint('decorations', __name__, template_folder="../templates", static_folder="../static")


@deco_routes.route('/', methods=["GET"])
def get_all_decorations():
    db.connect()
    decorations = {"decorations": []}
    for deco in Decoration.select():
        decorations["decorations"].append({
            "name": deco.deco_name,
            "url": url_for('.get_individual_decoration', name=deco.deco_name, _external=True)
        })
    db.close()
    return jsonify(decorations)


@deco_routes.route('/<name>', methods=["GET"])
def get_individual_decoration(name):
    db.connect()
    db_deco = Decoration.get(Decoration.deco_name == name)
    deco = {
        "name": db_deco.deco_name,
        "slots": db_deco.deco_slot_requirement,
        "positive_skill": {
            "name": db_deco.positive_skill_tree,
            "skill_points": db_deco.positive_skill_effect,
            "url": url_for("skills.get_skill_tree", tree_name=db_deco.positive_skill_tree, _external=True)
        },
        "negative_skill": {
            "name": db_deco.negative_skill_tree,
            "skill_points": db_deco.negative_skill_effect,
            "url": url_for("skills.get_skill_tree", tree_name=db_deco.negative_skill_tree, _external=True)
        }
    }

    combo = DecorationCombination.get(DecorationCombination.deco_id == db_deco.deco_id)
    item_1 = Item.get_by_id(combo.item_1_id).item_name
    item_2 = Item.get_by_id(combo.item_2_id).item_name
    deco["combination"] = [
        {
            "name": item_1,
            "quantity": combo.item_1_quantity,
            "url": url_for('items.get_individual_item', name=item_1, _external=True)
        },
        {
            "name": item_2,
            "quantity": combo.item_2_quantity,
            "url": url_for('items.get_individual_item', name=item_2, _external=True)
        }
    ]
    if combo.item_3_id != -1:
        item_3 = Item.get_by_id(combo.item_3_id).item_name
        deco["combination"].append({
            "name": item_3,
            "quantity": combo.item_3_quantity,
            "url": url_for('items.get_individual_item', name=item_3, _external=True)
        })
    db.close()
    return jsonify(deco)

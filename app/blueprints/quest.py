from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

quest_routes = Blueprint('quests', __name__, template_folder="../templates", static_folder="../static")


@quest_routes.route('/', methods=["GET"])
def get_all_quests():
    db.connect()
    quests = {"quests": []}
    for quest in Quest.select():
        quests["quests"].append({
            "name": quest.quest_name,
            "url": url_for('.get_individual_quest', name=quest.quest_name, _external=True)
        })
    db.close()
    return jsonify(quests)


@quest_routes.route('/<name>', methods=["GET"])
def get_individual_quest(name):
    pass

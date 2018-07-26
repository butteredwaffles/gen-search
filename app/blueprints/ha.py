from flask import Blueprint, jsonify
from models.db_config import db
from models.models import *

ha_routes = Blueprint('has', __name__, template_folder="../templates", static_folder="../static")


@ha_routes.route('/', methods=["GET"])
def get_hunter_arts():
    db.connect()
    arts = {"arts": []}
    for art in HunterArt.select():
        art_quests = HunterArtUnlock.select().where(art.art_id == HunterArtUnlock.art_id)
        quests = Quest.select(Quest.id, Quest.quest_name)
        arts["arts"].append({
            "name": art.art_name,
            "gauge_required": art.art_gauge,
            "description": art.art_description,
            "required_quests": [quest.quest_name for art_quest in art_quests for quest in quests.where(art_quest.quest_id == Quest.id)]
        })
    db.close()
    return jsonify(arts)

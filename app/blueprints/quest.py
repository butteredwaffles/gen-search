import re
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
            "url": url_for('.get_individual_quest', name=re.sub(r"\[.*?\] ", "", quest.quest_name), _external=True)
        })
    db.close()
    return jsonify(quests)


@quest_routes.route('/<name>', methods=["GET"])
def get_individual_quest(name):
    db.connect()
    # Some quests have the same name, so return a wrapper list
    quests = {"quests": []}
    for dbq in Quest.select().where(Quest.quest_name.contains(name)):
        goals = [{
            "zenny_reward": db_goal.zenny_reward,
            "hrp_reward": db_goal.hrp_reward,
            "wycadpts_reward": db_goal.wycadpts_reward,
            "description": db_goal.goal_description
        } for db_goal in [
            QuestGoal.get_by_id(dbq.goalid),
            QuestGoal.get_by_id(dbq.subgoalid)]]

        monsters = [{
            "name": Monster.get_by_id(quest.monsterid).mon_name,
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
        } for quest in QuestMonster.select().where(dbq.id == QuestMonster.questid)]

        rewards = [{
            "name": Item.get_by_id(item.itemid).item_name,
            "box": item.box_type,
            "quantity": item.quantity,
            "appear_chance": item.appear_chance if item.box_type != "Supplies" else 100
        } for item in QuestBoxItem.select().where(dbq.id == QuestBoxItem.questid)]

        quest = {
            "name": dbq.quest_name,
            "type": dbq.quest_type,
            "description": dbq.quest_description,
            "is_key": True if dbq.isKey == "yes" else False,
            "is_prowler": True if dbq.isProwler == "yes" else False,
            "is_unstable": True if dbq.isUnstable == "yes" else False,
            "time_limit": dbq.timeLimit,
            "contract_fee": dbq.contractFee,
            "goal": goals[0],
            "subgoal": goals[1],
            "monsters": monsters,
            "rewards": rewards,
            "prerequisites": [],
            "unlocks": [],
        }

        for que in QuestUnlock.select().where(dbq.id == QuestUnlock.questid):
            if que.unlock_type == "prerequisite":
                quest["prerequisites"].append({
                    "name": que.quest_name,
                    "url": url_for('.get_individual_quest', name=re.sub(r"\[.*?\] ", "", que.quest_name), _external=True)
                })
            else:
                quest["unlocks"].append({
                    "name": que.quest_name,
                    "url": url_for('.get_individual_quest', name=re.sub(r"\[.*?\] ", "", que.quest_name), _external=True)
                })

        quests["quests"].append(quest)
    db.close()
    return jsonify(quests)

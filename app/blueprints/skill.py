from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *

skill_routes = Blueprint('skills', __name__, template_folder="../templates", static_folder="../static")


@skill_routes.route('/', methods=["GET"])
def get_all_skills():
    db.connect()
    skills = {"skills": []}
    for skill in Skill.select(Skill.skill_tree).distinct():
        skills["skills"].append(url_for(".get_skill_tree", tree_name=skill.skill_tree, _external=True))
    db.close()
    return jsonify(skills)


@skill_routes.route('/<tree_name>', methods=["GET"])
def get_skill_tree(tree_name):
    db.connect()
    skills = Skill.select().where(Skill.skill_tree == tree_name)
    tree = {
        "skill_tree_name": tree_name,
        "skills": [],
        "decorations": {
            "helpful": [],
            "harmful": []
        }
    }
    for skill in skills:
        tree["skills"].append({
            "name": skill.skill_name,
            "points": skill.skill_value,
            "description": skill.skill_description
        })

    for deco in Decoration.select().where(Decoration.negative_skill_tree == tree_name):
        tree["decorations"]["harmful"].append(
            {
                "name": deco.deco_name,
                "points": deco.negative_skill_effect
            }
        )
    for deco in Decoration.select().where(Decoration.positive_skill_tree == tree_name):
        tree["decorations"]["helpful"].append(
            {
                "name": deco.deco_name,
                "points": deco.positive_skill_effect
            }
        )
    db.close()
    return jsonify(tree)


@skill_routes.route('/<tree_name>/<name>', methods=["GET"])
def get_individual_skill(tree_name, name):
    pass

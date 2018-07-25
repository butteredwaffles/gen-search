from flask import Blueprint, jsonify, url_for
from models.db_config import db
from models.models import *
import peewee

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

    if not skills:
        raise peewee.DoesNotExist

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

    skill_decos = get_decorations_for_skill(tree_name)
    tree["decorations"]["harmful"] = skill_decos[0]
    tree["decorations"]["helpful"] = skill_decos[1]

    db.close()
    return jsonify(tree)


@skill_routes.route('/<tree_name>/<name>', methods=["GET"])
def get_individual_skill(tree_name, name):
    db.connect()
    db_skill = Skill.get(Skill.skill_name == name)
    skill = {
        "name": db_skill.skill_name,
        "points": db_skill.skill_value,
        "description": db_skill.skill_description,
        "decorations": {
            "harmful": [],
            "helpful": []
        }
    }
    skill_decos = get_decorations_for_skill(tree_name)
    skill["decorations"]["harmful"] = skill_decos[0]
    skill["decorations"]["helpful"] = skill_decos[1]
    db.close()
    return jsonify(skill)


def get_decorations_for_skill(tree_name):
    harmful = []
    helpful = []
    for deco in Decoration.select().where(Decoration.negative_skill_tree == tree_name):
        harmful.append({
            "name": deco.deco_name,
            "points": deco.negative_skill_effect
        })
    for deco in Decoration.select().where(Decoration.positive_skill_tree == tree_name):
        helpful.append({
            "name": deco.deco_name,
            "points": deco.positive_skill_effect
        })
    return [harmful, helpful]

import json
import os
import peewee
import requests
from flask import Flask, render_template, url_for, jsonify
from blueprints.monster import monster_routes
from blueprints.item import item_routes
from blueprints.quest import quest_routes
from blueprints.decoration import deco_routes
from blueprints.skill import skill_routes
from utility import doc_to_html

app = Flask(__name__)
app.config['JSON_AS_ASCII'] = False
app.register_blueprint(monster_routes, url_prefix="/api/monster")
app.register_blueprint(item_routes, url_prefix="/api/item")
app.register_blueprint(quest_routes, url_prefix="/api/quest")
app.register_blueprint(deco_routes, url_prefix="/api/decoration")
app.register_blueprint(skill_routes, url_prefix="/api/skill")

DOC_DIRECTORY = "doc_texts/"
TXT_DOC_DIRECTORY = "doc_texts/txt/"
HTML_DOC_DIRECTORY = "templates/endpoint_docs/"
OVERWRITE_HTML_DOC_FILES = True


@app.route('/')
def index():
    example = json.dumps(requests.get(url_for('monsters.get_individual_monster', name="seltas", _external=True)).json(), indent=4, ensure_ascii=False)
    return render_template('index.html', code_example=example)


@app.route('/docs')
def docs():
    return render_template('docs.html', item_docs=get_doc('item'), quest_docs=get_doc('quest'), monster_docs=get_doc('monster'))


@app.errorhandler(peewee.DoesNotExist)
def does_not_exist_error(error):
    return jsonify({"message": "That is not a valid object to request!"}), 400


def get_doc(name):
    txtinput = TXT_DOC_DIRECTORY + name + ".txt"
    htmloutput = HTML_DOC_DIRECTORY + name + "_doc.html"
    [os.makedirs(directory, exist_ok=True) for directory in [DOC_DIRECTORY, TXT_DOC_DIRECTORY, HTML_DOC_DIRECTORY]]

    if not os.path.exists(txtinput):
        raise NotImplementedError()

    if os.path.exists(htmloutput) and not OVERWRITE_HTML_DOC_FILES:
        with open(htmloutput, "r") as f:
            return f.read()
    else:
        doc_to_html.convert_file_to_li(txtinput, htmloutput)
        with open(htmloutput, "r") as f:
            return f.read()


if __name__ == "__main__":
    app.run(debug=True, host="localhost", port=5000)

from flask import Flask, render_template, url_for
import json
import requests
from blueprints.monster import monster_routes
from blueprints.item import item_routes

app = Flask(__name__)
app.config['JSON_AS_ASCII'] = False
app.register_blueprint(monster_routes, url_prefix="/api/monster")
app.register_blueprint(item_routes, url_prefix="/api/item")


@app.route('/')
def index():
    example = json.dumps(requests.get(url_for('monsters.get_individual_monster', name="seltas", _external=True)).json(), indent=4, ensure_ascii=False)
    return render_template('index.html', code_example=example)


@app.route('/docs')
def docs():
    return render_template('docs.html')


if __name__ == "__main__":
    app.run(debug=True, host="localhost", port=5000)

from peewee import Model, IntegerField, FloatField, CharField
from . import db_config


class BaseModel(Model):
    class Meta:
        database = db_config.db


class Item(BaseModel):
    id = IntegerField()
    item_name = CharField()
    description = CharField()
    rarity = IntegerField()
    max_stack = IntegerField()
    sell_price = IntegerField()
    combinations = CharField()

    class Meta:
        table_name = "items"


class Monster(BaseModel):
    id = IntegerField()
    base_hp = IntegerField()
    base_size = FloatField()
    king_size = FloatField()
    mon_name = CharField(unique=True)
    silver_size = FloatField()
    small_size = FloatField()

    class Meta:
        table_name = "monsters"


class MonsterPart(BaseModel):
    id = IntegerField()
    part_name = CharField()
    stagger_value = IntegerField()
    extract_color = CharField()
    monsterid = IntegerField()

    class Meta:
        table_name = "monsterparts"


class MonsterDrop(BaseModel):
    id = IntegerField()
    itemid = IntegerField()
    monsterid = IntegerField()
    sourceid = IntegerField()
    rank = CharField()
    drop_chance = IntegerField()
    quantity = IntegerField()

    class Meta:
        table_name = "monsterdrops"

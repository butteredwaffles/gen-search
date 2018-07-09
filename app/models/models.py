from peewee import Model, IntegerField, FloatField, CharField
from . import db_config

class BaseModel(Model):
    class Meta:
        database = db_config.db

class Monster(BaseModel):
    base_hp = IntegerField(null=True)
    base_size = FloatField(null=True)
    king_size = FloatField(null=True)
    mon_name = CharField(null=True, unique=True)
    silver_size = FloatField(null=True)
    small_size = FloatField(null=True)

    class Meta:
        table_name = 'monsters'
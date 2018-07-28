from peewee import Model, IntegerField, FloatField, CharField
from . import db_config


class BaseModel(Model):
    class Meta:
        database = db_config.db
        table_function = lambda cls: cls.__qualname__.split('.')[0].lower() + "s"


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


class CraftItem(BaseModel):
    craft_id = IntegerField(primary_key=True)
    item_name = CharField()
    creation_type = CharField()
    creation_id = IntegerField()
    quantity = IntegerField()
    unlocks_creation = CharField()
    is_scrap = CharField()
    usage = CharField()


class Monster(BaseModel):
    id = IntegerField()
    base_hp = IntegerField()
    base_size = FloatField()
    king_size = FloatField()
    mon_name = CharField(unique=True)
    silver_size = FloatField()
    small_size = FloatField()


class MonsterPart(BaseModel):
    id = IntegerField()
    part_name = CharField()
    stagger_value = IntegerField()
    extract_color = CharField()
    monsterid = IntegerField()


class MonsterDrop(BaseModel):
    id = IntegerField()
    itemid = IntegerField()
    monsterid = IntegerField()
    sourceid = IntegerField()
    rank = CharField()
    drop_chance = IntegerField()
    quantity = IntegerField()


class Quest(BaseModel):
    id = IntegerField()
    quest_name = CharField()
    quest_type = CharField()
    quest_description = CharField()
    isKey = CharField()
    isProwler = CharField()
    isUnstable = CharField()
    timeLimit = IntegerField()
    contractFee = IntegerField()
    goalid = IntegerField()
    subgoalid = IntegerField()


class QuestGoal(BaseModel):
    id = IntegerField()
    zenny_reward = IntegerField()
    hrp_reward = IntegerField()
    wycadpts_reward = IntegerField()
    goal_description = CharField()


class QuestMonster(BaseModel):
    id = IntegerField()
    questid = IntegerField()
    monsterid = IntegerField()
    amount = IntegerField()
    isSpecial = CharField()
    mon_hp = IntegerField()
    stag_multiplier = FloatField()
    atk_multiplier = FloatField()
    def_multiplier = FloatField()
    exh_multiplier = FloatField()
    diz_multiplier = FloatField()
    mnt_multiplier = FloatField()


class QuestBoxItem(BaseModel):
    id = IntegerField()
    box_type = CharField()
    questid = IntegerField()
    itemid = IntegerField()
    quantity = IntegerField()
    appear_chance = IntegerField()


class QuestUnlock(BaseModel):
    id = IntegerField()
    unlock_type = CharField()
    questid = IntegerField()
    quest_name = CharField()


class Armor(BaseModel):
    armor_id = IntegerField(primary_key=True)
    armor_set = CharField()
    armor_name = CharField()
    armor_description = CharField()
    min_armor_defense = IntegerField()
    max_armor_defense = IntegerField()
    fire_def = IntegerField()
    water_def = IntegerField()
    thunder_def = IntegerField()
    ice_def = IntegerField()
    dragon_def = IntegerField()
    slots = IntegerField()
    rarity = IntegerField()
    max_upgrade = IntegerField()
    monster_id = IntegerField()
    is_blademaster = IntegerField()
    is_gunner = IntegerField()
    is_male = IntegerField()
    is_female = IntegerField()


class ArmorCraftItem(BaseModel):
    aci_id = IntegerField(primary_key=True)
    armor_id = IntegerField()
    item_name = CharField()
    quantity = IntegerField()
    unlocks_armor = IntegerField()


class ArmorUpgradeItem(BaseModel):
    aui_id = IntegerField(primary_key=True)
    armor_id = IntegerField()
    upgrade_level = IntegerField()
    item_name = CharField()
    quantity = IntegerField()


class ArmorScrapReward(BaseModel):
    asr_id = IntegerField(primary_key=True)
    armor_id = IntegerField()
    item_id = IntegerField()
    quantity = IntegerField()
    type = CharField()
    level = IntegerField()


class HunterArt(BaseModel):
    art_id = IntegerField(primary_key=True)
    art_name = CharField()
    art_gauge = IntegerField()
    art_description = CharField()


class HunterArtUnlock(BaseModel):
    unlock_id = IntegerField(primary_key=True)
    art_id = IntegerField()
    quest_id = IntegerField()


class Skill(BaseModel):
    skill_id = IntegerField(primary_key=True)
    skill_tree = CharField()
    skill_name = CharField()
    skill_value = IntegerField()
    skill_description = CharField()


class Decoration(BaseModel):
    deco_id = IntegerField(primary_key=True)
    deco_name = CharField()
    deco_slot_requirement = IntegerField()
    positive_skill_tree = CharField()
    positive_skill_effect = IntegerField()
    negative_skill_tree = CharField()
    negative_skill_effect = IntegerField()


class DecorationCombination(BaseModel):
    deco_comb_id = IntegerField(primary_key=True)
    deco_id = IntegerField()
    item_1_id = IntegerField()
    item_1_quantity = IntegerField()
    item_2_id = IntegerField()
    item_2_quantity = IntegerField()
    item_3_id = IntegerField()
    item_3_quantity = IntegerField()


class ElementDamage(BaseModel):
    elem_id = IntegerField(primary_key=True)
    weapon_id = IntegerField()
    elem_type = CharField()
    elem_amount = IntegerField()


class SwordValue(BaseModel):
    sword_id = IntegerField(primary_key=True)
    sword_class = CharField()
    sword_name = CharField()
    sword_set_name = CharField()
    monster_id = IntegerField()
    raw_dmg = IntegerField()
    affinity = IntegerField()
    sharp_0_id = IntegerField()
    sharp_1_id = IntegerField()
    sharp_2_id = IntegerField()
    slots = IntegerField()
    rarity = IntegerField()
    description = CharField()
    upgrades_into = CharField()
    price = IntegerField()


class SharpnessValue(BaseModel):
    sharp_id = IntegerField(primary_key=True)
    handicraft_modifier = IntegerField()
    red_sharpness_length = IntegerField()
    orange_sharpness_length = IntegerField()
    yellow_sharpness_length = IntegerField()
    green_sharpness_length = IntegerField()
    blue_sharpness_length = IntegerField()
    white_sharpness_length = IntegerField()


class HuntingHorn(BaseModel):
    hh_id = IntegerField(primary_key=True)
    sword_id = IntegerField()
    notes = CharField()


class PhialAndShellWeapon(BaseModel):
    pw_id = IntegerField(primary_key=True)
    sword_id = IntegerField()
    phial_or_shell_type = CharField()


class Bow(BaseModel):
    bow_id = IntegerField(primary_key=True)
    monster_id = IntegerField()
    bow_name = CharField()
    bow_set_name = CharField()
    bow_damage = IntegerField()
    affinity = IntegerField()
    arc_type = CharField()
    level_one_charge = CharField()
    level_two_charge = CharField()
    level_three_charge = CharField()
    level_four_charge = CharField()
    supported_coatings = CharField()
    slots = IntegerField()
    rarity = IntegerField()
    description = CharField()


class Bowgun(BaseModel):
    bg_id = IntegerField(primary_key=True)
    monster_id = IntegerField()
    bg_name = CharField()
    bg_type = CharField()
    bg_set_name = CharField()
    bg_damage = IntegerField()
    affinity = IntegerField()
    reload_speed = CharField()
    recoil = CharField()
    deviation = CharField()
    slots = IntegerField()
    rarity = IntegerField()
    description = CharField()


class PalicoWeapon(BaseModel):
    pw_id = IntegerField(primary_key=True)
    pw_name = CharField()
    pw_description = CharField()
    pw_rarity = IntegerField()
    pw_price = IntegerField()
    pw_type = CharField()
    pw_damage = IntegerField()
    pw_damage_type = CharField()
    pw_affinity = IntegerField()
    pw_element = CharField()
    pw_element_amt = IntegerField()
    pw_sharpness = CharField()
    pw_boomerang_damage = IntegerField()
    pw_boomerang_affinity = IntegerField()
    pw_boomerang_element = CharField()
    pw_boomerang_element_amt = IntegerField()
    pw_defense = IntegerField()


class PalicoArmor(BaseModel):
    pa_id = IntegerField(primary_key=True)
    pa_name = CharField()
    pa_description = CharField()
    pa_rarity = IntegerField()
    pa_price = IntegerField()
    pa_defense = IntegerField()
    pa_fire = IntegerField()
    pa_water = IntegerField()
    pa_thunder = IntegerField()
    pa_ice = IntegerField()
    pa_dragon = IntegerField()

    class Meta:
        table_name = "palicoarmor"


class PalicoCraftItem(BaseModel):
    pc_id = IntegerField(primary_key=True)
    palico_item = CharField()
    item_id = IntegerField()
    quantity = IntegerField()
    type = CharField()


class PalicoSkill(BaseModel):
    ps_id = IntegerField(primary_key=True)
    ps_name = CharField()
    ps_type = CharField()
    ps_description = CharField()
    ps_memory_req = IntegerField()
    ps_learn_level = IntegerField()

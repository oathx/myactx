条件判断	{"COND"，{条件}，{{满足条件触发的效果1}，{满足条件触发的效果2}}}
COND的第四位参数值可以支持施法者的属性计算，然后COND的ATTR是目标的属性，就可以比较了
	ATTR，常规属性
	DISTANCE，距离
	COMBO，连击数
	STATUS，状态名
	HP_RATE，血量万分比，后面需要用0占一位
	BLOCK_RATE，格挡值万分比，后面需要用0占一位
	BUFF_CNT 指定sn的buff的数量
	BUFF_CNT_ID 指定subid的buff数量
	BLOCKING 1格挡中 0未格挡
	REMAIN_TIME 剩余战斗时间
属性判断	{"ATTR"，{"ADD",属性名，具体值}}，TOTAL_DMG 累计受到的伤害
调用技能	{“SKILL”,技能ID}或者{"SKILL"，{{技能ID,权重}，{技能ID,权重}}}
添加buff	{"ADD_BUFF"，buffID，持续时间}
驱散buff	{"DIS_BUFF"，"SN"，buffID，数量}或者{"DIS_BUFF"，"ID"，subID，数量}
事件	{"EVENT",事件名，目标，{{触发的具体效果1}，{触发的具体效果2}}}
BE_DAMAGE: { "DMG_BY", {DMG_BY_SKILL,1101}, target, {{}}}
BE_HIT: { "DMG_BY", {DMG_BY_SKILL,1101}, target, {{}}}
BE_ATTACK: { "ATK_BY", {ATK_LIGHT,1101}, target, {{}}}
	BE_HIT 			= 命中
	BE_DAMAGE		= 受伤
	BE_ATTACK 		= 尝试攻击(起手就会触发，会影响当前的战斗结算)
	BE_COMBO 		= 连击
	BE_BLOCK 		= 格挡
	BE_BREAK_BLOCK 	= 破挡
	BE_HIT_CRITICAL 	= 暴击
	BE_KILL 		= 击杀目标
	BE_DASH 		= 冲刺
	BE_DODGE 		= 闪避
	BE_DIE 			= 死亡
	BE_HELPER 		= 协助
计时器	{"TIMER"，延迟时间，间隔时间，{{触发的具体效果1}，{触发的具体效果2}}}
状态添加清除	{"ADD_STATUS"，状态名，持续时间}和{"DEL_STATUS"，状态名}
	BS_STUN            -- 晕眩
	BS_UNDIE           -- 不死
	BS_CHAIN           -- 锁足
	BS_BLEED           -- 流血
	BS_POISON          -- 中毒
	BS_WEAK            -- 无法格挡
	BS_SUPER           -- 霸体
	BS_SILENCE         -- 沉默,参数ATK_TYPE_ALL,ATK_TYPE_ATTACK,ATK_TYPE_SKILL
	BS_STIFF           -- 僵直
	BS_LOST            -- 魅惑
	BS_FURY            -- 狂暴
	BS_STIFF			--定身
造成伤害	{ "DAMAGE", value }
反弹伤害	{ "REF_DMG", value }
吸血		{{ "VAMP", value, max }
受到某种伤害{ "DMG_BY", {BE_SKILL,BE_ATTACK}, target, {}}  if dmg_by==BE_SKILL or dmg_by==BE_ATTACK then do something
比较二者大小取大值 "return math.max(0,%d-35)"
buff的间隔时间最小为0.25秒
判断二者之间的距离 {{"COND",{"DISTANCE",BT_TARGET,Lt,5},{{效果}}}}
根据buff层数计算伤害 <BUFFCNT_1310>
给目标播放特效，time为0表示播放一次{"SFX",sfxid,time}比如{"SFX",EFFECT_FITST_ATTACK,0}
召唤机关{"INTERACTIVE",id,delta_x}比如{"INTERACTIVE",1000,3}相对距离为正表示的是目标面向的方向
{"ADD_STATUS_DATA", BS_ATTR_LOCK, {HP, 1}} 血量只能增
{"ADD_STATUS_DATA", BS_ATTR_LOCK, {HP, -1}} 血量只能减
{”ADD_STATUS_DATA", BS_ATTR_ADD_LIMIT, {HP, 1}} 血量增减值为1
{“COND", {”BOUNDARY“, 0, Gt, 2}} 0 为距离左版边的距离, 1为距离右版边的距离
{“COND", {”PROP“, "profession", Eq, PROFESSION_DESTROYER}} 可以判断角色属性配置表的的字段值，如职业，ID，性别等
{”ADD_STATUS_DATA",BS_IMMUNE,{"DAMAGE"}} 免疫伤害
{”ADD_STATUS_DATA",BS_IMMUNE,{"REF_DMG"}} 免疫反弹伤害
{”ADD_STATUS_DATA",BS_IMMUNE,{"ADD_BUFF", "SN", 1005}} 免疫指定SN的buff
{”ADD_STATUS_DATA",BS_IMMUNE,{"ADD_BUFF", "ID", 1005}} 免疫指定subid的一类buff
{”ADD_STATUS_DATA",BS_IMMUNE,{"ADD_BUFF", "ALL",}} 免疫所有buff
{”ADD_STATUS_DATA",BS_IMMUNE,{"ADD_STATUS", BS_SILENCE}} 免疫沉默
{{"FOLLOW_UP",{{"ATTR",ATTACK,Gt},{"HP_RATE",0,Lt}}, 3,true}}被动换人，主条件判断次要条件判断，延迟多久，是否播放协助，条件判断只是排序，部分属性
{ "EVENT", BE_DAMAGE, target, {"DMG_BY", DMG_FLAG_CRITICAL, target, {}} } 被暴击
{ "EVENT", BE_HIT, target, {"DMG_HIT", DMG_FLAG_CRITICAL, target, {}} } 暴击
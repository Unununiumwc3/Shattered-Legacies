"""Generates timeline-overview.drawio (Shattered Legacies timeline overview).
Usage (repo root): python3 tools/shattered-legacies/timelines/timeline_overview_gen.py [old.drawio] [out.drawio]
Default for both: src/WarcraftLegacies.Source/ShatteredLegacies/Timelines/timeline-overview.drawio (rewritten in place;
its page "test-4 (old draft)" is carried over). Edit SLOTS, POOLS, node(...) and edge(...) calls, then rerun.
The design behind every box (decision IDs D1 ... D17) is in Timelines/SCENARIO_CONCEPTS.md.
"""
import os, sys, uuid
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape

# ---------------------------------------------------------------- slots (WL PlayerSetup, P = WC3 player number)
WC3 = {'Red': '#FF0303', 'Blue': '#0042FF', 'Teal': '#1CE6B9', 'Purple': '#540081', 'Yellow': '#FFFC00',
       'Orange': '#FE8A0E', 'Green': '#20C000', 'Pink': '#E55BB0', 'Gray': '#959697', 'Light Blue': '#7EBFF1',
       'Dark Green': '#106246', 'Brown': '#4E2A04', 'Maroon': '#9B0000', 'Navy': '#0000C3', 'Turquoise': '#00EAFF',
       'Violet': '#BE00FE', 'Wheat': '#EBCD87', 'Peach': '#F8A48B', 'Mint': '#BFFF80', 'Lavender': '#DCB9EB',
       'Coal': '#282828', 'Snow': '#EBF0FF', 'Emerald': '#00781E', 'Peanut': '#A46F33'}
SLOTS = {  # key: (label, colour name)
    'P1':  ("Frostwolf / Warsong (pick)", 'Red'),
    'P2':  ("Stormwind", 'Blue'),
    'P3':  ("Quel'thalas", 'Teal'),
    'P3F': ("P3 human as Forsaken", 'Lavender'),
    'P4':  ("Scourge", 'Purple'),
    'P5':  ("Ironforge", 'Yellow'),
    'P7':  ("Fel Horde", 'Green'),
    'P8':  ("Dalaran / Gilneas (pick)", 'Pink'),
    'P9':  ("Skywall", 'Gray'),
    'P10': ("Lordaeron (Arthas / Garithos pick)", 'Light Blue'),
    'P12': ("Druids", 'Brown'),
    'P13': ("Black Empire", 'Maroon'),
    'P16': ("Illidari", 'Violet'),
    'P17': ("Ahn'qiraj", 'Wheat'),
    'P19': ("Sentinels / Draenei (pick)", 'Mint'),
    'P23': ("Kul Tiras", 'Emerald'),
    'P24': ("Burning Legion", 'Peanut'),
}
FREE = [('P11', 'clean'), ('P14', 'clean'), ('P20', 'clean'), ('P22', 'start location only'),
        ('P6', 'Warsong base preplaced'), ('P15', "Ny'alotha preplaced"), ('P18', 'Zandalari camp preplaced'),
        ('P21', 'NeutralHostileSetup uses it')]
BREAK = '#FF3333'

def hex2rgb(h): return tuple(int(h[i:i + 2], 16) for i in (1, 3, 5))
def rgb2hex(c): return '#%02X%02X%02X' % tuple(max(0, min(255, int(v))) for v in c)
def lum(h):
    r, g, b = hex2rgb(h); return (0.299 * r + 0.587 * g + 0.114 * b) / 255
def mix(h, t, a): return rgb2hex([x + (y - x) * a for x, y in zip(hex2rgb(h), hex2rgb(t))])
def fill(slot): return WC3[SLOTS[slot][1]]
def font(slot): return '#000000' if lum(fill(slot)) > 0.55 else '#FFFFFF'
def stroke(slot):
    c = fill(slot); L = lum(c)
    light = mix(c, '#000000', 0.45) if L > 0.7 else c
    dark = mix(c, '#FFFFFF', 0.5) if L < 0.35 else c
    return f'light-dark({light},{dark})'

COLW, NODEW, NODEH, DIAH, ROWH = 230, 170, 68, 84, 100
X0 = 70
def X(c): return X0 + c * COLW
NCOL = 11
COLUMNS = ["Start", "Pick", "Early triggers", "Early states", "Mid", "Pacts break", "Fallback / betrayal triggers",
           "Successors", "Late", "Late successors", "End"]

# ---------------------------------------------------------------- pools: (id, title, header colour, body light, body dark, dashed, lanes)
POOLS = [
    ('AL', "ALLIANCE &#8212; Arthas: one team until the red line &#183; Garithos: North / South from the start",
     '#2F5597', '#F2F6FC', '#1A2233', False,
     [('N', "North", 7), ('S', "South (pay the Imperial Tax in Arthas games)", 7)]),
    ('EM', "EMPIRE OF MANKIND (Garithos games)", '#B8860B', '#FFF8E1', '#2A2410', False, [('E', "Garithos' empire", 2)]),
    ('OT', "OWN TEAM", '#707070', '#F5F5F5', '#222222', False, [('O', "every box is its own team", 2)]),
    ('KA', "KALIMDOR", '#2E7D32', '#F1F8F1', '#16241A', False, [('K', "Kalimdor", 5)]),
    ('HO', "HORDE &#8212; only if founded", '#8B1A1A', '#FCF1F1', '#2A1616', True, [('H', "Horde", 2)]),
    ('OU', "OUTLAND", '#00695C', '#EEF8F6', '#13241F', False, [('I', "Fel Horde / Illidari", 3)]),
    ('SL', "SCOURGE-LEGION PACT &#8212; splits at the civil war", '#4A148C', '#F5F0FA', '#1F1630', False,
     [('SC', "Greater Scourge", 3), ('LG', "Legion &#8594; Dreadlord Insurgents (same team) + puppets", 5)]),
    ('OG', "OLD GODS", '#6A2C4A', '#F8F0F4', '#261820', False, [('OG', "Old Gods", 4)]),
    ('VI', "VICTOR'S TEAM &#8212; joins whoever dealt the blow", '#455A64', '#F1F4F5', '#1C2326', False,
     [('V', "victor's team", 2)]),
]

# ---------------------------------------------------------------- condition strips
STRIPS = {'A': ('ARTHAS GAMES', '#C0C0C0'), 'G': ('GARITHOS GAMES', '#FFD54F'),
          'S': ('SENTINELS PICK', '#FFFFFF'), 'D': ('DRAENEI PICK', '#FFFFFF')}

N = []
def node(i, lane, c, r, kind, slot, label, strip=None):
    N.append(dict(id=i, lane=lane, c=c, r=r, kind=kind, slot=slot, label=label, strip=strip))

# North
node('lor', 'N', 0, 0, 'faction', 'P10', "Lordaeron")
node('d1', 'N', 1, 0, 'decision', 'P10', "<b>D1</b><br>Arthas or Garithos?")
node('arthas', 'N', 2, 0, 'faction', 'P10', "Lordaeron<br><i>Arthas leads</i>", 'A')
node('culling', 'N', 3, 0, 'event', 'P4', "The Culling<br><i>Stratholme falls, no Line of Succession</i>", 'A')
node('lorhq', 'N', 4, 0, 'event', 'P10', "Lordaeron's HQ falls", 'A')
node('d14', 'N', 6, 0, 'decision', 'P10', "<b>D14</b><br>Scarlet restart", 'A')
node('scarlet', 'N', 7, 0, 'fb', 'P10', "Scarlet Crusade", 'A')
node('succ', 'N', 3, 1, 'event', 'P10', "Line of Succession<br><i>Arthas lvl 10 + Howling Fjord</i>", 'A')
node('kingarthas', 'N', 4, 1, 'faction', 'P10', "King Arthas", 'A')
node('kom', 'N', 6, 1, 'crown', 'P10', "&#128081; Kingdom of Man<br><i>both crowns + both capitals</i>")
node('garithos', 'N', 2, 2, 'faction', 'P10', "Garithos' Command<br><i>no fallback: the gamble</i>", 'G')
node('shores', 'N', 3, 2, 'event', 'P10', "Shores of Northrend<br><i>Mal'Ganis slain, Frostmourne refused</i>", 'G')
node('empowered', 'N', 6, 2, 'event', 'P10', "Empowered Garithos<br>reaches Northrend", 'G')
node('treason', 'N', 3, 3, 'decision', 'P10', "<b>D10</b><br>Treason!", 'G')
node('coron', 'N', 4, 3, 'event', 'P10', "Coronation<br><i>capital lost + retaken: King Arthas vs murlocs</i>", 'G')
node('dal', 'N', 0, 4, 'faction', 'P8', "Dalaran / Gilneas<br><i>(pick)</i>")
node('vcf', 'N', 4, 4, 'event', 'P8', "Violet Citadel falls")
node('d6', 'N', 6, 4, 'decision', 'P8', "<b>D6</b><br>Jaina's choice")
node('jstay', 'N', 7, 4, 'fb', 'P8', "Jaina's Sanctum<br><i>Stay</i>")
node('d7', 'N', 6, 5, 'decision', 'P8', "<b>D7</b><br>The Gates Close")
node('quel', 'N', 0, 6, 'faction', 'P3', "Quel'thalas")
node('sylslain', 'N', 1, 6, 'event', 'P3', "Sylvanas slain<br>by the Scourge")
node('d3', 'N', 2, 6, 'decision', 'P3', "<b>D3</b><br>Keep or defect?")
node('botnote', 'N', 3, 6, 'note', None, "Defect: the human plays the Forsaken in P3; Quel'thalas moves to a free slot as a bot")
node('sunwell', 'N', 6, 6, 'event', 'P3', "Sunwell destroyed<br><i>Quel'thalas HQ</i>")

# South
node('sw', 'S', 0, 0, 'faction', 'P2', "Stormwind")
node('defias', 'S', 1, 0, 'note', None, "Defias raids near Stormwind's CPs<br><i>no XP</i>", 'A')
node('swhq', 'S', 6, 0, 'event', 'P2', "Stormwind's HQ falls")
node('d9a', 'S', 7, 0, 'decision', 'P2', "<b>D9</b><br>South or North?")
node('valiance', 'S', 8, 0, 'fb', 'P2', "Valiance Expedition<br><i>team by D9</i>")
node('if', 'S', 0, 1, 'faction', 'P5', "Ironforge")
node('d2', 'S', 4, 1, 'decision', None, "<b>D2</b><br>Tax vote", 'A')
node('iffalls', 'S', 6, 1, 'event', 'P5', "Ironforge falls")
node('d9b', 'S', 7, 1, 'decision', 'P5', "<b>D9</b><br>South or North?")
node('nexp', 'S', 8, 1, 'fb', 'P5', "Dwarven Expedition (name open)<br><i>team by D9</i>")
node('kt', 'S', 0, 2, 'faction', 'P23', "Kul Tiras")
node('ktfalls', 'S', 6, 2, 'event', 'P23', "Kul Tiras falls<br><i>island</i>")
node('gilneas', 'S', 7, 3, 'bt', 'P8', "Gilneas behind the wall<br><i>Dalaran left behind</i>")
node('jsouth', 'S', 8, 4, 'bt', 'P8', "Jaina: Daughter of the Sea")
node('dream', 'S', 7, 5, 'fb', 'P12', "Druids: Dream trees<br><i>Twilight Grove, Seradane</i>")
node('quelsouth', 'S', 9, 6, 'bt', 'P3', "Quel'thalas joins the South<br><i>Selama ashal'anore!</i>", 'G')

# Empire of Mankind
node('emperor', 'E', 7, 0, 'faction', 'P10', "&#128081; Emperor Garithos<br><i>Empire of Mankind</i>", 'G')
node('ep8', 'E', 8, 0, 'faction', 'P8', "Dalaran / Jaina<br><i>if North or South</i>")
node('ep2', 'E', 9, 0, 'faction', 'P2', "Stormwind / Valiance")
node('ep5', 'E', 8, 1, 'faction', 'P5', "Ironforge / Dw. Expedition<br><i>dwarves welcome</i>")
node('ep23', 'E', 9, 1, 'faction', 'P23', "Kul Tiras")
node('enote', 'E', 10, 0, 'note', None, "Gilneas: Imperial Hounds<br>Helm worn: Ebon Blade join (\"Technically human.\")")

# Own team
node('walone', 'O', 4, 0, 'bt', 'P19', "Watchers: The River<br><i>alone, Tyrande left behind</i>", 'S')
node('kezan', 'O', 7, 0, 'fb', 'P23', "Kezan Enterprise")
node('kezgoal', 'O', 8, 0, 'crown', 'P23', "&#128081; 20,000 gold<br><i>Gallywix would be proud</i>")
node('xalturn', 'O', 9, 1, 'bt', 'P13', "Xal'atath turns<br><i>own team</i>")
node('expelled', 'O', 4, 1, 'bt', 'P3', "Expelled Quel'thalas<br><i>rogue nation</i>", 'G')
node('d16', 'O', 6, 1, 'decision', 'P3', "<b>D16</b><br>Kalimdor or South?", 'G')

# Kalimdor
node('dru', 'K', 0, 0, 'faction', 'P12', "Druids")
node('worldtree', 'K', 4, 0, 'shared', None, "SHARED &#183; first come<br><b>World Tree consumed</b><br><i>Archimonde, Garithos or Xal'atath</i>")
node('sen', 'K', 0, 1, 'faction', 'P19', "Sentinels / Draenei<br><i>(pick)</i>")
node('maiev', 'K', 3, 1, 'event', 'P19', "Maiev available", 'S')
node('d8', 'K', 4, 1, 'decision', 'P19', "<b>D8</b><br>The River", 'S')
node('senhq', 'K', 6, 1, 'event', 'P19', "HQ falls")
node('watchers', 'K', 7, 1, 'fb', 'P19', "Maiev's Watchers<br><i>Warden's Cage, + Tyrande</i>")
node('velen', 'K', 3, 2, 'event', 'P19', "Velen meets Archimonde", 'D')
node('d15', 'K', 4, 2, 'decision', 'P19', "<b>D15</b><br>The Triumvirate", 'D')
node('orc', 'K', 0, 3, 'faction', 'P1', "Frostwolf / Warsong<br><i>(pick)</i>")
node('d12', 'K', 6, 3, 'decision', 'P1', "<b>D12</b><br>Found the Horde?")
node('orchq', 'K', 6, 4, 'event', 'P1', "Orc HQ falls")
node('quelkal', 'K', 7, 3, 'bt', 'P3', "Quel'thalas joins the night elves<br><i>the Highborne come home</i>", 'G')

# Horde
node('horde', 'H', 7, 0, 'bt', 'P1', "The Horde")
node('maghar', 'H', 8, 1, 'fb', 'P7', "Mag'har<br><i>victor's team if no Horde</i>")
node('hyjal', 'H', 8, 0, 'bt', 'P8', "Jaina: Pact of Hyjal<br><i>Kalimdor if no Horde</i>")
node('fhorde', 'H', 9, 0, 'bt', 'P3F', "Forsaken (free)<br><i>own team if no Horde</i>")
node('belfh', 'H', 9, 1, 'bt', 'P3', "Sunfury join the Horde<br><i>own team if no Horde</i>")

# Outland
node('illi', 'I', 0, 0, 'faction', 'P16', "Illidari<br><i>Magtheridon enslaved</i>")
node('bc', 'I', 6, 0, 'event', 'P16', "Black Citadel falls<br><i>Illidari HQ</i>")
node('fel', 'I', 0, 1, 'faction', 'P7', "Fel Horde")
node('felhq', 'I', 6, 1, 'event', 'P7', "Fel Horde's HQ falls")
node('sunfury', 'I', 7, 2, 'fb', 'P3', "Sunfury<br><i>Kael, Netherstorm</i>")

# Scourge
node('sc', 'SC', 0, 0, 'faction', 'P4', "Scourge")
node('scar', 'SC', 3, 0, 'faction', 'P4', "Scourge with Arthas")
node('d5', 'SC', 4, 0, 'decision', 'P4', "<b>D5</b><br>Civil war")
node('scfree', 'SC', 6, 0, 'faction', 'P4', "Free Scourge<br><i>Tithe ends; Ascension: lvl 12 + channel</i>")
node('helm', 'SC', 7, 0, 'shared', None, "SHARED &#183; first come<br><b>Helm of Domination</b><br><i>Arthas, Bolvar or Garithos</i>")
node('lkarthas', 'SC', 8, 0, 'faction', 'P4', "&#128081; Lich King Arthas<br><i>The Ascension</i>")
node('lkdies', 'SC', 9, 0, 'event', 'P4', "Lich King Arthas dies")
node('fpup', 'SC', 2, 1, 'bt', 'P3F', "Forsaken: enslaved puppet<br><i>may stay</i>")
node('d4', 'SC', 6, 1, 'decision', 'P3F', "<b>D4</b><br>Break free?")
node('d11', 'SC', 8, 1, 'decision', 'P10', "<b>D11</b><br>Wear or shatter?", 'G')
node('lkgar', 'SC', 9, 1, 'bt', 'P10', "Lich King Garithos<br><i>undead serve the Empire</i>", 'G')
node('shatter', 'SC', 10, 1, 'event', 'P10', "Helm shattered<br><i>Scourge undead die</i>", 'G')
node('lkbolvar', 'SC', 8, 2, 'bt', 'P2', "&#128081; Lich King Bolvar")

# Legion
node('leg', 'LG', 0, 0, 'faction', 'P24', "Burning Legion")
node('summon', 'LG', 1, 0, 'event', 'P24', "Legion summoned")
node('legarch', 'LG', 2, 0, 'faction', 'P24', "Legion with Archimonde")
node('argus', 'LG', 3, 0, 'note', None, "Argus portal one-way; a channel reverses it")
node('archdies', 'LG', 6, 0, 'event', 'P24', "Archimonde dies<br><i>or all Argus CPs lost</i>")
node('insurg', 'LG', 7, 0, 'fb', 'P24', "Dreadlord Insurgents<br><i>same team, revive at the lair</i>")
node('d13', 'LG', 6, 1, 'decision', 'P24', "<b>D13</b><br>Grand Crusader", 'A')
node('cscar', 'LG', 7, 1, 'bt', 'P10', "Corrupted Scarlet Crusade", 'A')
node('jdemon', 'LG', 7, 2, 'bt', 'P8', "Jaina is a Demon", 'G')
node('manari', 'LG', 7, 3, 'bt', 'P19', "Man'ari draenei", 'D')
node('felleg', 'LG', 7, 4, 'bt', 'P7', "Fel Horde: Magtheridon unleashed<br><i>if the Legion beats the Illidari</i>")

# Old Gods
node('sky', 'OG', 0, 0, 'faction', 'P9', "Skywall")
node('skyhq', 'OG', 6, 0, 'event', 'P9', "Skywall falls")
node('be', 'OG', 0, 1, 'faction', 'P13', "Black Empire")
node('behq', 'OG', 6, 1, 'event', 'P13', "Black Empire falls")
node('aq', 'OG', 0, 2, 'faction', 'P17', "Ahn'qiraj")
node('aqhq', 'OG', 6, 2, 'event', 'P17', "Ahn'qiraj falls")
node('twilight', 'OG', 7, 3, 'fb', 'P1', "Twilight's Hammer<br><i>Grim Batol</i>")
node('deathwing', 'OG', 8, 3, 'crown', 'P1', "&#128081; The Hour of Twilight<br><i>Deathwing returns</i>")

# Victor's team
node('earthen', 'V', 7, 0, 'fb', 'P9', "The Earthen Ring<br><i>(open)</i>")
node('xal', 'V', 8, 0, 'fb', 'P13', "Xal'atath<br><i>Blade of the Black Empire (open)</i>")
node('brood', 'V', 9, 0, 'fb', 'P17', "Brood of Nozdormu")
node('akama', 'V', 7, 1, 'fb', 'P16', "Akama's Ashtongue<br><i>draenei buildings</i>")
node('d17', 'V', 8, 1, 'decision', 'P13', "<b>D17</b><br>The Blade turns")
node('ebon', 'V', 10, 0, 'fb', 'P4', "Knights of the Ebon Blade")

# ---------------------------------------------------------------- edges
E = []
def edge(s, t, slot, typ='auto', label=''):
    E.append(dict(s=s, t=t, slot=slot, typ=typ, label=label))

# Lordaeron
edge('lor', 'd1', 'P10')
edge('d1', 'arthas', 'P10', 'choice', 'Arthas')
edge('d1', 'garithos', 'P10', 'choice', 'Garithos')
edge('arthas', 'culling', 'P10')
edge('culling', 'scar', 'P4', 'auto', 'Arthas joins the Scourge')
edge('arthas', 'succ', 'P10')
edge('succ', 'kingarthas', 'P10')
edge('kingarthas', 'kom', 'P10', 'goal')
edge('arthas', 'lorhq', 'P10')
edge('lorhq', 'diss', 'P10')
edge('arthas', 'd14', 'P10', 'choice', 'after Tyr\'s Hand')
edge('d14', 'scarlet', 'P10', 'auto', 'fallback')
edge('scarlet', 'd13', 'P10', 'cond')
edge('garithos', 'shores', 'P10')
edge('shores', 'worldtree', 'P10', 'choice', 'Well of Eternity')
edge('worldtree', 'empowered', 'P10', 'auto', 'empowered')
edge('empowered', 'emperor', 'P10', 'auto', 'Empire forms')
for t, s in [('ep8', 'P8'), ('ep2', 'P2'), ('ep5', 'P5'), ('ep23', 'P23')]:
    edge('empowered', t, s)
edge('empowered', 'expelled', 'P3', 'auto', 'non-humans out')
edge('emperor', 'helm', 'P10', 'auto', 'victor over the Scourge')
edge('garithos', 'treason', 'P10', 'choice', '> 2 CPs lost')
edge('treason', 'expelled', 'P3', 'auto', 'forced')
edge('garithos', 'coron', 'P10')
edge('coron', 'scar', 'P4', 'cond', 'Lordaeron eliminated: Scourge slays the king')
edge('expelled', 'd16', 'P3')
edge('d16', 'quelsouth', 'P3', 'choice', 'Lordaeron gone + South exists')
edge('d16', 'quelkal', 'P3', 'choice', 'any time')
edge('expelled', 'sunwell', 'P3', 'cond', 'HQ falls')
# Dalaran / Gilneas
edge('dal', 'vcf', 'P8')
edge('vcf', 'd6', 'P8')
edge('d6', 'jstay', 'P8', 'choice', 'Stay')
edge('d6', 'hyjal', 'P8', 'choice', 'Pact of Hyjal')
edge('d6', 'jsouth', 'P8', 'choice', 'South exists')
edge('d6', 'jdemon', 'P8', 'choice', 'Garithos games')
edge('dal', 'd7', 'P8', 'choice')
edge('d7', 'gilneas', 'P8', 'auto', 'no fallback')
# Quel'thalas
edge('quel', 'sylslain', 'P3')
edge('sylslain', 'd3', 'P3')
edge('d3', 'quel', 'P3', 'choice', 'keep: Scourge gets Sylvanas')
edge('d3', 'fpup', 'P3F', 'choice', 'defect')
edge('quel', 'sunwell', 'P3')
edge('sunwell', 'sunfury', 'P3', 'auto', 'fallback')
edge('sunfury', 'belfh', 'P3', 'cond', 'Black Citadel falls')
edge('fpup', 'd4', 'P3F')
edge('d4', 'fhorde', 'P3F', 'choice', 'from the civil war on')
edge('lkdies', 'fhorde', 'P3F', 'auto', 'latest: automatic')
# South
edge('sw', 'defias', 'P2', 'cond')
for s, slot in [('sw', 'P2'), ('if', 'P5'), ('kt', 'P23')]:
    edge(s, 'd2', slot, 'choice')
edge('d2', 'diss', None, 'auto', 'majority refuses')
edge('sw', 'swhq', 'P2')
edge('swhq', 'd9a', 'P2')
edge('d9a', 'valiance', 'P2', 'auto', 'fallback')
edge('valiance', 'helm', 'P2', 'choice', 'Bolvar claims the Helm')
edge('if', 'iffalls', 'P5')
edge('iffalls', 'd9b', 'P5')
edge('d9b', 'nexp', 'P5', 'auto', 'fallback')
edge('kt', 'ktfalls', 'P23')
edge('ktfalls', 'kezan', 'P23', 'auto', 'fallback')
edge('kezan', 'kezgoal', 'P23', 'goal')
# Kalimdor
edge('dru', 'worldtree', 'P12', 'auto', 'HQ = Nordrassil')
edge('worldtree', 'dream', 'P12', 'auto', 'Druids\' fallback')
edge('worldtree', 'diss', 'P24', 'auto', 'Archimonde, Arthas games')
edge('legarch', 'worldtree', 'P24', 'choice', 'channel')
edge('sen', 'maiev', 'P19')
edge('maiev', 'd8', 'P19')
edge('d8', 'walone', 'P19', 'choice', 'The River')
edge('sen', 'senhq', 'P19')
edge('senhq', 'watchers', 'P19', 'auto', 'fallback')
edge('sen', 'velen', 'P19')
edge('velen', 'd15', 'P19')
edge('d15', 'manari', 'P19', 'choice', 'accept the offer')
edge('orc', 'd12', 'P1', 'choice')
edge('d12', 'horde', 'P1', 'auto', 'founded')
edge('archdies', 'd12', 'P24', 'cond', 'enables')
edge('orc', 'orchq', 'P1')
edge('orchq', 'twilight', 'P1', 'auto', 'fallback')
edge('twilight', 'deathwing', 'P1', 'goal')
# Outland
edge('illi', 'bc', 'P16')
edge('bc', 'akama', 'P16', 'auto', 'fallback, joins victor')
edge('bc', 'felleg', 'P7', 'cond', 'Legion is victor')
edge('fel', 'felhq', 'P7')
edge('felhq', 'maghar', 'P7', 'auto', 'fallback')
edge('felleg', 'maghar', 'P7', 'cond', 'HQ falls')
# Scourge
edge('sc', 'scar', 'P4')
edge('scar', 'd5', 'P4', 'choice', 'from turn X')
edge('d5', 'scfree', 'P4')
edge('scfree', 'helm', 'P4', 'choice', 'Ascension')
edge('helm', 'lkarthas', 'P4')
edge('lkarthas', 'diss', 'P4', 'auto', 'Arthas games')
edge('lkarthas', 'lkdies', 'P4')
edge('lkdies', 'ebon', 'P4', 'auto', 'joins victor')
edge('lkdies', 'helm', 'P4', 'cond', 'Helm free again')
edge('helm', 'd11', 'P10', 'auto', 'Garithos')
edge('d11', 'lkgar', 'P10', 'choice', 'wear')
edge('d11', 'shatter', 'P10', 'choice', 'shatter')
edge('helm', 'lkbolvar', 'P2', 'auto', 'Bolvar')
# Legion
edge('leg', 'summon', 'P24')
edge('summon', 'legarch', 'P24')
edge('legarch', 'archdies', 'P24')
edge('archdies', 'insurg', 'P24', 'auto', 'fallback, same team')
edge('legarch', 'd13', 'P24', 'choice', 'after the civil war')
edge('d13', 'cscar', 'P10', 'auto', 'forced')
# Old Gods
edge('sky', 'skyhq', 'P9')
edge('skyhq', 'earthen', 'P9', 'auto', 'joins victor')
edge('be', 'behq', 'P13')
edge('behq', 'xal', 'P13', 'auto', 'joins victor')
edge('xal', 'd17', 'P13')
edge('d17', 'worldtree', 'P13', 'choice', 'channel')
edge('worldtree', 'xalturn', 'P13', 'auto', 'betrayal')
edge('aq', 'aqhq', 'P17')
edge('aqhq', 'brood', 'P17', 'auto', 'joins victor')

# ---------------------------------------------------------------- build
cells = []
def esc(v): return escape(v, {'"': '&quot;'})
def cell(i, value, style, parent, x, y, w, h):
    cells.append(f'<mxCell id="{i}" value="{esc(value)}" style="{style}" parent="{parent}" vertex="1">'
                 f'<mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry"/></mxCell>')

for c, name in enumerate(COLUMNS):
    cell(f'col{c}', f'<b>{c}. {name}</b>', 'rounded=1;whiteSpace=wrap;html=1;fillColor=light-dark(#F5F5F5,#2A2A2A);fontColor=light-dark(#333333,#DDDDDD);strokeColor=#999999;fontSize=11;',
         '1', X(c), 0, NODEW, 36)
cell('timearrow', 'time, roughly &#8594;', 'text;html=1;align=left;verticalAlign=middle;fontSize=11;fontStyle=2;', '1', X(0), 38, 300, 20)

lane_pos, pool_geo = {}, {}
y = 70
PW = X(NCOL - 1) + NODEW + 30
for pid, title, head, bl, bd, dashed, lanes in POOLS:
    ph = sum(10 + rows * ROWH for _, _, rows in lanes)
    st = (f'swimlane;horizontal=0;html=1;startSize=30;fillColor={head};fontColor=#FFFFFF;'
          f'swimlaneFillColor=light-dark({bl},{bd});strokeColor={head};fontStyle=1;fontSize=12;')
    if dashed: st += 'dashed=1;'
    cell(f'pool_{pid}', title, st, '1', 0, y, PW, ph)
    pool_geo[pid] = (y, ph)
    ly = 0
    for lid, ltitle, rows in lanes:
        lh = 10 + rows * ROWH
        lst = (f'swimlane;horizontal=0;html=1;startSize=30;fillColor=none;swimlaneFillColor=none;'
               f'strokeColor={head};fontSize=10;fontStyle=2;')
        if dashed: lst += 'dashed=1;'
        cell(f'lane_{lid}', ltitle, lst, f'pool_{pid}', 30, ly, PW - 30, lh)
        lane_pos[lid] = (y + ly, lh)
        ly += lh
    y += ph + 40

def strip_html(code):
    if not code: return ''
    t, c = STRIPS[code]
    return (f'<div style="background:{c};color:#000000;font-size:8px;font-weight:bold;letter-spacing:1px;'
            f'border:1px solid #666666;margin-bottom:2px;">{t}</div>')

def nstyle(kind, slot):
    base = 'whiteSpace=wrap;html=1;fontSize=11;'
    if kind == 'faction':
        return base + f'rounded=1;fillColor={fill(slot)};fontColor={font(slot)};strokeColor={stroke(slot)};'
    if kind == 'fb':
        return base + f'shape=ext;double=1;rounded=1;fillColor={fill(slot)};fontColor={font(slot)};strokeColor=light-dark(#000000,#FFFFFF);'
    if kind == 'bt':
        return base + f'rounded=1;fillColor={fill(slot)};fontColor={font(slot)};strokeColor=light-dark(#000000,#FFFFFF);strokeWidth=2;dashed=1;'
    if kind == 'event':
        sc = stroke(slot) if slot else '#666666'
        return base + f'shape=hexagon;perimeter=hexagonPerimeter2;fixedSize=1;size=12;fillColor=#E6E6E6;fontColor=#000000;strokeColor={sc};strokeWidth=2;'
    if kind == 'decision':
        sc = stroke(slot) if slot else BREAK
        return base + f'rhombus;fillColor=#FFFFFF;fontColor=#000000;strokeColor={sc};strokeWidth=3;'
    if kind == 'note':
        return base + 'shape=note;size=12;fillColor=#FFF2CC;fontColor=#000000;strokeColor=#B3A100;fontSize=10;'
    if kind == 'shared':
        return base + 'shape=hexagon;perimeter=hexagonPerimeter2;fixedSize=1;size=14;fillColor=#FFD966;fontColor=#000000;strokeColor=#B8860B;strokeWidth=4;fontSize=10;'
    if kind == 'crown':
        return base + 'rounded=1;arcSize=40;fillColor=#FFF2B3;fontColor=#000000;strokeColor=#B8860B;strokeWidth=2;fontSize=10;'

def nlabel(n):
    lab = n['label']
    if n['kind'] in ('faction', 'fb', 'bt') and n['slot']:
        p = n['slot'].replace('P3F', 'P3')
        lab = f'<b>{p}</b> {lab}'
    return strip_html(n['strip']) + lab

seen = set()
for n in N:
    k = (n['lane'], n['c'], n['r']); assert k not in seen, k; seen.add(k)
    h = DIAH if n['kind'] == 'decision' else NODEH
    lx = X(n['c']) - 30
    ly = 10 + n['r'] * ROWH + (0 if n['kind'] == 'decision' else (DIAH - NODEH) // 2)
    cell(n['id'], nlabel(n), nstyle(n['kind'], n['slot']), f'lane_{n["lane"]}', lx, ly, NODEW, h)

def break_line(pid, label, hexid=None, hexlabel=None):
    py, ph = pool_geo[pid]
    lx = X(5) + NODEW // 2 - 5
    cell(f'line_{pid}', '', f'line;strokeWidth=4;direction=south;html=1;strokeColor={BREAK};dashed=1;', f'pool_{pid}', lx, 0, 10, ph)
    cell(f'linelabel_{pid}', label, f'text;html=1;align=center;fontSize=10;fontColor={BREAK};fontStyle=1;labelBackgroundColor=default;whiteSpace=wrap;',
         f'pool_{pid}', X(5) - 25, 2, 220, 34)
    if hexid:
        cell(hexid, hexlabel, f'shape=hexagon;perimeter=hexagonPerimeter2;fixedSize=1;size=14;whiteSpace=wrap;html=1;fillColor={BREAK};fontColor=#FFFFFF;strokeColor=#990000;strokeWidth=2;fontStyle=1;fontSize=11;',
             f'pool_{pid}', X(5) - 5, ph // 2 - 45, NODEW + 10, 90)

break_line('AL', 'Arthas games: &#9664; Great Alliance | North / South &#9654;', 'diss',
           strip_html('A') + 'ALLIANCE DISSOLUTION<br><span style="font-weight:normal"><i>first trigger wins</i></span>')
break_line('SL', '&#9664; pact | split &#9654; (civil war)')

# ---------------------------------------------------------------- routing
RECT = {}
for n in N:
    ly0, _ = lane_pos[n['lane']]
    h = DIAH if n['kind'] == 'decision' else NODEH
    yy = ly0 + 10 + n['r'] * ROWH + (0 if n['kind'] == 'decision' else (DIAH - NODEH) // 2)
    RECT[n['id']] = (X(n['c']), yy, NODEW, h)
py, ph = pool_geo['AL']
RECT['diss'] = (X(5) - 5, py + ph // 2 - 45, NODEW + 10, 90)
OBST = dict(RECT)
VCOR = [X(c) + NODEW + 15 for c in range(0, NCOL)]
HCOR = []
for lid, (ly0, lh) in lane_pos.items():
    rows = (lh - 10) // ROWH
    HCOR += [ly0 + r * ROWH + 5 for r in range(0, rows + 1)]
for pid, (py0, ph0) in pool_geo.items():
    HCOR.append(py0 + ph0 + 20)
HCOR = sorted(set(HCOR))

def seg_hits(p, q, skip):
    (x1, y1), (x2, y2) = p, q
    for k, (rx, ry, rw, rh) in OBST.items():
        if k in skip: continue
        m = 3
        if y1 == y2:
            if ry - m < y1 < ry + rh + m and max(x1, x2) > rx - m and min(x1, x2) < rx + rw + m: return True
        else:
            if rx - m < x1 < rx + rw + m and max(y1, y2) > ry - m and min(y1, y2) < ry + rh + m: return True
    return False
def path_ok(p, skip): return all(not seg_hits(p[i], p[i + 1], skip) for i in range(len(p) - 1))
def plen(p): return sum(abs(p[i][0] - p[i + 1][0]) + abs(p[i][1] - p[i + 1][1]) for i in range(len(p) - 1))

def route(sid, tid):
    sx, sy, sw_, sh = RECT[sid]; tx, ty, tw, th = RECT[tid]
    scx, scy, tcx, tcy = sx + sw_ / 2, sy + sh / 2, tx + tw / 2, ty + th / 2
    skip = {sid, tid}; cands = []
    if abs(scy - tcy) < 1:
        p = [(sx + sw_, scy), (tx, tcy)] if tx > sx else [(sx, scy), (tx + tw, tcy)]
        if path_ok(p, skip): cands.append((plen(p), p))
    if abs(scx - tcx) < 1:
        p = [(scx, sy + sh), (tcx, ty)] if ty > sy else [(scx, sy), (tcx, ty + th)]
        if path_ok(p, skip): cands.append((plen(p), p))
    for gx in VCOR:
        if sx <= gx <= sx + sw_ or tx <= gx <= tx + tw: continue
        ex = sx + sw_ if gx > sx else sx; en = tx if gx < tx else tx + tw
        p = [(ex, scy), (gx, scy), (gx, tcy), (en, tcy)]
        if path_ok(p, skip): cands.append((plen(p) + 40, p))
    for hy in HCOR:
        if sy <= hy <= sy + sh or ty <= hy <= ty + th: continue
        ey = sy + sh if hy > sy else sy; ny = ty if hy < ty else ty + th
        p = [(scx, ey), (scx, hy), (tcx, hy), (tcx, ny)]
        if path_ok(p, skip): cands.append((plen(p) + 40, p))
    if not cands:
        for g1 in VCOR:
            if sx <= g1 <= sx + sw_: continue
            ex = sx + sw_ if g1 > sx else sx
            for hy in HCOR:
                for g2 in VCOR:
                    if tx <= g2 <= tx + tw: continue
                    en = tx if g2 < tx else tx + tw
                    p = [(ex, scy), (g1, scy), (g1, hy), (g2, hy), (g2, tcy), (en, tcy)]
                    if path_ok(p, skip): cands.append((plen(p) + 120, p))
    if not cands: return None
    return min(cands, key=lambda c: c[0])[1]

USED_V, USED_H = {}, {}
def offset_for(store, key, a, b):
    cnt = sum(1 for (c, d) in store.get(key, []) if max(a, b) > min(c, d) and min(a, b) < max(c, d))
    return [0, 6, -6, 12, -12, 3, -3, 9, -9][min(cnt, 8)]
def apply_offsets(p):
    q = [list(pt) for pt in p]
    for i in range(1, len(q) - 2):
        a, b = q[i], q[i + 1]
        if a[0] == b[0]:
            off = offset_for(USED_V, a[0], a[1], b[1]); USED_V.setdefault(a[0], []).append((a[1], b[1])); a[0] += off; b[0] += off
        elif a[1] == b[1]:
            off = offset_for(USED_H, a[1], a[0], b[0]); USED_H.setdefault(a[1], []).append((a[0], b[0])); a[1] += off; b[1] += off
    return [tuple(pt) for pt in q]
def rel(pt, rid):
    x, y, w, h = RECT[rid]; return (round((pt[0] - x) / w, 4), round((pt[1] - y) / h, 4))

fails = []
for i, e in enumerate(E):
    col = stroke(e['slot']) if e['slot'] else BREAK
    st = f'edgeStyle=orthogonalEdgeStyle;rounded=1;orthogonalLoop=1;jettySize=auto;html=1;strokeColor={col};strokeWidth=2;endArrow=block;endFill=1;fontSize=10;labelBackgroundColor=default;'
    if e['typ'] == 'choice': st += 'dashed=1;dashPattern=8 4;'
    if e['typ'] == 'cond': st += 'dashed=1;dashPattern=2 3;'
    if e['typ'] == 'goal': st = st.replace(f'strokeColor={col}', 'strokeColor=#B8860B').replace('endArrow=block', 'endArrow=none') + 'dashed=1;dashPattern=2 3;'
    p = route(e['s'], e['t']); pts = ''
    if p is None:
        fails.append((e['s'], e['t']))
    else:
        p = apply_offsets(p)
        exX, exY = rel(p[0], e['s']); enX, enY = rel(p[-1], e['t'])
        st += f'exitX={exX};exitY={exY};exitDx=0;exitDy=0;entryX={enX};entryY={enY};entryDx=0;entryDy=0;'
        if p[1:-1]:
            pts = '<Array as="points">' + ''.join(f'<mxPoint x="{a}" y="{b}"/>' for a, b in p[1:-1]) + '</Array>'
    gx = -0.55 if (p and len(p) > 2) else 0
    cells.append(f'<mxCell id="e{i}" value="{esc(e["label"])}" style="{st}" parent="1" source="{e["s"]}" target="{e["t"]}" edge="1">'
                 f'<mxGeometry x="{gx}" relative="1" as="geometry">{pts}</mxGeometry></mxCell>')
print('unrouted:', fails)

# ---------------------------------------------------------------- side panel
LX = PW + 60
ly = 70
TXT = 'text;html=1;align=left;verticalAlign=top;fontSize=11;whiteSpace=wrap;'
cell('lg_title', '<b>HOW TO READ</b>', 'text;html=1;fontSize=14;align=left;', '1', LX, ly, 300, 26); ly += 32
legend = [
    ('faction', 'P10', None, 'Faction (fill = in-game player colour)'),
    ('fb', 'P10', None, 'Fallback (double border)'),
    ('bt', 'P10', None, 'Betrayal / sub path / choice result (dashed border)'),
    ('event', 'P10', None, 'Trigger / event (border = player it belongs to)'),
    ('decision', 'P10', None, 'Decision with ID -> decision register'),
    ('shared', None, None, 'Shared box: several claimants, first come closes it'),
    ('crown', None, None, '&#128081; End goal / final quest'),
    ('note', None, None, 'Note / mechanic'),
    ('faction', 'P10', 'A', 'Condition strip: only in some games / picks'),
]
for i, (k, s, strip, text) in enumerate(legend):
    h = 56 if k == 'decision' else 42
    lab = strip_html(strip) + ('<b>P10</b>' if strip else '')
    cell(f'lg{i}', lab, nstyle(k, s), '1', LX, ly, 120, h)
    cell(f'lgt{i}', text, 'text;html=1;align=left;verticalAlign=middle;fontSize=11;whiteSpace=wrap;', '1', LX + 135, ly, 300, h)
    ly += h + 8
for i, (typ, text) in enumerate([('auto', 'automatic / forced'), ('choice', 'player choice (bots: see register)'),
                                  ('cond', 'conditional / open question'), ('goal', 'link to an end goal')]):
    st = 'endArrow=block;endFill=1;html=1;strokeWidth=2;strokeColor=light-dark(#666666,#BBBBBB);'
    if typ == 'choice': st += 'dashed=1;dashPattern=8 4;'
    if typ == 'cond': st += 'dashed=1;dashPattern=2 3;'
    if typ == 'goal': st = 'endArrow=none;html=1;strokeWidth=2;strokeColor=#B8860B;dashed=1;dashPattern=2 3;'
    cells.append(f'<mxCell id="lgl{i}" value="" style="{st}" parent="1" edge="1"><mxGeometry relative="1" as="geometry">'
                 f'<mxPoint x="{LX}" y="{ly + 12}" as="sourcePoint"/><mxPoint x="{LX + 120}" y="{ly + 12}" as="targetPoint"/></mxGeometry></mxCell>')
    cell(f'lglt{i}', text, 'text;html=1;align=left;verticalAlign=middle;fontSize=11;', '1', LX + 135, ly, 300, 24)
    ly += 30
for i, text in enumerate(['Arrow colour = the player who moves (light-dark for readability).',
                          'Pool = team. Red dashed line = a pact breaks: left one team, right two.',
                          'Dashed pool = team exists only if founded.',
                          'Columns = rough time, not exact.']):
    cell(f'lge{i}', '&#8226; ' + text, TXT, '1', LX, ly, 440, 30); ly += 30

ly += 16
rows = ''.join(f'<tr><td style="background:{fill(s)};color:{font(s)};padding:1px 6px;"><b>{s.replace("P3F","P3")}</b></td><td style="padding:1px 6px;">{n}</td><td style="padding:1px 6px;">{c}</td></tr>'
               for s, (n, c) in SLOTS.items())
free = '<br>'.join(f'{p}: {c}' for p, c in FREE)
cell('slots', f'<b>PLAYER SLOTS</b> (WL PlayerSetup)<table style="font-size:11px;border-collapse:collapse;">{rows}</table>'
     f'<br><b>Unassigned slots</b> (defections: P11, then P14, then P20)<br>{free}<br><i>Lobby names in Info.json are stale (old WL 24p layout).</i>', TXT, '1', LX, ly, 440, 560)
ly += 570

REG = [
    ('D1', 'Lordaeron: Arthas / Garithos', 'game start (pick dialog)', 'pick', 'random'),
    ('D2', 'Tax vote', 'Arthas games, from turn 10; again every 5 turns', 'majority of human payers, tie = pay', 'abstain; all-bot South: one roll'),
    ('D3', "Quel'thalas: keep / defect", 'Sylvanas slain by the Scourge', '60 s, default keep (Scourge gets Sylvanas)', 'random (needs a free slot)'),
    ('D4', 'Forsaken break free', 'from the civil war; automatic when the Lich King dies', 'Hourglass', 'one roll'),
    ('D5', 'Civil war', 'Scourge, from turn 15', 'Hourglass', 'always'),
    ('D6', "Jaina's choice", 'Violet Citadel falls', '60 s, default Stay', 'random among available'),
    ('D7', 'The Gates Close', 'no Great Alliance + plague + Genn alive', 'Hourglass', 'one roll'),
    ('D8', 'The River', 'Maiev available (Sentinels pick)', 'Hourglass', 'one roll'),
    ('D9', 'South fallback goes North', "Ironforge's / Stormwind's fallback", 'choice', 'North if Arthas is loyal, or Garithos game'),
    ('D10', 'Treason!', 'Garithos games, Lordaeron lost > 2 CPs', 'Hourglass', 'always'),
    ('D11', 'Wear / shatter the Helm', 'Garithos claims the Helm', 'choice', 'random'),
    ('D12', 'Found the Horde', 'Archimonde dies', 'Hourglass', 'one roll'),
    ('D13', 'Grand Crusader', 'Legion, after the civil war, Arthas games', 'Hourglass', 'always'),
    ('D14', 'Scarlet restart', "Tyr's Hand quest done (WL research)", 'research', 'at HQ fall'),
    ('D15', 'The Triumvirate', 'Draenei pick, Velen meets Archimonde; open 3 min', 'Hourglass', 'one roll'),
    ('D16', 'Rogue Quel: Kalimdor or South', 'Garithos games; Kalimdor any time, South once Lordaeron is gone', 'Hourglass', 'random among available'),
    ('D17', 'The Blade turns', "Xal'atath on the victor's team, World Tree not yet consumed", 'channel at the World Tree', 'one roll'),
]
rows = ''.join(f'<tr><td style="padding:1px 4px;vertical-align:top;"><b>{a}</b></td><td style="padding:1px 4px;vertical-align:top;">{b}</td>'
               f'<td style="padding:1px 4px;vertical-align:top;">{c}</td><td style="padding:1px 4px;vertical-align:top;">{d}</td>'
               f'<td style="padding:1px 4px;vertical-align:top;">{e}</td></tr>' for a, b, c, d, e in REG)
cell('register', '<b>DECISION REGISTER</b><table style="font-size:10px;border-collapse:collapse;" border="1">'
     '<tr><th>ID</th><th>Decision</th><th>Appears when</th><th>Human</th><th>Bots</th></tr>' + rows + '</table>'
     '<br>Bot rules: 1) random &#183; 2) decide the moment it appears &#183; 3) single-option paths: one roll, take or never',
     TXT, '1', LX, ly, 640, 800)
ly += 820

GOALS = [
    ('P10 Arthas', 'Kingdom of Man (WL)', 'no'), ('P10 Garithos', 'Empire of Mankind', 'yes'),
    ('P4 Scourge', 'The Ascension (WL)', 'no'), ('P2 Valiance', 'Lich King Bolvar', 'no'),
    ('P1 Twilight\'s Hammer', 'The Hour of Twilight: Deathwing returns', 'no'),
    ('P23 Kezan', 'acquire 20,000 gold', 'no'),
    ('all others', 'open', ''),
]
rows = ''.join(f'<tr><td style="padding:1px 4px;">{a}</td><td style="padding:1px 4px;">{b}</td><td style="padding:1px 4px;">{c}</td></tr>' for a, b, c in GOALS)
cell('goals', '<b>&#128081; END GOALS</b><table style="font-size:10px;border-collapse:collapse;" border="1">'
     '<tr><th>Faction</th><th>Final quest</th><th>Changes alliances</th></tr>' + rows + '</table>', TXT, '1', LX, ly, 440, 170)
ly += 190

OPEN = ["Fallbacks of Skywall / Black Empire not decided: Earthen Ring / Xal'atath or Ramkahen / Titan Watchers",
        "Brood of Nozdormu (Ahn'qiraj): assets?",
        "WL defeats only empty slots WITH a faction: preplaced units of P6, P15, P18 stay -- remove them?",
        "Rename lobby slots in Info.json to the Shattered Legacies layout.",
        "End goals for the remaining factions."]
cell('open', '<b>OPEN</b><br>' + '<br>'.join('&#8226; ' + o for o in OPEN), TXT + 'strokeColor=#999999;rounded=1;spacing=8;',
     '1', LX, ly, 440, 200)
ly += 220
page_h = max(y, ly)

# ---------------------------------------------------------------- assemble
DEFAULT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', 'src', 'WarcraftLegacies.Source',
                       'ShatteredLegacies', 'Timelines', 'timeline-overview.drawio')
OLD = sys.argv[1] if len(sys.argv) > 1 else DEFAULT
OUT = sys.argv[2] if len(sys.argv) > 2 else DEFAULT
old_xml, pages = '', 1
if os.path.exists(OLD):
    diagrams = ET.parse(OLD).getroot().findall('diagram')
    # an earlier output carries the old draft as its own page; a plain draft file has it as its first page
    old = next((d for d in diagrams if d.get('name') == 'test-4 (old draft)'), None)
    if old is None and not any(d.get('name') == 'Timeline overview' for d in diagrams):
        old = diagrams[0]
    if old is not None:
        old.set('name', 'test-4 (old draft)')
        old_xml, pages = ET.tostring(old, encoding='unicode'), 2
model = (f'<mxGraphModel dx="1600" dy="1000" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" '
         f'page="1" pageScale="1" pageWidth="{LX + 700}" pageHeight="{page_h}" math="0" shadow="0"><root>'
         '<mxCell id="0"/><mxCell id="1" parent="0"/>' + ''.join(cells) + '</root></mxGraphModel>')
out = (f'<mxfile host="app.diagrams.net" pages="{pages}"><diagram name="Timeline overview" id="{uuid.uuid4().hex[:20]}">'
       f'{model}</diagram>{old_xml}</mxfile>')
ET.fromstring(out)
open(OUT, 'w').write(out)
ids = {n['id'] for n in N} | {'diss'}
for e in E: assert e['s'] in ids and e['t'] in ids, e
print('nodes', len(N), 'edges', len(E), 'page', LX + 700, page_h)

# extract_use_ability_targets.awk -- recovers who the live server cast each ability on, from the packet
# captures.
#
# WHY THIS EXISTS. The emulator used to decide whether a cast lands on its caster from the ability's
# range: 0 meant the caster. The client carries a TargetType per ability (data/bin/abilityexport.bin),
# and the live 1.4.8 server wrote the target it chose into every cast start. Across the 1,027 logs,
# abilities of client TargetType 0 or 3 were cast on their caster (772 of 773) and TargetType 1
# abilities on another unit (844 of 844), 163 of those with a client range of 0. That is the evidence
# behind migration 09 and AbilityInfo.TargetsCaster.
#
# USAGE
#   cd "D:/Repos/Shmerrick/WAR-RE-Toolkit/libs/protocolservices/Packet Logs"
#   zcat *.gz | gawk -f .../extract_use_ability_targets.awk | sort | uniq -c > use_ability_targets.tsv
#
# OUTPUT is one line per cast or channel start: entry <tab> kind <tab> target, where kind is 1 (cast) or
# 3 (channel) and target is `self` (the target oid is the caster's), `other`, or `none` (oid 0, which
# ground-targeted abilities send). Object ids are per session, so only the relation is kept.
#
# LAYOUT is F_USE_ABILITY (0xDA) as tools/captures/extract_use_ability.awk documents it: ability entry at
# frame bytes 5-6, caster oid 7-8, EffectID 9-10, target oid 11-12, kind 13.
/packet : \(0x/ {
  if (inpkt && n >= 19) emit()
  inpkt = ($0 ~ /^\[Server\] packet : \(0xDA\)/); n = 0; next
}
!inpkt { next }
/^-{10,}/ {
  if (n >= 19) emit()
  inpkt = 0; n = 0; next
}
/^\|/ {
  line = $0
  # the column-legend line ends with the literal 0123456789ABCDEF pane
  if (line ~ /0123456789ABCDEF\|$/) next
  if (line ~ /^\|-+/) next
  # hex pane is between the first and second '|'
  p = index(substr(line,2), "|")
  if (p <= 0) next
  hex = substr(line, 2, p-1)
  cnt = split(hex, tok, " ")
  for (i = 1; i <= cnt; ++i)
    if (tok[i] ~ /^[0-9A-Fa-f][0-9A-Fa-f]$/) b[n++] = strtonum("0x" tok[i])
}
function emit(   caster, target, rel) {
  if (b[2] != 218) return
  if (b[13] != 1 && b[13] != 3) return
  caster = b[7]*256 + b[8]; target = b[11]*256 + b[12]
  rel = (target == 0) ? "none" : ((target == caster) ? "self" : "other")
  print (b[5]*256 + b[6]) "\t" b[13] "\t" rel
}

# extract_use_ability.awk -- recovers ability EffectID, cast time and channel length from the live-server
# packet captures.
#
# WHY THIS EXISTS. When a cast starts, the server writes the ability's EffectID -- the visual the
# client plays -- and its cast time into F_USE_ABILITY (0xDA); a channel start carries the channel's
# length in the same place. WorldServer does it in AbilityProcessor.cs:427-443 and
# NewChannelHandler.cs:96-111, and the real 1.4.8 server did too, so the captures hold the live
# values. They settle what the client files leave open, and they check the client export itself:
# across the 1,027 logs (722,775 server frames), the live EffectID matched data/bin/abilityexport.bin
# on all 1,435 abilities with a clean cast-start sample, and the live cast time on 1,298 of 1,312.
#
# USAGE
#   cd "D:/Repos/Shmerrick/WAR-RE-Toolkit/libs/protocolservices/Packet Logs"
#   zcat *.gz | gawk -f .../extract_use_ability.awk | sort | uniq -c > use_ability.tsv
#
# OUTPUT is one line per server frame: entry <tab> effectid <tab> kind <tab> ms, where ms is the cast
# time for kind 1, the channel length for kind 3, and -1 otherwise. `uniq -c` turns that into frame
# counts per distinct value.
#
# LAYOUT. A frame is 2 bytes of size, 1 byte of opcode, then the payload, all big-endian: ability
# entry at frame bytes 5-6, caster oid 7-8, EffectID 9-10, target oid 11-12, kind 13, origin 14, and
# a uint32 of milliseconds at 15-18. Calibrated on a Serpent Passage frame,
#   00 14 DA 00 00 3A 89 02 5A 0E A0 02 5A 01 01 00 00 0B B8 00 00 00 00
# entry 14985, caster and target 602, EffectID 3744, kind 1, cast 3000 -- which is the emulator's own
# writer, field for field.
#
# READING IT. Kind 1 starts a cast and kind 3 a channel; take EffectID and timings from those. Kind 2
# frames often carry EffectID 0 for abilities whose kind-1 frames carry the real one, so a 0 there
# proves nothing. The timings are what the live server applied, so a caster's own reductions can
# lower them: compare the dominant value per entry, never a single frame.
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
function emit(   kind) {
  if (b[2] != 218) return
  kind = b[13]
  print (b[5]*256 + b[6]) "\t" (b[9]*256 + b[10]) "\t" kind "\t" ((kind == 1 || kind == 3) ? ((b[15]*256 + b[16])*256 + b[17])*256 + b[18] : -1)
}

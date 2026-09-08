# extract_item_names.awk -- recovers item names from the live-server packet captures.
#
# WHY THIS EXISTS. Item names are the one piece of item data with no client source: WorldServer
# writes the name into the item packet itself (World/Objects/Item.cs:512) and the real 1.4.8 server
# did too, so no file in the client holds them and item_infos.Name had never been checked against
# anything. The captures are the exception -- they are the live server's own bytes, dated 2013, and
# 24,977 F_GET_ITEM (0xAA) frames across the 1,027 logs name 1,955 distinct items.
#
# USAGE
#   cd "D:/Repos/Shmerrick/WAR-RE-Toolkit/libs/protocolservices/Packet Logs"
#   zcat *.gz | gawk -f .../extract_item_names.awk | sort -u > items.tsv
#
# OUTPUT is entry <tab> modelid <tab> name.
#
# LAYOUT. A frame is 2 bytes of size, 1 byte of opcode, then the payload. Entry is a big-endian
# uint32 at payload offset 7 (frame byte 10) and ModelId a big-endian uint16 straight after it
# (frame byte 14); the name is the first length-prefixed run of printable ASCII beyond that.
# Calibrated on entry 208084 "Red Bloodthirsty Cold One", whose 00 03 2C D4 and 24 72 sit exactly
# there and match the database's Entry and ModelId.
#
# ACCURACY, AND ITS LIMIT. 1,760 extracted entries exist in our table and 1,709 names agree --
# 97.1%, which a wrong offset could not produce. The failure mode is the alternate-appearance block:
# BuildItem writes seven zero bytes when an item has no alt appearance and a longer variable block
# when it does, so those frames shift and the read lands on a neighbouring id. They betray
# themselves two ways -- the same entry acquires several different names across frames, and the
# frame's ModelId stops matching. Filter on both before trusting a disagreement; roughly 15 of
# 1,760 fail one of them.
/packet : \(0x/ {
  inpkt = ($0 ~ /F_GET_ITEM/); n = 0; next
}
!inpkt { next }
/^-{10,}/ {
  if (n > 14) emit();
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
function emit(   entry,i,L,j,ok,s) {
  entry = b[10]*16777216 + b[11]*65536 + b[12]*256 + b[13]
  if (entry <= 0 || entry > 2100000000) return
  for (i = 14; i < n; ++i) {
    L = b[i]
    if (L < 3 || i + L >= n) continue
    ok = 1
    for (j = i+1; j <= i+L; ++j) if (b[j] < 32 || b[j] > 126) { ok = 0; break }
    if (!ok) continue
    s = ""
    for (j = i+1; j <= i+L; ++j) s = s sprintf("%c", b[j])
    # a name starts with a letter, not punctuation or a digit run
    if (s !~ /^[A-Za-z]/) continue
    print entry "\t" (b[14]*256 + b[15]) "\t" s
    return
  }
}

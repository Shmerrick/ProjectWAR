# Official Warhammer Online patch notes

Mythic's public release notes for *Warhammer Online: Age of Reckoning*, as plain text.
This is a primary historical source: when the question is "did the live game actually do X",
these outrank this emulator's code and data, and they outrank any private-server source.
They belong beside the two out-of-repo authorities named in `CLAUDE.md` — the 1.4.8 client
and the WAR-RE-Toolkit capture corpus — as a third.

## What is here

| Directory | Count | Coverage |
|:---|---:|:---|
| `.` | 31 | Game Updates **1.0 → 1.4.8**, English. Launch (26 Sep 2008) to the final patch. Complete. |
| `hotfixes/` | 22 | Hot fixes and out-of-band fixes, Apr 2011 → Nov 2012, named by publication date. |
| `localized/` | 21 | German and French editions, 1.3.6 → 1.4.8. Mythic only localized from 1.3.6 on; 1.4.8 has a German edition but no French one. |
| `pts-announcements/` | 8 | Public-test-server announcements. Not release notes — kept because distinguishing what reached live from what only reached the PTS is a recurring question, and these say what went to test and when. |

Each `_index.txt` lists version/date, title and filename.

## Provenance

* **1.4.7** also ships inside the live client at `notes/english/readme.xml`; that copy was
  converted independently and matches the archived one.
* Everything else came from the Internet Archive. The English Game Updates were enumerated
  from the archived `warhammeronline.com/patchnotes` index and cross-checked against the
  separate `news/PatchNotes` index, which lists no article the set here is missing. Hot fixes
  were enumerated from all three pages of the archived `news/HotFixes` index. Every file
  records the exact article URL it came from.
* Converted from HTML to plain text. Wording, ordering and spelling are unchanged, **including
  Mythic's own typos** — "Skaven Rat Orge", "Mourkain Templel", "changed.The new list". Do not
  silently correct these: the client strings carry the same errors, so the typos are useful for
  matching.

## Why this was added

To settle whether **Play as Skaven** shipped to live or was public-test only. It shipped:
1.4.0 introduced it gated behind repelling Thanquol's Incursion, 1.4.1 and 1.4.3 balanced and
bug-fixed it, and 1.4.5 rebuilt it as the Grovod Caverns scenario, which 1.4.7 still lists in
the permanent line-up. See `docs/handoffs/2026-09-07-four-systems.md`.

That question was not answerable from the client, the packet captures or the world database,
and would not have become answerable from any of them — which is the general argument for
keeping these in the repo.

# Official Warhammer Online patch notes

The complete run of Mythic's public patch notes, **1.0 through 1.4.8**, as plain text.
This is a primary historical source: when a question is "did the live game actually do X",
these notes outrank this emulator's code and data, and they outrank any private-server
source. They sit alongside the two out-of-repo authorities named in `CLAUDE.md`
(the 1.4.8 client and the WAR-RE-Toolkit capture corpus).

## Provenance

* **1.4.7** also ships inside the live client at `notes/english/readme.xml`; that copy was
  converted separately and matches.
* Everything else was retrieved from the Internet Archive's capture of
  `warhammeronline.com/patchnotes` (snapshot `20130510122219`), which indexes all 31
  updates. Each file names the exact article URL it came from.
* Converted from HTML to plain text. Wording, ordering and spelling are unchanged,
  including Mythic's own typos ("Skaven Rat Orge", "Mourkain Templel", "changed.The new
  list"). Do not silently correct these — they are useful for matching against client
  strings, which carry the same errors.
* German and French editions of several updates exist in the same archive and were not
  retrieved.

`_index.txt` lists version, article title and filename, one per line.

## Why this was added

Settling whether **Play as Skaven** shipped to live or was public-test-only. It shipped:
1.4.0 introduced it, 1.4.1 and 1.4.3 balanced and bug-fixed it, and 1.4.5 rebuilt it as the
Grovod Caverns scenario. See `docs/handoffs/2026-09-07-four-systems.md`. That question could
not be answered from the client, the captures or the world database, and would not have been
answerable from any of them — which is the general argument for keeping these here.

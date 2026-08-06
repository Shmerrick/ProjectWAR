-- Kill Collector dialogue text. Apply to the WORLD database.
--
-- Without this, most collectors fall back to a generated hint line, because only
-- 21 of 132 have a creature_texts row and those rows are truncated mid-sentence
-- in the published data.
--
-- Return of Reckoning's API cannot supply the text: quest.description is the
-- placeholder "Initial text for kill collectors is set in creature_texts",
-- journalEntry is empty, and tomeOfKnowledgeEntry.description is only
-- "You have met <name>". There is no creature-text query.
--
-- Source is the game client itself. Each collector's Tome of Knowledge
-- "Noteworthy Person" entry lives in the section-2 string table inside data.myp
-- (entry 8764b86c/d321fe71), indexed by tok_infos.Index. Book markup is stripped
-- and the third-person attribution line removed, leaving the NPC's own words.
--
-- Rows land in creature_texts, so CreatureService.GetCreatureText picks them up
-- with no code change. Collectors that already had a text row are left alone.

-- 85  Grimilda Mughammer
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 85, 'Smell that? That\'s what dead Squig smells like: rotten mushrooms an\' fouled breath. Once you kill one, the stench never really comes off yer weapon, does it?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=85));

-- 613  Skolm Goldskaar
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 613, 'I\'ve heard lots of flappin\' gums \'bout whether or not Snotlings are just wee Goblins that may grow to be Orcs. I\'ll leave that debate to the manling scholars. The only thing I can say for true is that Snotlings make less of a mess when I hit \'em with me hammer.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=613));

-- 2156  Barin Grimbeard
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2156, 'These Spites reek of Elf magic. Tricksters and backstabbers, the lot of \'em! Me Da always taught me that the best way to deal with Elf magic is to swing yer axe first and ask questions later les\' they try to confuse and confound you.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2156));

-- 4018  Kurgan Ironfist
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4018, 'Bats is just unnatural. Like flyin\' rats, they are. As if the bitin\' an\' the disease weren\'t enough to deal with, one got sucked into the blades of a Gyrocopter last week an\' made a chuffin\' great mess. The pilot nearly crashed the blasted thing, and it put me off me lunch. I was fishin\' grint out of me ale for an hour.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4018));

-- 5314  Estelle Meyer
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5314, 'Oh, my poor Walther! He never had a chance! At first, I thought they were cute. Their giant noses and ears and their downtrodden look made them look like sad little marionettes. \n\n"But when they started tearing Walther apart with their cruel claws and makeshift weapons, giggling all the while, I realized what they are: Boglars are monsters. No different than their larger kin.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5314));

-- 3628  Elagroth Karginson
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3628, 'The greenskins are bad enough on foot, but when they get goin\' on the backs of their Boars, it can mean real trouble. I\'ve seen the tusks on those things split an armored Dwarf open like a stuffed pig on feastday.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3628));

-- 4227  Grunlok Steelspade
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4227, 'The main thing I remember about the Boglars is their beady little eyes, utterly devoid of compassion and feeling. They\'re creatures of pure malice and ain\'t a one that\'s lived that\'s deservin\' of the time it takes to scrape what\'s left of him off yer boot when ya stomp him.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4227));

-- 4228  Lokki Redbeard
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4228, 'I care not whatever unnatural forces keep these Tortured Souls bound here. I just want \'em gone. We put the poor creatures to rest, an\' maybe we get some rest of our own, eh?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4228));

-- 10206  Bragni Tunnelcut
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 10206, 'The Sandspike Lizards\'ve got breath like a midden heap covered in burnt beard hair. It\'s terrible! It smells worse than Troll, an\' that\'s sayin\' somethin\'.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=10206));

-- 34224  Doragrum Stouthammer
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34224, 'Ya hear that? It\'s the cries of a Bloodspike Howler. The way they carry on of an evenin\' sets my teeth on edge. Haven\'t got more\'n a moment\'s sleep in days.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34224));

-- 34215  Thurn Goldbrewer
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34215, 'I dunno why some of these Gobbos got hoods on, and I don\'t rightly care. Anythin\' that covers up a bit of those ugly little beasties is a good thing, in me humble opinon. Plus, it\'s an extra grabbin\' point for when ya need to throttle \'em.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34215));

-- 10196  Hlom Hrnghamson
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 10196, 'Fish, eh? I\'d be careful of anythin\' that comes from the waters around here. Those River Trolls do some right nasty things to the waters they call home. Last time one of the lads here sampled a bit of the local fish, he turned green as a Gobbo an\' was sick for a week.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=10196));

-- 10201  Mak Kleintrek
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 10201, 'If yer blooded, you\'d best hope you can get to safety right quick. Once a Flesh Hound catches yer scent, he\'ll follow you to the ends of the earth. Sometimes, I wake up in the middle of the night because I dream about the beasts chasin\' me down.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=10201));

-- 1664  Karon Vigridson
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1664, 'Picture a chittering horde of the most virile and putrescent pox. Then give \'em all feet, hands and heads. Now you\'ve got the perfect way to spread sickness \'mongst the cities. Don\'t let their size fool ya. Nurglings might be the most dangerous beasties there are. At least for the sickly manlings, anyway.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1664));

-- 1149  Olfgrond Hammerson
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1149, 'I hear people say things like \'blind as a bat \' when referrin\' to \'em. It\'s clear them folks\'ve never seen a bloody great flying bat come swoopin\' down and rip someone\'s throat out. They don\'t seem to have much trouble findin\' ways to kill you, that\'s for certain.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1149));

-- 6944  Leo Heller
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6944, 'The Wastewater Bandits are every bit as ruthless and cruel as the other monsters we have to deal with. They\'d show ya what yer guts look like for a copper an\' not think a thing of it.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6944));

-- 123528  Rori Mailcutter
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 123528, 'Only manlings could produce somethin\' as perverted as a Ghoul. Twisted, pathetic creatures that feed on their own race\'s flesh. You wouldn\'t see anythin\' like that from a Dwarf, I\'ll tell you that!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=123528));

-- 792  Rafi Throstsson
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 792, 'Ogres is tough. They\'ve got a hide like tanned Rhinox-leather and a disposition to match. Luckily fer us, they\'re easy to lead around. Whether you use their purses or their stomachs to do it, you can pretty much point \'em where you wanna go.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=792));

-- 26300  Slayer Bolgun
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 26300, 'Yer lookin\' fer a fight are ye? Well, I\'ll tell you true, a Venom Gnawer is a good test for one of yer caliber. I barely break a sweat fightin\' em any more. Sometimes, if I\'m feelin\' a bit saucy, I\'ll try fightin\' em with one hand behind me back.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=26300));

-- 10213  Bogni Goblingrinder
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 10213, 'I don\'t like it when them Gobbos get that look in their eyes. You can tell somethin\' bad\'s afoot when they\'s start chantin\' an their eyes get to glowin\'. When that happens, you\'d best hope you\'ve got a Runepriest about, or the Gobbo blows himself up.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=10213));

-- 6343  Hanri Raudson
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6343, 'I saw one of them Crag Boars toss the Orc on its back high into the air, and when the greenskin landed the Boar gored him until he stopped twitchin\'. Then the beast ran off into the woods, saddle and all.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6343));

-- 3517  Ruthgar
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3517, 'An army can only fight as if it\'s well fed. If these trolls ain\'t dealt with directly, we won\'t have nought to eat but salt pork an\' dried berries.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3517));

-- 4643  Dieter Stroh
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4643, 'Rub the leg with some salt and sprinkle it with some rosemary, thyme, an\' whatever other herbs strike yer fancy. \n\n"Then baste it with a dram of oil, an\' roast it over a spit for three hours. Once you\'ve had a bite, you\'ll swear to Sigmar it\'s the best chicken you\'ve ever had."\n\n- Dieter Stroh on Marshtail Salamander meat', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4643));

-- 18851  Dieter Totenhosen
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 18851, 'It seems even the beasts ain\'t follwin\' the nat\'ral order of things any more. It used to be that you could get bit by a spider an\' you\'d expect ta die right quick.\n\n"These days, even the mundane beasts are doing things they ain\'t supposed to. I heard one of them Moss Crawlers bit ol\' Fritz an\' he lingered for nigh on a week before going home to Morr. Norm\'ly that\'d be the end of it. But whatever poison went searin\' through poor Fritz\'s blood also spread to his kin. By the end of the week, they was all dead.\n\n"We had to burn their house to the ground to keep it from spreadin\'.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=18851));

-- 99770  Rolf Grimwold
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 99770, 'You ever seen a barn cat toyin\' wit\' a rat, \'fore he kills it? He ain\'t killin\' that vermin \'cause he\'s \'ungry. He\'s toyin\' wit the li\'l bugger.\n\n"I\'ve seen Sabertusks do the same thing to a man.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=99770));

-- 34725  Vilmer DonHeisen
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34725, 'A lotta folks make the mistake of thinkin\' Ogres is fat. They may look like a bloated white bean, but under that waxy skin, they\'s all muscle.\n\n"I learned that the hard way. The muscles in the beast\'s gullet turned my blade away an\' what was a sure killin\' blow just made the beast bleed out. I couldn\'t get out of the lout\'s way as he fell.\n\n"Nearly lost me leg, I did.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34725));

-- 35375  Archibald Fleck
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 35375, 'Look at this specimen! It\'s clearly decayed too much to be valuable for our studies. We must get fresher kills if we are to truly observe the inner workings of such a creature.\n\n"Fetch me another of these and be quick about it.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=35375));

-- 34703  Borak Brightaxe
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34703, 'The old Dwarf shook his axe for a moment, then stooped and rubbed dirt on the bronze-colored blade to clear it of the Squig\'s remains. They were filthy creatures, and Borak Brightaxe couldn\'t bear to see his father\'s axe sullied with their blood.\n\n"If I ever get out of here, I\'ll find someone else ta kill these beasties for me. It\'s an insult to me Da\'s honor to have his axe stained wit\' the blood of such wee monsters.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34703));

-- 98920  Mary Delarue
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 98920, 'I \'ear their chitterin\' an their disgustin\' furred feet tappin\' on the wood outside my window. They\'s got fangs drippin\' wit\' green ichor what\'ll kill a cow in an instant.\n\n"They took my \'usband, but they\'ll not take me!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=98920));

-- 99771  Barnabas Kuhn
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 99771, 'The wolf is evil\'s herald. Before the forces of Chaos arrived in Reikland, the wolves came in force, as if in anticipation of the feast that was to come. Animals and people are slaughtered wholesale at the feet of the Chaos host, and the wolves never want for food when there\'s a war on.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=99771));

-- 1341  Sigrid Widmann
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1341, 'Bandits, pffft! They\'re nothin\' more than parasites. In fact, they\'s worse than parasites, \'cause they\'s suckin\' blood an\' life from their own kind.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1341));

-- 6234  Hartlieb Roth
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6234, 'We must keep word of our movements from reachin\' the ears of the Raven Host again. They seem to know what are lads\'re up to before we do. They\'ve got to have spies and scouts near the camp.\n\n"Up \'til now, we\'ve lacked the troops to send out after \'em. But now that you Order of the Griffon lads\'ve graced us wit\' yer presence, maybe you can take care of this l\'il problem fer us.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6234));

-- 6223  Danil Balk
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6223, 'We Kislevites\'ve survived worse. An\' we did it without the help of the fellas in Altdorf. We\'ll stave off these bloody Northmen, an\' we\'ll do it together.\n\n"Don\'t worry, we will live to protect your northern borders once again, southerner!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6223));

-- 711  Guildmaster Geoff
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 711, 'What use is havin\' money if ye can\'t live long enough ta spend it? I try to explain this to the lads in the Guild, but they\'re all too wrapped up in ledgers an\' profit margins to understand the scale of this conflict.\n\n"Some of them fled as far away as Tilea an\' Cathay. But if we can\'t stop the spread of Chaos here, it ain\'t gonna be long after afore Tchar\'zanek an\' his host move on to those places, as well.\n\n"If we don\'t pitch in now, we might as well surrender to the darkness.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=711));

-- 15015  Constable Luidheim
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 15015, 'You may have heard the expression, \'An army travels on its stomach\' before. And while that view is a bit simple, it\'s accurate. \n\n"If we can\'t eliminate the threat to the supply lines that the Winter Wolves pose, I fear the campaign to rescue Praag will fail.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=15015));

-- 7335  Quartermaster Randol
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 7335, 'Ahh, coin! \'Tis the great motivator, is it not? Were it not for the power of coin, would any distasteful task ever get done?\n\n"You can force someone to do an unpleasant task, but you\'ll get a surly, unmotivated worker. Offering a bounty on something is the quickest way to ensure it gets done with vigor!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=7335));

-- 6453  Harald Nacht
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6453, 'I know you fancy yerself a tough one, but I\'ll bet you\'ve never been alone in the woods armed wit\' nothin\' but a dull blade an\' hidin\' behind a wall made of the corpses the rest of your unit while a slaverin\' herd o\' Beastmen closed in.\n\n"That\'s what happened to me. I only survived by the good grace of Sigmar. So you\'ll have to forgive me if I\'m a bit skittish when it comes to those monsters.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6453));

-- 194  Aen Windsong
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 194, 'Unforgivable betrayals have created a gulf between our two races which will never be bridged. Until you have known the pain of having to draw a blade against your own brother, I do not think the Empire of Men will fully comprehend the sense of loss I feel each time I must do battle with the Dark Elves.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=194));

-- 1409  Caethsetir Silverstrand
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1409, 'The perfidy of the Dark Elves knows no bounds. Is it not enough that they strike against us? As if their insane quest to destroy our race were notsufficient , they are even making war against the beasts of Ulthuan by introducing dark and twisted creatures that bear the taint of the Witch King and his hatred to our shores.\n\n"Even now, their giant breed of scorpion wreaks havoc amongst the endemic beasts of our home.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1409));

-- 2428  Aeldith Tearsong
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2428, 'The Beastmasters of the Dark Elves are creatures of unspeakable evil. They twist the pure beasts of the forests into warped creatures that live for nothing but killing. \n\n"Some of their \'pets,\' like the Manticore, have always had a predilection for such behavior. I doubt much that the Dark Elves had to do anything at all to sway such beasts to their cause. The mere promise of bloodshed and the ability to sow terror was likely encouragement enough to draw the Manticores into the fold.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2428));

-- 2802  Elthossar Youngstar
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2802, 'These beasts have eight legs, poisoned mandibles, and all the qualities you might associate with a typical spider, but they are creatures of such prodigious size that I would almost hesitate to describe them as such.\n\n"It\'s clear that the taint of Chaos has marked these spiders, causing them grow so large that even a tiny bit of their venom can kill an Elf in seconds. They must be eliminated.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2802));

-- 3078  Celorn Slenderwand
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3078, 'The Haunting Bats may be at home in the darkness, but they are not the only ones who are comfortable moving about in the shadows. \n\n"The Shadow Warriors of Ulthuan walk in the darkness without fear. It is second nature to us.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3078));

-- 3096  Shenassa Blesswind
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3096, 'The Cold Ones are foul-smelling beasts to be sure. Even if they weren\'t too stupid to move stealthily, you can smell the great lizards from leagues away.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3096));

-- 3300  Cirroniol Sureshot
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3300, 'Is there any fate worse than to be held prisoner within your own body? The loss of your will in this manner is among our greatest fears. How can one expect to fend off the temptations of anger and wrath when one\'s very will can be wrested away so easily? \n\n"The Cockatrice can do that, and worse. I will not allow our campaign in Elbisar to be threatened by such mindless beasts, regardless of their powers.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3300));

-- 3957  Quartermaster Aenil
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3957, 'It saddens me that we must cull the ranks of the boars so mercilessly. Our people are in desperate straits, and we must feed them if we are to repel the Dark Elves from our shores. I can only take solace in the knowledge that we do not cause the beasts undue suffering, offering them a swift death and only taking that which we need.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3957));

-- 4153  Seer Jhoril
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4153, 'The Undead are an abomination. No soul should suffer the indignation of being forced to walk again as the pawn of hunger and retribution. I will not tolerate this corruption upon the shores of Ulthuan a moment longer. Put these creatures to rest so that their souls might know peace once more.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4153));

-- 4753  Mesilhin Darkeagle
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4753, 'You can smell the corruption in the air. It is a sickening scent that carries with it the stench of disease. I know not what Malekith\'s minions have unleashed upon the forest of Avelorn, but it is surely a disease most foul. \n\n"The tide of ruin must be stemmed before it overtakes the entire forest. For once Avelorn is fully consumed, Ulthuan is sure to follow.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4753));

-- 5862  Elithaen Leafwind
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5862, 'The Savage Trolls are amongst the deadliest beasts I have yet seen. I have never known any mundane creature so resistant to harm. I have seen them fight through the most grievous of wounds, their mottled green flesh knitting itself together before my very eyes.\n\n"Be very cautious. Carry a brand or a torch with you at all times, for a Troll is never fully dead until it has been reduced to ash.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5862));

-- 34738  Nalrothel Forestshadow
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34738, 'The greenskins are as a tide of locusts. When they rise behind the banner of a powerful warlord, they swarm across the lands to consume everything they can. In their wake, they leave naught but death and destruction. \n\n"We cannot, must not, let Ulthuan suffer such a fate.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34738));

-- 35311  Dekindes Tovliryn
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 35311, 'The menhirs remain threatened. The Dark Elves know not what they do in allying themselves with the Ruinous Powers and engaging in this folly. In the end, whatever dark pacts they have struck with the humans of the north will result in the destruction of this world.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=35311));

-- 36805  Ilfuwyr Cetaine
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 36805, 'I shudder to think of what the kin-traitors could accomplish if they could put aside their petty bickering long enough to act as one. Ironic, is it not, that we can thank Malekith and his draconian leadership for sowing the seeds of mistrust that continue to plague their entire race?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=36805));

-- 1595  Braen Tallofel
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1595, 'Watch as our hated kin continue to march into our ranks like lambs to the slaughter. The front lines look like an abattoir\'s cast-offs and still they throw themselves against us.\n\n"I could almost pity them but for our history.\n\n"Go now, and kill in the name of Khaine – before there are none left to slaughter."\n\n- Karithan Aveth', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1595));

-- 5514  Dinen Lothreull
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5514, 'The Executioners swing their great blades as though they were an extension of their own bodies. Halberdiers cleave their foes in twain. \n\n"Yet, I can kill just as quickly as any of them with this tiny but elegant blade and a measure of Scorpion\'s toxin.\n\n"It hardly seems fair at all – for them."\n\n- Moridyth Cethlan', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5514));

-- 6481  Firithas Lossaryn
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6481, 'The spirits of the wrathful dead walk amongst us. They seek to prevent us from fulfilling our duty to Lord Malekith.\n\n"If need be, we will kill them twice. The only will that matters here is that of the Witch King, and he will not be deterred by ghosts."\n\n- Krathar Dreyalan', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6481));

-- 1781  Celanar Orithwyn
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1781, 'Our offensive is being slowed by bears? Do you mock me? Or do you truly intend to imply that an entire army of the deadliest warriors in the known world are being hindered by mere beasts of the wild?\n\n"It\'s preposterous!"\n\n- Rindhor Swartshield', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1781));

-- 976  Virian Corresyn
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 976, 'At last we approach an age in which we will no longer need fear the ancient Dragons of Ulthuan. Once we have subverted those who will take wing at our side and killed all the rest, the Dragons will cease to be a threat to the machinations of the Witch King.\n\n"I, for one, will be thrilled to see the last of the scaly beasts put to the sword. They are entirely too capricious and headstrong to be counted on as long-term allies."\n\n- Krellian Certhren', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=976));

-- 1063  Lasinon Silverdawn
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1063, 'All that remains of our traitorous kin are the broken remnants of their once-proud army. Being forced to hide in the darkness and strike at our flanks like the vermin they are, would seem a just punishment for their centuries of arrogance. I would like nothing more than to let them seethe and ultimately die away in exile.\n\n"Unfortunately, their small-scale attacks are slowing the army\'s advance, and I cannot allow that to happen. Much as it pains me, we will need to root them out."\n\n- Thandros Uthorin', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1063));

-- 9939  Seladon Thistlewind
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 9939, 'The Harpies have long been our tenuous allies. They were controllable only when offered an alternative enemy to ourselves. Now that our traitorous kin are on the retreat, the Harpies are running out of fresh sources of meat.\n\n"It is only a matter of time before they turn against us, for their numbers are too great to be controlled."\n\n- Telurna Darkveil', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=9939));

-- 9935  Arnien Brightwater
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 9935, 'Let them run! We will find them all no matter what rock they choose to crawl under. I will hunt them to the far corners of the world if need be! Not even the lands of the humans will provide the followers of traitor King solace. \n\n"When the Witch King claims that which is rightfully his, we will march on what is left of the humans as well!"\n\n- Zural Bitterwind', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=9935));

-- 75  Wobna Slipsquig
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 75, 'Squiggies here! Squiggies dere! Squiggies is bouncin\' everywhere! Wot\'s a li\'l Gobbo like me to do?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=75));

-- 2700  Xobz Madgut
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2700, 'Stupid stunties fink dey\'s so smart wif dere big loud guns. We\'ll show dem a fing or two. Too much shootin\' means dey won\'t hear it when we send some Gobbos wot can sneak up on \'em."\n\n- Thundering Xobz', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2700));

-- 3191  Runtskull
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3191, 'Spiders is tasty eatin\'. Dey\'s not like most fings we eat that\'s soft on the outside and full of crunchy bits in the middle. Spiders is crunchy on the outside and squishy in the middle!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3191));

-- 4641  Thudfist
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4641, 'Dem Ghouls isn\'t like any humies wot I seen before. They\'s got claws for one fing, and dey keep findin\' all the food we leave around for eatin\' later.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4641));

-- 4590  Reezel
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4590, 'When I tell da other Gobbos about my Beastmen cape, dey jus\' look at me funny cuz they fink it\'s a waste of tasty gobbets, but bosses is big and dey need a really big cape. It\'s a good plan!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4590));

-- 4198  Sneaks
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4198, 'Dem stunties is lucky. Dey got dem big metal floaty fings dat look way more fun den our barges. The only way I\'d get to go on one of our barges would be if dey make me row.\n\n"I wonder how many stunties are in those floaty fings?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4198));

-- 21002  Moozle
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 21002, 'Fork-tongues ain\'t nearly as fun to bash as stunties is. They\'s not even good eatin\', bein\' all stringy and wotnot. But if you keep bashin\' dem fork-tongues, we\'s got more time to be scrappin\' wif da stunties.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=21002));

-- 8185  Gremgob
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 8185, 'I\'s like it when dem stunties use their big hammers to fight wif. Dey spend so much time swingin\' \'em \'round and wailin\' like a hungry Squig dat you can sneak up behind \'em pretty easy.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=8185));

-- 1112  Shaman Grayfrobba
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1112, 'Da Great Green is gettin\' crowded. All of these hooded Gobbos are talkin\' wif da spirits an\' ghosty boyz wot are supposed to be helpin\' me."\n\n- Shaman Greyfrobba', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1112));

-- 8191  Smashmur
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 8191, 'Stunties is dumb. Dey spend all dat time down in a hole diggin\' up shinies. We Orcs is way smarter than they are. We just wait for \'em to come out of da hole an\' den we bash \'em. Dat way we get a good scrap, and we get to take da shinies. If stunties was smart like us, dey\'d do da same fing, but dey\'s too dumb by half.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=8191));

-- 3691  Grik
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3691, 'Humies don\'t unnerstand how to use dere Arrer Boyz. Dey keep hidin\' in da woods an\' poppin\' out to shoot arrers at us. But den dey run away instead of fightin\'. Dat\'s no fun at all.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3691));

-- 34542  Tog Nailclub
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34542, 'If I wanna show Shatterfang dat I\'m da best wivout fightin\' him meself, I\'ll need ta show his boyz dat he\'s not as good at findin\' good scraps as I am. Da more stunties we kill, da more he\'ll have to yell and scream at his boyz to keep \'em in line. While he\'s busy doin\' dat, we get to kill more stunties and den his ladz\'ll turn on him.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34542));

-- 1706  Kark
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1706, 'I dunno what use dem bats have for me stabba, but I can\'t let \'em just take it. Dem bats\'ll have to be taught a lesson. Jus\' be careful if da one wot\'s got me stabba comes at ya, it was a pretty sharp one.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1706));

-- 5593  Bloodtoof
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5593, 'Droboz don\'t know nuffink. I belong in dat Waaagh! He don\'t unnerstand dat a Waaagh!\'s not just \'bout smashin\' fings. It\'s also \'bout outsmartin\' dat wot you want to smash too. Dat\'s why I need to be dere. I was smart enough to get dese gits to kill stunties for me, weren\'t I?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5593));

-- 6992  Grimor
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6992, 'Humies are a lot like stunties, only dey\'s bigger and less stringy. But stunties got a lot more meat on the bone den humies. Plus, humies squeal a lot more when ya cook \'em, so they\'s more fun. It\'s a matter of taste, is all. An dere beards don\'t taste like coal, neither!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6992));

-- 6776  Gorlub
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6776, 'You littl\'uns fink yer good enough to fight wif me? That\'s funny, that\'s what that is. What makes you fink I even need ya? Tell ya what. Ya kill enough of dem Ghoul fings an\' I\'ll fink about it.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6776));

-- 8511  Gurglesmear
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 8511, 'I need a few enterprising ladz to get rid of some of dese Moltenhide Hounds. Da fings\'ve got teef like stabbas and dey got breaf like a cookfire, so it ain\'t gonna be easy. But, when I get in good wif\' da Shaman, I\'ll put in a good word for ya!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=8511));

-- 21378  Ripgut
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 21378, 'Boars got a temper like a hungry Black Orc. It\'s what makes \'em good in a scrap, but it also makes \'em dangerous.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=21378));

-- 15004  Dragna
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 15004, 'Da stunties keep frowin\' fings at us wif dere big machines. We\'s can make better ones. I need yer help to keep da machines workin\'. Kill me some of dem Snowfury Fork-Tongues. When ya get back, I\'ll tell ya how dat helps wif da machines.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=15004));

-- 8157  Snogrot
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 8157, 'Dat\'s a lotta rats, it is. Normally, I\'d say dere a good fing. Always a handy snack to have lyin\' around. But da boss finks dat many rats might keep us from getting\' inta da stunty keep, so we need to kill some of \'em.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=8157));

-- 748  Vasili Tomerev
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 748, 'The charred ghouls are an affront to the Raven God. They feast upon spoils rightly gathered in tribute to our lord, and they carry plague and disease which may afflict our brethren. They must be eliminated."\n\n- Vasili Tomarev', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=748));

-- 2280  Rangrith
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2280, 'Be on your guard when you wander near the forests. They\'re full of deceptive and spiteful creatures that will lure you into their lairs using any means they can. \n\n"Not that someone such as me would ever fall for such tactics. I\'ve heard … rumors to that effect. Trust neither your eyes nor your ears, and kill whatever you see before it has a chance to deceive you.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2280));

-- 2312  Gottfried Holz
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2312, 'The Beastmen are true children of Chaos. Their gift comes with a price. They are utterly resistant to control, and they cannot be led by anyone other than their own kind. That is not to say they don\'t occasionally make useful tools. It\'s not difficult to use their savage nature against them by leading them by the nose into conflict with one of your enemies. \n\n"Using no more than a few haunches of rancid beef, we once lured an entire herd of Beastmen into battle with an Empire regiment, thus securing our flank.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2312));

-- 99428  Tosha Schreiber
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 99428, 'The Raven God speaks to me in my dreams. He shows me what futures await me if I only give myself over to His will. The power is overwhelming. I can\'t help but feel unworthy when He speaks to me. \n\n"He said that I must earn the right to receive this blessing. He wishes for me to eliminate the wolves from Ostland, so that is what we shall do.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=99428));

-- 5021  Nina
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5021, 'None should fly, save the gods, themselves. Makes it all the harder to kill \'em.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5021));

-- 99436  Harkon Steinkell
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 99436, 'Have a sip of this wine! That\'s some good stuff, yes? Careful! Not so fast! It\'s a brew we make from the blood of the Riptide Monitors that live on the beaches below an\' its some of the strongest stuff I\'ve ever drank.\n\n"You drink enough of this, and you\'ll swear you see the Raven God himself.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=99436));

-- 4437  Bjorn Bulweis
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4437, 'Boars are filthy beasts, always spendin\' their time wallowing in the muck and digging for grubs and other nasty bits! They\'re like giant, snortin\' vermin."\n\n- Fnord Bulweis', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4437));

-- 34896  Reikl Bloodpike
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34896, 'Sure you think you\'re brave now, but can you hold your water when a bear three times your size is bearing down on you? It\'s easy to talk as though you\'re a true warrior when you\'re not being tested.\n\n"Why don\'t you have a go at some of the Faceshredder Bears and see how you do, then?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34896));

-- 35363  Kaltea Wyrmreaver
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 35363, 'How naïve would it be for us to assume that the other denizens of this land would not fight back against our assault? Would the beasts of the Chaos Wastes not fight with tooth, nail, and claw to repel any invasion upon their homes? \n\n"Why then would we not expect the animals here to fight in the same way?', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=35363));

-- 34458  Norrmar Boneblade
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 34458, 'When first you see a full-sized Ogre Bull charge that shakes the earth under your very feet, will you turn tail and run? When you are close enough to smell the rancid breath of the beast and see the piggy blackness of his eyes, will you break? Or will you falter when the Ogre bellows so loudly that your ears bleed?\n\n"These are questions you will only be able to answer once you have faced one yourself."\n\n- Norrmarr Boneblade', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=34458));

-- 1461  Xahiriek
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1461, 'It\'s any wonder that this squalid middenheap of a land is festering with plague and the children of Nurgle. Your species is filthy! When last did you bathe, barbarian?\n\n"Oh how I long for the comforts of home!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1461));

-- 7924  Aesa Heidrdottir
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 7924, 'The weaklings here worship bears! Is that not hilarious? Their god\'s manifestation on this world is a malodorous, vicious, and fat beast that sleeps away half the year. \n\n"No wonder their cities crumble at our feet.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=7924));

-- 6753  Zalir Shadowtalon
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 6753, 'Watching Lord Malekith\'s so-called allies at work is like watching flies struggle to move once they\'ve had their wings pulled out. There\'s a certain earnestness that shines through. \n\n"Ultimately, you\'re not quite sure if they\'ll ever get where they need to be unless a large hand reaches down from the sky, plucks them into the air, and puts them where they\'re bloody well supposed to go.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=6753));

-- 416  Lahkis Curseblade
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 416, 'Oh Terell, will you never learn? What would possess someone to believe that they could tame a beast such as one of these Mountain Lynxes? It\'s madness.\n\n"Perhaps madness is the curse. Maybe that has been what has plagued our family all these years!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=416));

-- 7914  Jarl Spearfist
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 7914, 'You don\'t know the true measure of a man until you see how he stands up in battle, how he reacts to the blood, screams, and the cries for mercy that go unanswered under the din of metal clashing against metal.\n\n"Show these Initiates how a true warrior fights!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=7914));

-- 8442  Amundr The Despoiler
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 8442, 'Tzeentch particularly loves the power granted Him each time a soul devoted to Sigmar is sacrificed to the Changer on the altar of battle. Souls devoted to the southern god are like delicacies to Him as he drinks in their misery. There is nothing so sweet as the suffering of the abandoned."\n\n- Amundr the Despoiler', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=8442));

-- 5793  Gorfaug Manhewer
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5793, 'Waaagh!" shouted the Marauder.\n\nGorfaug Manhewer shook his head sadly. "No! No! No! You say it from back here in da back of yer throat, like dis: \'WAAAGH!\'" The bellow was so loud that people on the other side of the encampment started with fear. "Try \'gain." \n\n"Waaagh!" shouted the Marauder once more.\n\n"Yer still not doin\' it right, humie! You sound like Goblin wots been breathin\' too many Squig spores. It\'s like dis: \'WAAAGH!\'', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5793));

-- 2524  Hakazin Felwind
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2524, 'Slow roasted pork and a stew of bear meat and root vegetables will be the centerpieces of my victory feast. The stew is of an ancient recipe that pre-dates the Sundering. It is a particular favorite of my Lord, General Malagurn.\n\n"Return swiftly, for the meat must be dusted with flour and seared before I can add it to the stew.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2524));

-- 2640  Beastmaster Zakira
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2640, 'As amusing as it is to watch the Cliff Lizards tear apart the prisoners I send along the passes at the vanguard of our army, it makes for slow going. If we cannot speed our passage through their mountain lairs, it is we who will serve as the next meal for the High Elves\' adopted pets.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2640));

-- 2613  Maefyr Fellwhip
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 2613, 'The petrifying gaze of a Cockatrice can paralyze even the hardiest of warriors, allowing the beasts to pick his corpse clean over several agonizing hours.\n\n"I\'ve had the pleasure of watching the creatures do their gory work, and it is exquisite. The victim remains both conscious and aware as the beasts begin their meal. Though paralyzed, the prey is able to feel every bit of pain their razor sharp claws and beaks wreak upon his flesh as the Cockatrices tear it off in bite-sized chunks.\n\n"Unable to move, or even to cry out, the victim\'s mind snaps as he tries desperately to disassociate himself from the pain. We mistakenly rescued a kinsmen from his ordeal. When the paralysis wore off, he was left a mindless, gibbering idiot. It would have been better to let him die.\n\n"Luckily, he was spared the ordeal of seeing the ruin his flesh had become, as the Cockatrices had begun their feast with his eyes.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=2613));

-- 3308  Torrath Ullen
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3308, 'The Shadepaw Wolves are proving to be more trouble than they\'re worth. I have no wish to control a beast that serves no other purpose than to howl like a fisherman\'s widow while I\'m trying to sleep.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3308));

-- 3767  Tracker Drazek Kar
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 3767, 'There is only one way to make a beast follow your every command and that is to instill fear of the highest order within its heart. Our weak-willed cousins spend days, even weeks coaxing the Ellyrion steeds to their stables in order to train them. The beasts are intractable, even stubborn in the face of their fawnings.\n\n"Yet, I\'ve seen a single herd of Ellyrion steeds panic and run at the hint of a Great Cat\'s passage. That is power. If you could harness that fear, the beasts would be easy to break to your will.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=3767));

-- 7297  Taraj Kirak
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 7297, 'When we\'re through with Ellyrion, we will leave it a desolate husk, drained of all its beauty and life just as our accursed kin did to our beloved Nagarythe.\n\n"It will be as though the entire land were overrun by salamanders and burnt to cinders."\n\n- Tarak Kirak', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=7297));

-- 7813  Uzoe Ra'an
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 7813, 'This day, our greatest foes are not our traitorous kin, but rather the beasts and spirits of Avelorn. If we are unable to prevent the forest spirits and their minions from getting word of our movements to the sycophants of the false Phoenix King, House Uthorin will lose face with the Witch King. You can be quite certain that the lackeys of House Arkaneth would not fail to capitalize on such a failure by currying our failure into a boon for their reputation in Malekith\'s court.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=7813));

-- 4821  Garek Zornil
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4821, 'If our cousins believe they can hide the location of the Everqueen from us much longer, they are sorely mistaken. We will find her, and we will carve a path of hewn limbs, blood, and flayed skin as we search.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4821));

-- 4053  Nyethan Blackblade
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 4053, 'If it pleases the kin-traitors to believe they come to battle astride noble beasts then let them continue to delude themselves. Griffons are every bit as bloodthirsty as the Manticores that our treacherous cousins are so quick to label as a product of evil.\n\n"Have you seen a Griffon up close? Such a creature can only be born of Chaos.\n\n"Yet again their hypocrisy is beyond measure.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=4053));

-- 7820  Chorane
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 7820, 'Each time the children of the Liar-King look up into the sky and see one of their beloved Pegasi riding the winds, it gives them a small measure of hope.\n\n"Our victory here would not be complete if that continued to happen.\n\n"The flying nags\'ll have to go.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=7820));

-- 1532  Karithan Aveth
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 1532, 'Watch as our hated kin continue to march into our ranks like lambs to the slaughter. The front lines look like an abattoir\'s cast-offs and still they throw themselves against us.\n\n"I could almost pity them but for our history.\n\n"Go now, and kill in the name of Khaine, before there are none left to slaughter.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=1532));

-- 5963  Krathar Dreyalan
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5963, 'The spirits of the wrathful dead walk amongst us. They seek to prevent us from fulfilling our duty to Lord Malekith.\n\n"If need be, we will kill them twice. The only will that matters here is that of the Witch King, and he will not be deterred by ghosts.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5963));

-- 5953  Rindhor Swartshield
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 5953, 'Our offensive is being slowed by bears? Do you mock me? Or do you truly intend to imply that an entire army of the deadliest warriors in the known world are being hindered by mere beasts of the wild?\n\n"It\'s preposterous!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=5953));

-- 7735  Telurna Darkveil
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 7735, 'The Harpies have long been our tenuous allies. They were controllable only when offered an alternative enemy to ourselves. Now that our traitorous kin are on the retreat, the Harpies are running out of fresh sources of meat.\n\n"It is only a matter of time before they turn against us, for their numbers are too great to be controlled.', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=7735));

-- 42473  Zural Bitterwind
INSERT INTO `creature_texts` (`Entry`,`Text`,`creature_texts_ID`)
  SELECT 42473, 'Let them run! We will find them all no matter what rock they choose to crawl under. I will hunt them to the far corners of the world if need be! Not even the lands of the humans will provide the followers of traitor King solace.\n\n"When the Witch King claims that which is rightfully his, we will march on what is left of the humans as well!', UUID()
  WHERE NOT EXISTS (SELECT 1 FROM (SELECT 1) AS x WHERE EXISTS (SELECT 1 FROM `creature_texts` WHERE `Entry`=42473));


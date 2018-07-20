CREATE EVENT characters_value_24hr 
ON SCHEDULE EVERY 1 hour
    DO 
INSERT INTO war_characters.characters_value_24hr(characterId, Level, Xp, RenownRank, Money, timestamp)
SELECT c.characterId, Level, Xp, RenownRank, Money, now() from war_characters.characters_value cv, war_characters.characters c, war_accounts.accounts a
where a.accountId = c.accountId and c.characterId = cv.characterId and a.gmlevel <= 1;
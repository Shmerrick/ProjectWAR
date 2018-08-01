SET GLOBAL event_scheduler = ON;

DROP EVENT `Character_hourly_dump`;

CREATE EVENT `Character_hourly_dump` ON SCHEDULE
        EVERY 1 hour
    DO 
INSERT INTO war_characters.characters_value_24hr(characterId, Level, Xp, RenownRank, Money, timestamp)
SELECT characterId, Level, Xp, RenownRank, Money, now() from war_characters.characters_value;


SHOW EVENTS

select * from war_characters.characters_value_24hr ORDER BY TIMESTAMP DESC

SHOW VARIABLES
WHERE VARIABLE_NAME = 'event_scheduler'
SELECT * FROM INFORMATION_SCHEMA.events;
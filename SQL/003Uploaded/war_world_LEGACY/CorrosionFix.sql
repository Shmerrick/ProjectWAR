#old value -30
update war_world.buff_commands T set  T.SecondaryValue = -1500 where T.Entry = 10699;
#old value -1500
update war_world.buff_commands T set  T.TertiaryValue= NULL where T.Entry = 10699;
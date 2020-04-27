
#Inserts lvl RR 29 weapons into the vendor in altdorf
Insert into war_world.vendor_items  values (252,5500565,0,0,0,'(30,208433)');
Insert into war_world.vendor_items  values (252,5500535,0,0,0,'(30,208433)');
Insert into war_world.vendor_items  values (252,5500551,0,0,0,'(30,208433)');
Insert into war_world.vendor_items  values (252,5500465,0,0,0,'(30,208433)');

update war_world.vendor_items set ReqItems = '(30,208470)' where VendorId=252 and ItemId=5500565;
update war_world.vendor_items set ReqItems = '(30,208470)'   where VendorId=252 and ItemId=5500535;
update war_world.vendor_items set ReqItems = '(30,208470)'   where VendorId=252 and ItemId=5500551;
update war_world.vendor_items set ReqItems = '(30,208470)'   where VendorId=252 and ItemId=5500465;
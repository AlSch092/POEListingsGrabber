# POEListingsGrabber
Grabs all listings from poe.trade for a given item and parameters.

Usage:

./ListingGrabber league itemName

-OR-

./ListingGrabber inputFile

an Input file has the following structure:

ServerName ItemName type= base= dmg_min= dmg_max= aps_min= aps_max= crit_min= crit_max= dps_min= dps_max= edps_min= edps_max= pdps_min= pdps_max= armour_min= armour_max= evasion_min= evasion_max= shield_min= shield_max= block_min= block_max= sockets_min= sockets_max= link_min= link_max= sockets_r= sockets_g= sockets_b= sockets_w= linked_r= linked_g= linked_b= linked_w= rlevel_min= rlevel_max= rstr_min= rstr_max= rdex_min= rdex_max= rint_min= rint_max= mod_name= mod_min= mod_max= mod_weight= group_type=And group_min= group_max= group_count=1 q_min= q_max= level_min= level_max= ilvl_min= ilvl_max= rarity= progress_min= progress_max= sockets_a_min= sockets_a_max= map_series= altart= identified= corrupted= crafted= enchanted= fractured= synthesised= mirrored= veiled= shaper= elder= crusader= redeemer= hunter= warlord= replica= seller= thread= online=x capquality=x buyout_min= buyout_max= buyout_currency= has_buyout=1 exact_currency=x

...

Each line of the file is a parameter in the search body in a web request. Some of these parameters take either '1' or 'x' as the correct value, which is decided by their web server at poe.trade. I cannot help with these.

TO-DO: Add functionalities for multiple stacked items, like 5 chaos orbs instead of just 1 chaos orb (requires using 'currency market' on poe.trade)
